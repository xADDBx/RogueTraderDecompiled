using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Core.Cheats;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Net;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Sound;
using Kingmaker.UI;
using Kingmaker.UI.Canvases;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Models.SettingsUI.UISettingsSheet;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;

namespace Kingmaker.View;

public class CameraRig : MonoBehaviour, IAreaHandler, ISubscriber, IAdditiveAreaSwitchHandler, IGameModeHandler, IDialogStartHandler, IDialogFinishHandler, IFullScreenUIHandler, IFullScreenUIHandlerWorkaround
{
	private sealed class ShadowDistanceOverride : OverridableValue<float>.ISource
	{
		private const int kPriority = 0;

		private float m_ShadowDistance;

		private bool m_ShadowDistanceSpecified;

		private bool m_Enabled;

		private bool m_Registered;

		float OverridableValue<float>.ISource.Value => m_ShadowDistance;

		public void SetShadowDistance(float value)
		{
			m_ShadowDistance = value;
			m_ShadowDistanceSpecified = true;
			Refresh();
		}

		public void SetEnabled(bool value)
		{
			m_Enabled = value;
			Refresh();
		}

		private void Refresh()
		{
			bool flag = m_Enabled && m_ShadowDistanceSpecified;
			if (m_Registered == flag)
			{
				return;
			}
			WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
			if ((object)asset != null)
			{
				if (flag)
				{
					asset.ShadowSettings.ShadowDistance.AddOverride(this, 0);
				}
				else
				{
					asset.ShadowSettings.ShadowDistance.RemoveOverride(this);
				}
				m_Registered = flag;
			}
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("CameraRig");

	[Header("Main Camera")]
	public bool NewBehaviour;

	public bool NewBehaviour2;

	private const int GroundMask = 4194576;

	[SerializeField]
	private Transform m_CameraAttachPoint;

	[SerializeField]
	public Transform CameraGlobalMapAttachPoint;

	[SerializeField]
	public Transform CameraStarSystemAttachPoint;

	[SerializeField]
	private Transform m_CameraMapAttachPoint;

	[SerializeField]
	private Transform m_AudioListener;

	private Vector3? m_AttackPointPos;

	[SerializeField]
	private GameObject m_MapLight;

	[SerializeField]
	[InspectorReadOnly]
	private Bounds m_LevelBounds;

	private Vector3 m_TargetPosition;

	private Vector3 m_TargetRotate;

	private Vector2 m_RotateDistance;

	private Vector2 m_ViewportOffset;

	private readonly ShakeSettings m_Shake = new ShakeSettings();

	private Coroutine m_ScrollRoutine;

	private Coroutine m_RotateRoutine;

	private Vector2 m_ScrollBy2D;

	private readonly List<CameraShakeFx> m_ShakeFxList = new List<CameraShakeFx>();

	private Vector3 m_AnchorShakeOffset;

	private Vector3 m_CameraShakeOffset;

	private Vector3? m_BaseMousePoint;

	private RaycastHit[] m_ReuseHits = new RaycastHit[16];

	private bool m_ShouldPlaceOnGround;

	[Tooltip("Прожектор из камеры для дополнительного освещения окружения. Deprecated, осталось от PF2, на WH не используется. Настраивалось в LightController")]
	public Light CameraLight;

	[Tooltip("Контурное освещение для персонажей, которое вертится вслед за камерой. Настраивается в LightController")]
	public Light CharacterContourLight;

	[Space(10f)]
	[Header("Surface camera settings")]
	[Tooltip("Enables vertical camera rotation. Only for space combat.")]
	public bool EnableSurfaceOrbitCamera = true;

	public float MinSurfaceCameraAngle = -20f;

	public float MaxSurfaceCameraAngle = 15f;

	[Space(10f)]
	[Header("Action Camera")]
	public GameObject ActionCameraRoot;

	[Space(10f)]
	[Header("Additional space combat camera settings")]
	[Tooltip("Enables vertical camera rotation. Only for space combat.")]
	public bool EnableSpaceOrbitCamera = true;

	public float MinSpaceCameraAngle = -20f;

	public float MaxSpaceCameraAngle = 15f;

	private bool m_EnableOrbitCamera;

	[Tooltip("Enable camera zoom by moving it along the Z axis. Only for space combat.")]
	public bool EnablePhysicalZoom = true;

	public float ZoomMin;

	public float ZoomMax = 15f;

	private int m_DisableCounter;

	private float m_DisableTimer;

	private bool m_SkipClampOneFrame;

	private float m_PrevSmoothedDt;

	private float m_ScrollRoutineEndsOn;

	private Vector3 m_ScrollRoutineEndPos;

	private float m_RotateRoutineEndsOn;

	private bool m_RotationByMouse;

	private bool m_RotationByKeyboard;

	private bool m_HandRotationLock;

	private bool m_HandScrollLock;

	private bool m_RecordLock;

	private Matrix4x4 m_WorldToClipMatrixCached;

	private FullScreenUIType m_FullScreenUIType;

	private readonly ShadowDistanceOverride m_ShadowDistanceOverride = new ShadowDistanceOverride();

	private bool m_HardBindPositionEnabled;

	private Vector3 m_HardBindPosition = Vector3.negativeInfinity;

	private Vector2 m_ScrollOffset;

	private float m_RotateOffset;

	private bool m_ReportedNaNPos;

	private bool m_ReportedNaNBasis;

	private Vector3? m_LastHighGroundPos;

	private float m_LastDistToHighGroundPos;

	private float m_LowerSpeedY;

	private bool m_Lowering;

	private float m_TimeToGroundMs;

	public static CameraRig Instance { get; private set; }

	[Cheat(Name = "Debug_Camera_Scroll")]
	public static bool DebugCameraScroll { get; set; }

	[Cheat(Name = "camera_scroll_mod")]
	public static float ConsoleScrollMod { get; set; } = 1f;


	[Cheat(Name = "camera_rotation_mod")]
	public static float ConsoleRotationMod { get; set; } = 1f;


	public Transform CameraAttachPoint
	{
		get
		{
			if (!(Game.Instance?.CurrentlyLoadedArea?.AreaStatGameMode == GameModeType.GlobalMap))
			{
				if (!(Game.Instance?.CurrentlyLoadedArea?.AreaStatGameMode == GameModeType.StarSystem))
				{
					return m_CameraAttachPoint;
				}
				return CameraStarSystemAttachPoint;
			}
			return CameraGlobalMapAttachPoint;
		}
	}

	public float ScrollRubberBand { get; set; }

	private float ScrollScreenThreshold { get; set; }

	private float ScrollSpeed { get; set; }

	private float RotationSpeed { get; set; }

	private float RotationRubberBand { get; set; }

	private float RotationTime { get; set; }

	private float m_RotationRatio { get; set; }

	public Vector3? SavedPosition { get; set; }

	public float SavedRotation { get; set; }

	public Camera Camera { get; private set; }

	public CameraZoom CameraZoom { get; private set; }

	public Vector3 Up { get; private set; }

	public Vector3 Right { get; private set; }

	public bool NoClamp { get; set; }

	public CountingGuard FixCamera { get; set; } = new CountingGuard();


	private bool IsSpaceGameMode
	{
		get
		{
			if (!(Game.Instance?.CurrentMode == GameModeType.GlobalMap))
			{
				return Game.Instance?.CurrentMode == GameModeType.StarSystem;
			}
			return true;
		}
	}

	public bool RotationByMouse => m_RotationByMouse;

	public Vector3 TargetPosition => m_TargetPosition;

	public Transform ListenerUpdater => m_AudioListener;

	public bool IsScrollingByRoutine => m_ScrollRoutine != null;

	public bool IsScrollingByRoutineSynced
	{
		get
		{
			foreach (PlayerCommands<SynchronizedData> player in Game.Instance.SynchronizedDataController.SynchronizedData.Players)
			{
				foreach (SynchronizedData command in player.Commands)
				{
					Kingmaker.Controllers.Net.CameraData camera = command.camera;
					if (camera != null && camera.isScrollingByRoutine)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool OnLevelBound { get; private set; }

	public bool DontForceLookAtTarget { get; set; }

	public GameObject MapLight => m_MapLight;

	public float TimeToGround { get; set; }

	public float DistanceFromHighGround { get; set; }

	[Cheat(Name = "Is_Shaking", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static bool IsShakingCheat
	{
		get
		{
			return Instance.m_Shake.Active;
		}
		set
		{
			Instance.m_Shake.Active = value;
		}
	}

	public void OnAreaBeginUnloading()
	{
		ResetCamera();
	}

	public void OnAreaDidLoad()
	{
		SetupCamera();
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		ResetCamera();
	}

	public void OnAdditiveAreaDidActivated()
	{
		SetupCamera();
	}

	public void ResetCamera()
	{
		Camera.transform.localPosition = Vector3.zero;
		Camera.transform.localRotation = Quaternion.identity;
		m_TargetRotate = Vector3.zero;
	}

	public void SetupCamera()
	{
		if (SavedPosition.HasValue)
		{
			ScrollToImmediately(SavedPosition.Value);
			Vector3 eulerAngles = base.transform.eulerAngles;
			eulerAngles.y = SavedRotation;
			eulerAngles.x = 0f;
			base.transform.eulerAngles = eulerAngles;
			m_TargetRotate = eulerAngles;
			SavedPosition = null;
		}
		else
		{
			Vector3 eulerAngles2 = base.transform.eulerAngles;
			eulerAngles2.x = 0f;
			base.transform.eulerAngles = eulerAngles2;
			m_TargetRotate = eulerAngles2;
			BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
			if (currentlyLoadedArea != null && currentlyLoadedArea.IsPartyArea)
			{
				ScrollToImmediately(Game.Instance.Player.MainCharacter.Entity.Position);
			}
		}
		m_LevelBounds = (from c in UnityEngine.Object.FindObjectsOfType<Collider>()
			where c.gameObject.layer == 8
			select c).Aggregate(default(Bounds), delegate(Bounds b, Collider c)
		{
			b.Encapsulate(c.bounds);
			return b;
		});
		ResetCurrentModeSettings();
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.SpaceCombat)
		{
			m_EnableOrbitCamera = EnableSpaceOrbitCamera;
		}
		else if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.Default)
		{
			m_EnableOrbitCamera = EnableSurfaceOrbitCamera;
		}
		else
		{
			m_EnableOrbitCamera = false;
		}
	}

	public Vector2 WorldToViewport(Vector3 point)
	{
		Matrix4x4 worldToClipMatrixCached = m_WorldToClipMatrixCached;
		return (worldToClipMatrixCached.MultiplyPoint(point) + Vector3.one) * 0.5f;
	}

	public static Vector2 WorldToViewportMainCamera(Vector3 point)
	{
		Camera main = Camera.main;
		if (main == null)
		{
			return default(Vector2);
		}
		return ((main.projectionMatrix * main.worldToCameraMatrix).MultiplyPoint(point) + Vector3.one) * 0.5f;
	}

	public void ChangeListenerParent(Transform parent = null)
	{
		parent = (parent ? parent : CameraAttachPoint);
		if (!(parent != null) || parent.gameObject.activeInHierarchy)
		{
			Quaternion localRotation = m_AudioListener.localRotation;
			Vector3 localPosition = m_AudioListener.localPosition;
			m_AudioListener.SetParent(parent);
			m_AudioListener.localRotation = localRotation;
			m_AudioListener.localPosition = localPosition;
		}
	}

	public void ResetCurrentModeSettings()
	{
		BlueprintCameraSettings blueprintCameraSettings = Game.Instance.CurrentlyLoadedArea?.CameraSettings ?? ((BlueprintCameraSettings)BlueprintRoot.Instance.CameraRoot.GroundMapSettings);
		if (!CameraZoom)
		{
			CameraZoom = GetComponent<CameraZoom>();
		}
		if (!Camera)
		{
			Camera = GetComponentInChildren<Camera>();
		}
		CameraZoom.FovMin = blueprintCameraSettings.FovMin;
		CameraZoom.FovMax = blueprintCameraSettings.FovMax;
		CameraZoom.FovDefault = blueprintCameraSettings.FovDefault;
		CameraZoom.PlayerScrollPosition = CameraZoom.FovDefaultNormalized;
		CameraZoom.ZoomLength = blueprintCameraSettings.ZoomLength;
		CameraZoom.Smoothness = blueprintCameraSettings.ZoomSmoothness;
		CameraZoom.EnablePhysicalZoom = blueprintCameraSettings.EnablePhysicalZoom;
		CameraZoom.PhysicalZoomMin = blueprintCameraSettings.PhysicalZoomMin;
		CameraZoom.PhysicalZoomMax = blueprintCameraSettings.PhysicalZoomMax;
		ScrollRubberBand = blueprintCameraSettings.ScrollRubberBand;
		ScrollScreenThreshold = blueprintCameraSettings.ScrollScreenThreshold;
		ScrollSpeed = blueprintCameraSettings.ScrollSpeed;
		RotationRubberBand = blueprintCameraSettings.RotationRubberBand;
		RotationSpeed = blueprintCameraSettings.RotationSpeed;
		RotationTime = blueprintCameraSettings.RotationTime;
		m_RotationRatio = blueprintCameraSettings.RotationRatio;
		TimeToGround = blueprintCameraSettings.TimeToGround;
		DistanceFromHighGround = blueprintCameraSettings.DistanceFromHighGround;
		Camera.transform.SetParent(CameraAttachPoint);
		Camera.fieldOfView = CameraZoom.FovDefault;
		SetAttachPoint(CameraAttachPoint);
		if (blueprintCameraSettings.OverrideStartPosition)
		{
			base.transform.position = blueprintCameraSettings.StartPosition;
		}
		m_HardBindPositionEnabled = blueprintCameraSettings.HardBindPositionEnabled;
		m_HardBindPosition = blueprintCameraSettings.HardBindPosition;
		if (blueprintCameraSettings.OverrideStartRotation)
		{
			base.transform.rotation = Quaternion.Euler(blueprintCameraSettings.StartRotation);
		}
		m_ShadowDistanceOverride.SetShadowDistance(blueprintCameraSettings.ShadowDistance);
	}

	public void SetAttachPoint(Transform attachPoint)
	{
		Camera.transform.parent = attachPoint.transform;
		ChangeListenerParent(attachPoint);
		Camera.transform.localPosition = Vector3.zero;
		Camera.transform.localRotation = Quaternion.identity;
		UpdateListener(attachPoint);
	}

	private void OnValidate()
	{
		if (m_CameraAttachPoint == null)
		{
			PFLog.Default.Warning("Camera Rig needs camera attach point set up", this);
			m_CameraAttachPoint = base.transform;
		}
	}

	public void RenewKeyboardBindings()
	{
		if (base.isActiveAndEnabled)
		{
			ApplyBindAction(delegate(string s, Action action)
			{
				Game.Instance.Keyboard.Bind(s, action);
			});
		}
	}

	private void OnEnable()
	{
		EventBus.Subscribe(this);
		Camera = GetComponentInChildren<Camera>();
		CameraZoom = GetComponent<CameraZoom>();
		RenewKeyboardBindings();
		AudioListenerPositionController[] componentsInChildren = GetComponentsInChildren<AudioListenerPositionController>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
		m_ShadowDistanceOverride.SetEnabled(value: true);
		CinemachineCore.CameraUpdatedEvent.AddListener(OnCinemachineUpdate);
	}

	private void OnCinemachineUpdate(CinemachineBrain brain)
	{
		if (Camera != null && brain.OutputCamera == Camera)
		{
			m_WorldToClipMatrixCached = Camera.projectionMatrix * Camera.worldToCameraMatrix;
		}
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
		KeyboardAccess keyboard = Game.Instance.Keyboard;
		ApplyBindAction(keyboard.Unbind);
		m_ShadowDistanceOverride.SetEnabled(value: false);
		CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCinemachineUpdate);
	}

	private void UpdateListener(Transform newRoot)
	{
		m_AudioListener.gameObject.SetActive(value: false);
		m_AudioListener = newRoot.GetComponentInChildren<AudioListenerPositionController>(includeInactive: true)?.transform ?? m_AudioListener;
		m_AudioListener.gameObject.SetActive(value: true);
	}

	private void ApplyBindAction(Action<string, Action> bindAction)
	{
		UIKeybindGeneralSettings uIKeybindGeneralSettings = UISettingsRoot.Instance.UIKeybindGeneralSettings;
		if (uIKeybindGeneralSettings != null && !(uIKeybindGeneralSettings.CameraLeft == null) && !(uIKeybindGeneralSettings.CameraRight == null) && !(uIKeybindGeneralSettings.CameraUp == null) && !(uIKeybindGeneralSettings.CameraDown == null) && !(uIKeybindGeneralSettings.CameraRotateLeft == null) && !(uIKeybindGeneralSettings.CameraRotateRight == null))
		{
			bindAction(uIKeybindGeneralSettings.CameraLeft.name, AddLeft);
			bindAction(uIKeybindGeneralSettings.CameraRight.name, AddRight);
			bindAction(uIKeybindGeneralSettings.CameraUp.name, AddUp);
			bindAction(uIKeybindGeneralSettings.CameraDown.name, AddDown);
			bindAction(uIKeybindGeneralSettings.CameraRotateLeft.name, RotateLeft);
			bindAction(uIKeybindGeneralSettings.CameraRotateRight.name, RotateRight);
		}
	}

	private void AddLeft()
	{
		float num = (float)SettingsRoot.Controls.CameraScrollSpeedKeyboard * 0.02f;
		m_ScrollOffset += new Vector2(0f - num, 0f);
	}

	private void AddRight()
	{
		float x = (float)SettingsRoot.Controls.CameraScrollSpeedKeyboard * 0.02f;
		m_ScrollOffset += new Vector2(x, 0f);
	}

	private void AddUp()
	{
		float y = (float)SettingsRoot.Controls.CameraScrollSpeedKeyboard * 0.02f;
		m_ScrollOffset += new Vector2(0f, y);
	}

	private void AddDown()
	{
		float num = (float)SettingsRoot.Controls.CameraScrollSpeedKeyboard * 0.02f;
		m_ScrollOffset += new Vector2(0f, 0f - num);
	}

	public void RotateLeft()
	{
		if (!m_RotationByMouse && !m_RotationByKeyboard)
		{
			if (IsSpaceGameMode)
			{
				m_RotationByKeyboard = false;
				m_RotateOffset = 0f;
			}
			else
			{
				m_RotateOffset = m_RotationRatio * (float)SettingsRoot.Controls.CameraRotationSpeedKeyboard * 0.05f;
				m_RotationByKeyboard = true;
			}
		}
	}

	public void RotateRight()
	{
		if (!m_RotationByMouse && !m_RotationByKeyboard)
		{
			if (IsSpaceGameMode)
			{
				m_RotationByKeyboard = false;
				m_RotateOffset = 0f;
			}
			else
			{
				m_RotateOffset = (0f - m_RotationRatio) * (float)SettingsRoot.Controls.CameraRotationSpeedKeyboard * 0.05f;
				m_RotationByKeyboard = true;
			}
		}
	}

	private void Update()
	{
		if (m_DisableCounter > 0)
		{
			m_DisableTimer -= Time.unscaledDeltaTime;
			if (m_DisableTimer < 0f)
			{
				Camera.gameObject.SetActive(value: false);
				return;
			}
		}
		if (!LoadingProcess.Instance.IsLoadingInProcess)
		{
			UpdateInternal();
		}
	}

	public void UpdateForce()
	{
		UpdateInternal();
	}

	public void SwitchOff(float timeout)
	{
		if (m_DisableCounter == 0)
		{
			m_DisableTimer = timeout;
		}
		m_DisableCounter++;
	}

	public void SwitchOn()
	{
		if (m_DisableCounter <= 0)
		{
			PFLog.Default.Warning("Trying to switch on camera rig when it's not off");
			return;
		}
		m_DisableCounter--;
		if (m_DisableCounter == 0)
		{
			m_DisableTimer = 0f;
			Camera.gameObject.SetActive(value: true);
		}
	}

	private void UpdateInternal()
	{
		m_SkipClampOneFrame = false;
		if (m_HardBindPositionEnabled)
		{
			base.transform.position = m_HardBindPosition + GetViewportOffset();
			m_WorldToClipMatrixCached = Camera.projectionMatrix * Camera.worldToCameraMatrix;
			return;
		}
		float debugTimeScale = Game.Instance.TimeController.DebugTimeScale;
		float num = Mathf.Min(m_PrevSmoothedDt = Mathf.Lerp(m_PrevSmoothedDt, Time.unscaledDeltaTime, 0.2f), 0.5f) * Mathf.Clamp(debugTimeScale, 1f, 4f);
		Quaternion b = Quaternion.Euler(m_TargetRotate);
		bool num2 = Mathf.Abs(Quaternion.Dot(base.transform.rotation, b)) < 1f;
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, num * RotationRubberBand);
		bool flag = num2 && m_ViewportOffset != Vector2.zero;
		base.transform.position = Vector3.Lerp(base.transform.position, m_TargetPosition + GetViewportOffset() - m_AnchorShakeOffset, flag ? 1f : (num * ScrollRubberBand));
		if (float.IsNaN(base.transform.position.sqrMagnitude))
		{
			ScrollToImmediately(Game.Instance.Player?.MainCharacter.Get()?.Position ?? Vector3.zero);
			if (!m_ReportedNaNPos)
			{
				m_ReportedNaNPos = true;
				PFLog.Default.Error("Camera position is NaN");
			}
		}
		m_WorldToClipMatrixCached = Camera.projectionMatrix * Camera.worldToCameraMatrix;
	}

	public void SetViewportOffset(Vector2 viewportOffset)
	{
		m_ViewportOffset = viewportOffset;
	}

	private Vector3 GetViewportOffset()
	{
		return m_ViewportOffset;
	}

	public Vector3 ClampByLevelBounds(Vector3 point)
	{
		Bounds bounds = Game.Instance.CurrentlyLoadedAreaPart?.Bounds.CameraBounds ?? m_LevelBounds;
		OnLevelBound = point.x > bounds.max.x || point.x < bounds.min.x || point.z > bounds.max.z || point.z < bounds.min.z;
		point.x = Mathf.Min(Mathf.Max(point.x, bounds.min.x), bounds.max.x);
		point.z = Mathf.Min(Mathf.Max(point.z, bounds.min.z), bounds.max.z);
		return point;
	}

	private Vector2 GetCameraScrollShiftByMouse()
	{
		Vector2 vector = Input.mousePosition;
		float num = (float)SettingsRoot.Controls.CameraScrollSpeedEdge * 0.02f;
		bool flag = (bool)SettingsRoot.Controls.CameraScrollOutOfScreenEnabled && m_FullScreenUIType == FullScreenUIType.Unknown;
		Vector2 vector2 = vector;
		int num2;
		if (vector2.x < ScrollScreenThreshold && (flag || vector2.x >= 0f))
		{
			num2 = -1;
		}
		else
		{
			Vector2 vector3 = vector2;
			num2 = ((vector3.x > (float)Screen.width - ScrollScreenThreshold && (flag || vector3.x <= (float)Screen.width)) ? 1 : 0);
		}
		int num3 = num2;
		Vector2 vector4 = vector;
		if (vector4.y < ScrollScreenThreshold && (flag || vector4.y >= 0f))
		{
			num2 = -1;
		}
		else
		{
			Vector2 vector5 = vector4;
			num2 = ((vector5.y > (float)Screen.height - ScrollScreenThreshold && (flag || vector5.y <= (float)Screen.height)) ? 1 : 0);
		}
		int num4 = num2;
		if (num3 != 0 || num4 != 0)
		{
			Game.Instance.CameraController?.Follower?.Release();
		}
		return new Vector2((float)num3 * num, (float)num4 * num);
	}

	public void TickScroll()
	{
		float num = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
		if (m_ScrollRoutine != null && Time.time > m_ScrollRoutineEndsOn)
		{
			StopCoroutine(m_ScrollRoutine);
			m_ScrollRoutine = null;
			Vector3 targetPosition = PlaceOnGround(m_ScrollRoutineEndPos);
			base.transform.position = (m_TargetPosition = targetPosition);
		}
		if (m_HandScrollLock || m_RecordLock)
		{
			return;
		}
		if (m_ScrollRoutine == null)
		{
			Vector2 scrollOffset = m_ScrollOffset;
			bool flag = m_FullScreenUIType != FullScreenUIType.Unknown;
			if ((!Application.isEditor || DebugCameraScroll) && !Game.Instance.IsControllerGamepad && (bool)SettingsRoot.Controls.ScreenEdgeScrolling && !flag && Application.isFocused)
			{
				scrollOffset += GetCameraScrollShiftByMouse();
			}
			scrollOffset += m_ScrollBy2D;
			m_ScrollBy2D.Set(0f, 0f);
			if (scrollOffset == Vector2.zero)
			{
				return;
			}
			Game.Instance.CameraController?.Follower?.Release();
			float cameraScrollMultiplier = Game.Instance.CurrentlyLoadedArea.CameraScrollMultiplier;
			scrollOffset *= ScrollSpeed * num * cameraScrollMultiplier * ConsoleScrollMod;
			FigureOutScreenBasis();
			Vector3 prevPos = m_TargetPosition;
			m_TargetPosition += scrollOffset.x * Right + scrollOffset.y * Up;
			EventBus.RaiseEvent(delegate(ICameraMovementHandler h)
			{
				h.HandleCameraTransformed(Vector3.Dot(prevPos, m_TargetPosition));
			});
			if (!NoClamp && !m_SkipClampOneFrame)
			{
				m_TargetPosition = ClampByLevelBounds(m_TargetPosition);
			}
			m_TargetPosition = PlaceOnGround(m_TargetPosition);
			if (NewBehaviour)
			{
				if (!m_AttackPointPos.HasValue)
				{
					m_AttackPointPos = Camera.transform.parent.localPosition;
				}
				m_TargetPosition = LowerGently(prevPos, m_TargetPosition, num);
			}
		}
		m_ScrollOffset = Vector2.zero;
	}

	private void FigureOutScreenBasis()
	{
		Right = Vector3.ProjectOnPlane(Camera.transform.right, Vector2.up).normalized;
		Vector3 vector = ((Mathf.Abs(Vector3.Dot(Camera.transform.up, Vector2.up)) < Mathf.Epsilon) ? Camera.transform.forward : Camera.transform.up);
		Up = Vector3.ProjectOnPlane(vector, Vector2.up).normalized;
		bool flag = false;
		if (float.IsNaN(Right.sqrMagnitude))
		{
			Right = Vector3.right;
			flag = true;
		}
		if (float.IsNaN(Up.sqrMagnitude))
		{
			Up = Vector3.forward;
			flag = true;
		}
		if (flag && !m_ReportedNaNBasis)
		{
			m_ReportedNaNBasis = true;
			PFLog.Default.Error($"Up or Right vector are NaN in CameraRig. Camera.transform.position={Camera.transform.position}");
		}
	}

	private Vector2 GetLocalPointerPosition()
	{
		if (!MainCanvas.Instance)
		{
			return Input.mousePosition;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle(MainCanvas.Instance.RectTransform, Input.mousePosition, UICamera.Instance, out var localPoint);
		return localPoint;
	}

	private void RotateByMiddleButton()
	{
		if (!PointerController.InGui && Input.GetMouseButtonDown(2) && !m_RotationByMouse && !m_RotationByKeyboard)
		{
			m_RotationByMouse = true;
			Game.Instance.CursorController.SetRotateCameraCursor(state: true);
			m_BaseMousePoint = GetLocalPointerPosition();
			m_RotateDistance = Vector2.zero;
			m_TargetRotate = base.transform.eulerAngles;
		}
	}

	private void CheckRotate()
	{
		if (!Input.GetMouseButton(2) && m_RotationByMouse)
		{
			Game.Instance.CursorController.SetRotateCameraCursor(state: false);
			m_BaseMousePoint = null;
			m_RotationByMouse = false;
		}
	}

	public void TickRotate()
	{
		CheckRotate();
		if (m_RotateRoutine != null && Time.time > m_RotateRoutineEndsOn)
		{
			StopCoroutine(m_RotateRoutine);
			m_RotateRoutine = null;
		}
		if (m_ScrollRoutine != null || m_RotateRoutine != null || m_HandRotationLock || m_RecordLock)
		{
			return;
		}
		RotateByMiddleButton();
		Vector2 vector = Vector2.zero;
		if (m_RotationByMouse)
		{
			vector = CameraDragToRotate();
		}
		else if (m_RotationByKeyboard)
		{
			vector.x = m_RotateOffset;
		}
		if (m_RotationByMouse || m_RotationByKeyboard)
		{
			Vector3 target = base.transform.rotation.eulerAngles;
			target.y += vector.x * RotationSpeed * ConsoleRotationMod;
			if (m_EnableOrbitCamera)
			{
				target.x += vector.y * RotationSpeed;
				target.x = Mathf.Clamp((target.x <= 180f) ? target.x : (0f - (360f - target.x)), MinSpaceCameraAngle, MaxSpaceCameraAngle);
				target.z = 0f;
			}
			m_TargetRotate = target;
			EventBus.RaiseEvent(delegate(ICameraMovementHandler h)
			{
				h.HandleCameraRotated(target.y);
			});
		}
		m_RotationByKeyboard = false;
		m_RotateOffset = 0f;
		FigureOutScreenBasis();
	}

	private Vector2 CameraDragToRotate()
	{
		if (!m_BaseMousePoint.HasValue)
		{
			return Vector2.zero;
		}
		Vector2 vector = new Vector2(m_BaseMousePoint.Value.x - GetLocalPointerPosition().x, m_BaseMousePoint.Value.y - GetLocalPointerPosition().y);
		Vector2 result = (m_RotateDistance - vector) * (BlueprintRoot.Instance ? (SettingsRoot.Controls.CameraRotationSpeedEdge.GetValue() * 0.05f) : 2f);
		m_RotateDistance = vector;
		return result;
	}

	public void TickShake()
	{
		if (!SettingsRoot.Graphics.CameraShake)
		{
			return;
		}
		m_Shake.Tick(Time.deltaTime);
		if (m_Shake.Active)
		{
			base.transform.position += m_Shake.CurrentShift;
		}
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 right = CameraAttachPoint.right;
		Vector3 up = CameraAttachPoint.up;
		foreach (CameraShakeFx shakeFx in m_ShakeFxList)
		{
			Vector2 vector = shakeFx.CalculateDelta(base.transform.position);
			Vector3 vector2 = vector.x * right + vector.y * up;
			if (shakeFx.ShakeAnchor)
			{
				zero += vector2;
			}
			else
			{
				zero2 += vector2;
			}
		}
		Vector3 vector3 = zero - m_AnchorShakeOffset;
		if (!vector3.Equals(Vector3.zero))
		{
			base.transform.position += vector3;
		}
		m_AnchorShakeOffset = zero;
		vector3 = zero2 - m_CameraShakeOffset;
		if (!vector3.Equals(Vector3.zero))
		{
			Camera.transform.position += vector3;
		}
		m_CameraShakeOffset = zero2;
		if (!DontForceLookAtTarget)
		{
			Camera.transform.LookAt(base.transform.position);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(m_LevelBounds.center, m_LevelBounds.size);
	}

	public void ScrollTo(Vector3 position)
	{
		if (!m_RecordLock)
		{
			if (m_ScrollRoutine != null)
			{
				StopCoroutine(m_ScrollRoutine);
			}
			m_TargetPosition = position;
			m_SkipClampOneFrame = true;
		}
	}

	public float CalculateScrollTime(Vector3 sourcePos, Vector3 targetPos, float maxTime, float maxSpeed, float speed)
	{
		float num = Vector3.Distance(targetPos, sourcePos);
		float num2 = ((0f < maxTime) ? Mathf.Clamp(num / maxTime, speed, (speed <= maxSpeed) ? maxSpeed : float.MaxValue) : speed);
		if (!(0f < num2))
		{
			return 0f;
		}
		return num / num2;
	}

	public Coroutine ScrollToTimed(Vector3 position, float maxTime = 0f, float maxSpeed = float.MaxValue, float speed = 0f, AnimationCurve curve = null, bool useUnscaledTime = false, Action callback = null)
	{
		float targetTime;
		return ScrollToTimed(position, out targetTime, maxTime, maxSpeed, speed, curve, useUnscaledTime, callback);
	}

	public Coroutine ScrollToTimed(Vector3 position, out float targetTime, float maxTime = 0f, float maxSpeed = float.MaxValue, float speed = 0f, AnimationCurve curve = null, bool useUnscaledTime = false, Action callback = null)
	{
		if (m_RecordLock)
		{
			targetTime = 0f;
			return null;
		}
		if (m_ScrollRoutine != null)
		{
			StopCoroutine(m_ScrollRoutine);
		}
		m_ScrollRoutineEndPos = position;
		if (maxTime == 0f && speed > 0f)
		{
			targetTime = Game.Instance.SynchronizedDataController.GetMinScrollTimeBySpeed(position, speed);
		}
		else
		{
			targetTime = Game.Instance.SynchronizedDataController.GetMinScrollTimeBySpeed(position, speed, maxTime, maxSpeed);
		}
		Vector3 position2 = base.transform.position;
		if (curve == null)
		{
			curve = AnimationCurveUtility.LinearAnimationCurve;
		}
		m_ScrollRoutine = StartCoroutine(ScrollToRoutine(position2, ClampByLevelBounds(position), targetTime, curve, useUnscaledTime, callback));
		return m_ScrollRoutine;
	}

	public new void StopCoroutine([CanBeNull] Coroutine coroutine)
	{
		if (coroutine != null)
		{
			if (m_ScrollRoutine == coroutine)
			{
				m_ScrollRoutine = null;
			}
			else if (m_RotateRoutine == coroutine)
			{
				m_RotateRoutine = null;
			}
			base.StopCoroutine(coroutine);
		}
	}

	private IEnumerator ScrollToRoutine(Vector3 fromPosition, Vector3 toPosition, float time, AnimationCurve curve, bool useUnscaledTime, Action callback = null)
	{
		float start = unityTime();
		m_ScrollRoutineEndsOn = start + time;
		while (unityTime() < m_ScrollRoutineEndsOn)
		{
			yield return null;
			Vector3 pos = Vector3.LerpUnclamped(fromPosition, toPosition, curve.Evaluate((unityTime() - start) / time));
			pos = PlaceOnGround(pos);
			base.transform.position = (m_TargetPosition = pos);
		}
		base.transform.position = PlaceOnGround(toPosition);
		yield return null;
		m_ScrollRoutine = null;
		callback?.Invoke();
		float unityTime()
		{
			if (!useUnscaledTime)
			{
				return Time.time;
			}
			return Time.unscaledTime;
		}
	}

	public void ScrollBy2D(Vector2 vec)
	{
		m_ScrollBy2D = vec;
	}

	public bool IsScrollActive([CanBeNull] Coroutine scrollCoroutine)
	{
		if (scrollCoroutine == null)
		{
			return false;
		}
		return scrollCoroutine == m_ScrollRoutine;
	}

	public bool IsRotationActive([CanBeNull] Coroutine coroutine)
	{
		if (coroutine == null)
		{
			return false;
		}
		return coroutine == m_RotateRoutine;
	}

	public void RotateTo(float position)
	{
		if (!m_RecordLock)
		{
			if (m_RotateRoutine != null)
			{
				StopCoroutine(m_RotateRoutine);
			}
			m_TargetRotate.y = position;
		}
	}

	public float CalculateRotateTime(float sourceAngle, float targetAngle, float maxTime, float speed)
	{
		float num = Mathf.Abs(Mathf.DeltaAngle(sourceAngle, targetAngle));
		float num2 = ((0f < maxTime) ? (num / maxTime) : speed);
		if (!(0f < num2))
		{
			return 0f;
		}
		return num / num2;
	}

	public Coroutine RotateToTimed(float toAngle, float maxTime = 0f, float speed = 0f, AnimationCurve curve = null)
	{
		float targetTime;
		return RotateToTimed(toAngle, out targetTime, maxTime, speed, curve);
	}

	public Coroutine RotateToTimed(float toAngle, out float targetTime, float maxTime = 0f, float speed = 0f, AnimationCurve curve = null)
	{
		if (m_RecordLock)
		{
			targetTime = 0f;
			return null;
		}
		if (m_RotateRoutine != null)
		{
			StopCoroutine(m_RotateRoutine);
		}
		base.transform.DOKill();
		m_RotationByMouse = false;
		m_RotationByKeyboard = false;
		if (curve == null)
		{
			curve = AnimationCurveUtility.LinearAnimationCurve;
		}
		if (maxTime == 0f && speed > 0f)
		{
			targetTime = Game.Instance.SynchronizedDataController.GetMinRotateTimeBySpeed(toAngle, speed);
		}
		else
		{
			targetTime = Game.Instance.SynchronizedDataController.GetMinRotateTimeBySpeed(toAngle, speed, maxTime);
		}
		if (targetTime == 0f)
		{
			m_RotateRoutine = null;
			return m_RotateRoutine;
		}
		float y = base.transform.rotation.eulerAngles.y;
		m_RotateRoutine = StartCoroutine(RotateToRoutine(y, toAngle, targetTime, curve));
		return m_RotateRoutine;
	}

	private IEnumerator RotateToRoutine(float fromAngle, float toAngle, float time, AnimationCurve curve)
	{
		float start = Time.time;
		m_RotateRoutineEndsOn = start + time;
		while (Time.time < m_RotateRoutineEndsOn)
		{
			yield return null;
			float num = curve.Evaluate((Time.time - start) / time);
			m_TargetRotate.y = fromAngle + Mathf.DeltaAngle(fromAngle, toAngle) * num;
			base.transform.rotation = Quaternion.Euler(m_TargetRotate);
		}
		m_TargetRotate.y = toAngle;
		base.transform.rotation = Quaternion.Euler(m_TargetRotate);
		yield return null;
		m_RotateRoutine = null;
	}

	private Vector3 PlaceOnGround(Vector3 pos)
	{
		if (NewBehaviour)
		{
			return PlaceOnGround2(pos);
		}
		Vector3 forward = Camera.transform.forward;
		int num = Physics.RaycastNonAlloc(pos - forward * 100f, forward, m_ReuseHits, 200f, 4194576);
		switch (num)
		{
		case 0:
			return pos;
		case 1:
			return m_ReuseHits[0].point;
		default:
		{
			float num2 = float.MaxValue;
			RaycastHit raycastHit = default(RaycastHit);
			raycastHit.point = pos;
			RaycastHit raycastHit2 = raycastHit;
			Vector3 vector = pos - forward * 0.25f;
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit3 = m_ReuseHits[i];
				Collider collider = raycastHit2.collider;
				if ((object)collider != null && collider.gameObject.layer == 22 && raycastHit3.collider.gameObject.layer != 22)
				{
					continue;
				}
				float sqrMagnitude = Vector3.Project(vector - raycastHit3.point, forward).sqrMagnitude;
				if (!(sqrMagnitude < num2))
				{
					Collider collider2 = raycastHit2.collider;
					if (((object)collider2 != null && collider2.gameObject.layer == 22) || raycastHit3.collider.gameObject.layer != 22)
					{
						continue;
					}
				}
				num2 = sqrMagnitude;
				raycastHit2 = raycastHit3;
			}
			return raycastHit2.point;
		}
		}
	}

	private Vector3 PlaceOnGround2(Vector3 pos)
	{
		Vector3 down = Vector3.down;
		int num = Physics.RaycastNonAlloc(pos - down * 100f, down, m_ReuseHits, 200f, 4194576);
		Vector3 result = pos;
		BlueprintAreaPart currentlyLoadedAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
		if (currentlyLoadedAreaPart != null && currentlyLoadedAreaPart.Bounds.UseCameraCollidersAsBounds)
		{
			result = base.transform.position;
		}
		switch (num)
		{
		case 0:
			return result;
		case 1:
			return m_ReuseHits[0].point;
		default:
		{
			float num2 = float.MinValue;
			RaycastHit raycastHit = default(RaycastHit);
			raycastHit.point = pos;
			RaycastHit raycastHit2 = raycastHit;
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit3 = m_ReuseHits[i];
				Collider collider = raycastHit2.collider;
				if ((object)collider != null && collider.gameObject.layer == 22 && raycastHit3.collider.gameObject.layer != 22)
				{
					continue;
				}
				if (!(raycastHit3.point.y > num2))
				{
					Collider collider2 = raycastHit2.collider;
					if (((object)collider2 != null && collider2.gameObject.layer == 22) || raycastHit3.collider.gameObject.layer != 22)
					{
						continue;
					}
				}
				num2 = raycastHit3.point.y;
				raycastHit2 = raycastHit3;
			}
			return raycastHit2.point;
		}
		}
	}

	private Vector3 LowerGently(Vector3 prevPos, Vector3 newPos, float dt)
	{
		if (newPos.y - prevPos.y >= 0f)
		{
			if (m_LastHighGroundPos.HasValue)
			{
				UberDebug.Log("Lost high ground");
			}
			m_LastHighGroundPos = null;
			m_Lowering = false;
			return newPos;
		}
		if (!m_LastHighGroundPos.HasValue)
		{
			UberDebug.Log($"New high ground {prevPos}");
			m_LastHighGroundPos = prevPos;
			m_LastDistToHighGroundPos = 0f;
			m_Lowering = false;
		}
		float num = Vector2.Distance(newPos.To2D(), m_LastHighGroundPos.Value.To2D());
		float lastDistToHighGroundPos = m_LastDistToHighGroundPos;
		m_LastDistToHighGroundPos = num;
		float y = newPos.y;
		if (num > lastDistToHighGroundPos)
		{
			if (num > DistanceFromHighGround || NewBehaviour2)
			{
				if (!m_Lowering)
				{
					m_Lowering = true;
					m_TimeToGroundMs = TimeToGround;
				}
				if (m_Lowering && m_TimeToGroundMs > 0f)
				{
					m_LowerSpeedY = (prevPos.y - newPos.y) / m_TimeToGroundMs;
					y = prevPos.y - m_LowerSpeedY * dt;
					float num2 = y;
					if (NewBehaviour2)
					{
						y = EnsureAboveGround(new Vector3(newPos.x, y, newPos.z)).y;
					}
					if (y < newPos.y)
					{
						y = newPos.y;
					}
					if (!NewBehaviour2 || y <= num2)
					{
						m_TimeToGroundMs -= dt;
					}
				}
				else
				{
					m_LowerSpeedY = 0f;
					y = newPos.y;
				}
			}
			else
			{
				y = m_LastHighGroundPos.Value.y;
			}
		}
		else
		{
			y = prevPos.y;
		}
		return new Vector3(newPos.x, y, newPos.z);
	}

	private Vector3 EnsureAboveGround(Vector3 pos)
	{
		Vector3 position = base.transform.position;
		base.transform.position = pos;
		Vector3 position2 = Camera.transform.position;
		base.transform.position = position;
		Vector3 down = Vector3.down;
		int num = Physics.RaycastNonAlloc(position2 - down * 100f, down, m_ReuseHits, 200f, 4194576);
		switch (num)
		{
		case 0:
			return pos;
		case 1:
			if (position2.y > m_ReuseHits[0].point.y)
			{
				return new Vector3(pos.x, pos.y + m_ReuseHits[0].point.y - position2.y + m_AttackPointPos.Value.y * 0.5f, pos.z);
			}
			return pos;
		default:
		{
			float num2 = float.MinValue;
			RaycastHit raycastHit = default(RaycastHit);
			raycastHit.point = pos;
			RaycastHit raycastHit2 = raycastHit;
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit3 = m_ReuseHits[i];
				Collider collider = raycastHit2.collider;
				if ((object)collider != null && collider.gameObject.layer == 22 && raycastHit3.collider.gameObject.layer != 22)
				{
					continue;
				}
				if (!(raycastHit3.point.y > num2))
				{
					Collider collider2 = raycastHit2.collider;
					if (((object)collider2 != null && collider2.gameObject.layer == 22) || raycastHit3.collider.gameObject.layer != 22)
					{
						continue;
					}
				}
				num2 = raycastHit3.point.y;
				raycastHit2 = raycastHit3;
			}
			if (position2.y > raycastHit2.point.y)
			{
				return new Vector3(pos.x, pos.y + raycastHit2.point.y - position2.y + m_AttackPointPos.Value.y, pos.z);
			}
			return pos;
		}
		}
	}

	public float GetSqr2DDistanceTo(Vector3 pos)
	{
		if (NewBehaviour)
		{
			return (pos.To2D() - base.transform.position.To2D()).sqrMagnitude;
		}
		Vector3 vector = pos - base.transform.position;
		Vector3 forward = Camera.transform.forward;
		return (vector - Vector3.Project(vector, forward)).sqrMagnitude;
	}

	public void ScrollToImmediately(Vector3 position)
	{
		if (!m_RecordLock)
		{
			position = PlaceOnGround(position);
			base.transform.position = position + GetViewportOffset();
			m_TargetPosition = position;
		}
	}

	public void RotateToImmediately(float position)
	{
		if (!m_RecordLock)
		{
			if (m_RotateRoutine != null)
			{
				StopCoroutine(m_RotateRoutine);
			}
			m_TargetRotate.y = position;
			base.transform.rotation = Quaternion.Euler(m_TargetRotate);
		}
	}

	public void StartShake(float amplitude, float speed)
	{
		m_Shake.Start(amplitude, speed);
	}

	public void StopShake()
	{
		m_Shake.Active = false;
	}

	public bool IsShaking()
	{
		return m_Shake.Active;
	}

	public void AddShakeFx(CameraShakeFx cameraShakeFx)
	{
		m_ShakeFxList.Add(cameraShakeFx);
	}

	public void RemoveShakeFx(CameraShakeFx cameraShakeFx)
	{
		m_ShakeFxList.Remove(cameraShakeFx);
	}

	public Vector3 GetTargetPointPosition()
	{
		return m_TargetPosition;
	}

	public float GetCameraHeight()
	{
		return CameraAttachPoint.transform.localPosition.y;
	}

	public Vector3 GetAttachPointPosition()
	{
		return CameraAttachPoint.position;
	}

	public void SetRotation(float cameraRotation)
	{
		base.transform.rotation = Quaternion.Euler(0f, cameraRotation, 0f);
	}

	public void ResetCameraRotate()
	{
		RotateToTimed(90f, 0.4f);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		Game.Instance.CursorController.ClearCursor(force: true);
		m_BaseMousePoint = null;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		Game.Instance.CursorController.ClearCursor(force: true);
		m_BaseMousePoint = null;
	}

	void IDialogFinishHandler.HandleDialogFinished(BlueprintDialog dialog, bool success)
	{
		m_HandRotationLock = false;
	}

	void IDialogStartHandler.HandleDialogStarted(BlueprintDialog dialog)
	{
		if (dialog.IsLockCameraRotationButtons)
		{
			m_HandRotationLock = true;
		}
	}

	void IFullScreenUIHandler.HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		HandleFullScreenUiChangedInternal(state, fullScreenUIType);
	}

	void IFullScreenUIHandlerWorkaround.HandleFullScreenUiChangedWorkaround(bool state, FullScreenUIType fullScreenUIType)
	{
		HandleFullScreenUiChangedInternal(state, fullScreenUIType);
	}

	private void HandleFullScreenUiChangedInternal(bool state, FullScreenUIType fullScreenUIType)
	{
		m_FullScreenUIType = (state ? fullScreenUIType : FullScreenUIType.Unknown);
	}

	public void LockCamera()
	{
		m_HandRotationLock = true;
		m_HandScrollLock = true;
		CameraZoom.ZoomLock = true;
	}

	public void UnLockCamera()
	{
		m_HandRotationLock = false;
		m_HandScrollLock = false;
		CameraZoom.ZoomLock = false;
	}

	public void SetIsRecordPlayback(bool isPlayingBackRecord)
	{
		m_RecordLock = isPlayingBackRecord;
		CameraZoom.RecordLock = isPlayingBackRecord;
	}

	public Quaternion SetAudioListenerForActionCamera()
	{
		m_AudioListener.GetComponent<ListenerZoom>().enabled = false;
		Quaternion localRotation = m_AudioListener.transform.localRotation;
		ChangeListenerParent(Camera.transform);
		m_AudioListener.ResetAll();
		return localRotation;
	}

	public void ResetAudioListenerAfterActionCamera(Quaternion listenerLocalRotation)
	{
		m_AudioListener.localRotation = listenerLocalRotation;
		m_AudioListener.GetComponent<ListenerZoom>().enabled = true;
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Logger.Error(this, "Multiple camera rigs. Old: {0}, New: {1}. Replacing", Instance, this);
		}
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
			return;
		}
		Logger.Error(this, "Multiple camera rigs. Old: {0}, New: {1}. Keeping old", Instance, this);
	}

	[Cheat(Name = "Start_Shake", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void StartShakeCheat(float amplitude, float speed)
	{
		Instance.StartShake(amplitude, speed);
	}
}

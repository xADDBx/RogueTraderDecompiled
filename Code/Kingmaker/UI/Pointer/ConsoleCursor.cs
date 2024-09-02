using System.Collections;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.PathRenderer;
using Kingmaker.UI.Workarounds;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;
using Rewired.Components;
using Rewired.Integration.UnityUI;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.Pointer;

public class ConsoleCursor : BaseCursor, IRewiredCursorController, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, IInteractionHighlightUIHandler
{
	[Header("Console")]
	[SerializeField]
	private float m_MoveSpeed;

	[SerializeField]
	private float m_RotationAngle;

	[SerializeField]
	private GameObject m_LineContainer;

	private CanvasScalerWorkaround m_CanvasScaler;

	public BoolReactiveProperty IsActiveProperty = new BoolReactiveProperty(initialValue: false);

	public BoolReactiveProperty IsNotActiveProperty = new BoolReactiveProperty(initialValue: true);

	[SerializeField]
	private PlayerMouse m_Mouse;

	private bool m_TurnBaseMode;

	private Vector2 m_ScreenCenter;

	private bool m_LockState;

	public static ConsoleCursor Instance { get; private set; }

	private float ScreenScale
	{
		get
		{
			if (!(m_CanvasScaler != null))
			{
				return 1f;
			}
			return Mathf.Min((float)Screen.width / m_CanvasScaler.referenceResolution.x, (float)Screen.height / m_CanvasScaler.referenceResolution.y);
		}
	}

	bool IRewiredCursorController.Enabled
	{
		get
		{
			return base.IsActive;
		}
		set
		{
			SetActive(value);
		}
	}

	GameObject IRewiredCursorController.Cursor => m_CursorTransform.Or(null)?.gameObject;

	public bool OnScreenCenter => Vector2.Distance(m_Position, m_ScreenCenter) <= 200f;

	private bool CheckVisibility
	{
		get
		{
			if ((m_CurrentType == CursorType.Default || m_CurrentType == CursorType.Restricted) && m_TurnBaseMode)
			{
				return PointerController.InGui;
			}
			return true;
		}
	}

	private bool CheckRotation
	{
		get
		{
			if (m_TurnBaseMode)
			{
				InteractionHighlightController instance = InteractionHighlightController.Instance;
				if (instance != null && !instance.IsHighlighting)
				{
					return !PointerController.InGui;
				}
			}
			return false;
		}
	}

	public void Initialize(CanvasScalerWorkaround canvasScaler)
	{
		m_CanvasScaler = canvasScaler;
	}

	protected override void OnBind()
	{
		Instance = this;
		UIKitRewiredCursorController.SetRewiredCursorController(this);
		m_Mouse.enabled = false;
		m_Mouse.GetComponent<RewiredStandaloneInputModule>().AddMouseInputSource(m_Mouse);
		m_ScreenCenter = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		EventBus.Subscribe(this);
		SwitchTurnBaseMode(TurnController.IsInTurnBasedCombat());
	}

	protected override void OnDispose()
	{
		if (m_LockState)
		{
			StopAllCoroutines();
			m_LockState = false;
		}
		Instance = null;
		RewiredStandaloneInputModule component = m_Mouse.GetComponent<RewiredStandaloneInputModule>();
		component.RemoveMouseInputSource(m_Mouse);
		component.PlayerMice = null;
		EventBus.Unsubscribe(this);
	}

	protected override void OnSetPosition(Vector2 position)
	{
		m_Mouse.screenPosition = position;
	}

	protected override void OnSetActive(bool active)
	{
		m_Mouse.enabled = active;
		if (active)
		{
			SetToCenter();
		}
		else
		{
			SetToOut();
		}
		IsActiveProperty.Value = active;
		IsNotActiveProperty.Value = !active;
	}

	public void MoveCursor(Vector2 moveVector)
	{
		if (!m_LockState)
		{
			Vector2 position = base.Position;
			position += moveVector * m_MoveSpeed;
			position.x = Mathf.Clamp(position.x, 0f, (float)Screen.width - m_CursorTransform.rect.width);
			position.y = Mathf.Clamp(position.y, m_CursorTransform.rect.height, Screen.height);
			base.Position = position;
		}
	}

	public void SetToCenter()
	{
		base.Position = m_ScreenCenter;
	}

	public void SetToOut()
	{
		base.Position = new Vector2(-1f, -1f);
	}

	public void SetCursorPositionToTarget(Vector2 pos, bool smooth = false, float duration = 0.3f)
	{
		StartCoroutine(MoveCursorToTarget(pos, duration, smooth));
	}

	private IEnumerator MoveCursorToTarget(Vector2 pos, float duration, bool smooth)
	{
		float elapsedTime = 0f;
		m_LockState = true;
		try
		{
			if (base.transform.parent.transform as RectTransform == null)
			{
				m_LockState = false;
				yield break;
			}
			Vector2 correctedPos = new Vector2(pos.x * ScreenScale, pos.y * ScreenScale);
			if (smooth)
			{
				while (elapsedTime < duration)
				{
					elapsedTime += Time.deltaTime;
					float t = Mathf.Clamp01(elapsedTime / duration);
					base.Position = Vector2.Lerp(base.Position, correctedPos, t);
					yield return null;
				}
			}
			base.Position = correctedPos;
		}
		finally
		{
			m_LockState = false;
		}
	}

	public void SnapToCurrentNode()
	{
		InteractionHighlightController instance = InteractionHighlightController.Instance;
		if ((instance == null || !instance.IsHighlighting) && !PointerController.InGui && (m_TextsGroup.activeInHierarchy || m_CursorImage.gameObject.activeInHierarchy))
		{
			m_CursorTransform.anchoredPosition = UnitPathManager.Instance.PointerCellDecalCornersPositions.Select((Vector3 p) => UIUtilityGetRect.ObjectPixelPositionInRect(p, m_CursorTransform)).MaxBy((Vector2 p) => p.x);
		}
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		SwitchTurnBaseMode(isTurnBased);
	}

	void ITurnBasedModeResumeHandler.HandleTurnBasedModeResumed()
	{
		SwitchTurnBaseMode(on: true);
	}

	private void SwitchTurnBaseMode(bool on)
	{
		m_TurnBaseMode = on;
		UpdateVisibility();
	}

	private void SwitchRotate(bool rotate)
	{
		Quaternion identity = Quaternion.identity;
		Quaternion quaternion = Quaternion.Euler(0f, 0f, m_RotationAngle);
		m_CursorTransform.rotation = (rotate ? quaternion : identity);
		m_TextsGroup.transform.rotation = identity;
		m_AbilityGroup.transform.rotation = identity;
		m_NoMoveObject.transform.rotation = identity;
	}

	protected override void OnSetCursor(CursorType type)
	{
		UpdateVisibility();
	}

	protected override void OnSetText(bool hasText)
	{
		if (m_TurnBaseMode)
		{
			m_LineContainer.SetActive(hasText);
		}
	}

	void IInteractionHighlightUIHandler.HandleHighlightChange(bool isOn)
	{
		UpdateVisibility();
	}

	public override void OnGuiChanged(bool onGui)
	{
		UpdateVisibility();
		if (onGui && m_TurnBaseMode)
		{
			SetToCenter();
		}
	}

	private void UpdateVisibility()
	{
		SwitchRotate(CheckRotation);
		m_CursorImage.gameObject.SetActive(CheckVisibility);
	}
}

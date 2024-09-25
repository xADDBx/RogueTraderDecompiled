using System.Collections;
using System.Linq;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Workarounds;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual;
using Kingmaker.Visual.Decals;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics;
using Owlcat.Runtime.Visual.CustomPostProcess;
using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Overrides;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Kingmaker.UI.DollRoom;

public class DollRoomBase : MonoBehaviour, ISaveManagerHandler, ISubscriber
{
	[SerializeField]
	[FormerlySerializedAs("m_CharacterPlaceholder")]
	protected Transform m_TargetPlaceholder;

	[SerializeField]
	protected DollRoomTargetController m_TargetController;

	[SerializeField]
	private Volume m_DollRoomPostProcessVolume;

	[SerializeField]
	private Volume m_DollRoomCharGenPostProcessVolume;

	[SerializeField]
	private float m_DollAppearStep = 0.05f;

	[SerializeField]
	private float m_DollAppearStepInterval = 0.05f;

	[SerializeField]
	protected DollCamera m_Camera;

	[SerializeField]
	protected Light[] m_DisabledLights;

	[SerializeField]
	private DollRoomBackground m_Background;

	[SerializeField]
	private CanvasScalerWorkaround m_DefaultCanvasScaler;

	private CustomPostProcessOverride m_DollPostProcessOverride;

	private ShaderPropertyDescriptor m_DollDissolveProperty;

	private ShaderPropertyDescriptor m_DollBackgroundProperty;

	private Vector2 m_RenderTextureSize;

	private RenderTexture m_RenderTexture;

	private bool m_IsInitialized;

	private bool m_SavedApplyShaderManually;

	private float m_SavedShadowDistance;

	private Scene m_SavedActiveScene;

	private CameraStackManager.CameraStackState m_CameraStackState;

	public bool IsVisible { get; private set; }

	public virtual void Initialize(DollRoomTargetController characterController)
	{
		m_TargetController = characterController;
		if (m_TargetController == null || m_TargetController.RawImage == null)
		{
			Debug.LogError(m_TargetController.name + " hasn't image reference for doll room rendering");
			m_RenderTextureSize = new Vector2(1024f, 1024f);
		}
		else
		{
			m_RenderTextureSize = characterController.GetRawImageSize(m_DefaultCanvasScaler);
			m_TargetController.Target = m_TargetPlaceholder;
		}
		EnsureRenderTexture();
		EnsureBackground();
		m_IsInitialized = true;
	}

	private void EnsureBackground()
	{
		if (!(m_Background != null))
		{
			m_Background = GetComponentInChildren<DollRoomBackground>();
		}
	}

	private CustomPostProcessOverride GetCustomPostProcessOverride(Volume postProcessVolume)
	{
		foreach (VolumeComponent component in postProcessVolume.profile.components)
		{
			if (component.GetType() == typeof(CustomPostProcessOverride))
			{
				return (CustomPostProcessOverride)component;
			}
		}
		return null;
	}

	private void SetupInventoryDollPostProcessAndAnimation(Volume targetVolume)
	{
		if (targetVolume == null)
		{
			return;
		}
		if (m_DollPostProcessOverride == null)
		{
			m_DollPostProcessOverride = GetCustomPostProcessOverride(targetVolume);
		}
		if (!(m_DollPostProcessOverride == null))
		{
			if (m_DollDissolveProperty == null)
			{
				m_DollDissolveProperty = m_DollPostProcessOverride.Settings.value.EffectOverrides[0].Parameters[4].Property;
			}
			if (m_DollDissolveProperty != null)
			{
				StopCoroutine(DollAppear(m_DollDissolveProperty));
				m_DollDissolveProperty.FloatValue = 1f;
				StartCoroutine(DollAppear(m_DollDissolveProperty));
			}
		}
	}

	public void SetupDollPostProcessAndAnimation(bool isCharGen)
	{
		if (!(m_DollRoomPostProcessVolume == null) && !(m_DollRoomCharGenPostProcessVolume == null))
		{
			if (isCharGen)
			{
				m_DollRoomPostProcessVolume.enabled = false;
				m_DollRoomCharGenPostProcessVolume.enabled = true;
			}
			else
			{
				m_DollRoomPostProcessVolume.enabled = true;
				m_DollRoomCharGenPostProcessVolume.enabled = false;
				SetupInventoryDollPostProcessAndAnimation(m_DollRoomPostProcessVolume);
			}
		}
	}

	private IEnumerator DollAppear(ShaderPropertyDescriptor alphaProperty)
	{
		for (float alpha = 1f; alpha >= 0f; alpha -= m_DollAppearStep)
		{
			alphaProperty.FloatValue = alpha;
			yield return new WaitForSecondsRealtime(m_DollAppearStepInterval);
		}
		alphaProperty.FloatValue = 0f;
	}

	private void OnEnable()
	{
		if (m_IsInitialized && m_RenderTexture == null)
		{
			EnsureRenderTexture();
		}
	}

	private void OnDisable()
	{
		if (IsVisible)
		{
			Hide();
		}
		ReleaseRenderTexture();
	}

	private void EnsureRenderTexture()
	{
		if (m_RenderTexture != null && (m_RenderTexture.width != (int)m_RenderTextureSize.x || m_RenderTexture.height != (int)m_RenderTextureSize.y))
		{
			ReleaseRenderTexture();
		}
		m_RenderTexture = new RenderTexture((int)m_RenderTextureSize.x, (int)m_RenderTextureSize.y, 0, RenderTextureFormat.ARGBHalf);
		m_RenderTexture.name = $"DollRoomRT_{m_RenderTexture.width}_{m_RenderTexture.height}_{m_RenderTexture.format}";
		m_TargetController.RawImage.texture = m_RenderTexture;
	}

	private void ReleaseRenderTexture()
	{
		if (m_RenderTexture != null)
		{
			m_RenderTexture.Release();
			m_RenderTexture = null;
		}
	}

	public void SetVisibility(bool isVisible)
	{
		if (isVisible)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public virtual void Show()
	{
		if (!IsVisible)
		{
			RenderBackgroundForDollRoom();
			IsVisible = true;
			m_DisabledLights = Object.FindObjectsOfType<Light>().Except(GetComponentsInChildren<Light>()).ToArray();
			VisualSettingsOn();
			SetActiveLights(isActive: false);
			base.gameObject.SetActive(value: true);
			SetFogOfWarEnabled(enabled: false);
			m_Camera.SetAsCurrent();
			EventBus.Subscribe(this);
			PositionBasedDynamicsConfig.Instance.UpdateMode = UpdateMode.FixedUpdateFrequency;
		}
	}

	public virtual void Hide()
	{
		if (IsVisible)
		{
			IsVisible = false;
			SetActiveLights(isActive: true);
			m_DisabledLights = null;
			VisualSettingsOff();
			Cleanup();
			base.gameObject.SetActive(value: false);
			SetFogOfWarEnabled(enabled: true);
			EventBus.Unsubscribe(this);
			PositionBasedDynamicsConfig.Instance.UpdateMode = UpdateMode.FixedUpdateFrequencyWithPause;
		}
	}

	private void RenderBackgroundForDollRoom()
	{
		if (m_Background == null)
		{
			PFLog.TechArt.Error(base.gameObject, "Can't find doll room background object");
			return;
		}
		if (m_DollPostProcessOverride == null && m_DollRoomPostProcessVolume != null)
		{
			m_DollPostProcessOverride = GetCustomPostProcessOverride(m_DollRoomPostProcessVolume);
		}
		if (m_DollPostProcessOverride == null)
		{
			PFLog.TechArt.Error(base.gameObject, "Can't find post process override for doll room");
			return;
		}
		if (m_DollBackgroundProperty == null)
		{
			m_DollBackgroundProperty = m_DollPostProcessOverride.Settings.value.EffectOverrides[0].Parameters[0].Property;
		}
		if (m_DollBackgroundProperty == null)
		{
			PFLog.TechArt.Error(base.gameObject, "Can't find dissolve property in post process override for doll room");
		}
		else
		{
			m_Background.Render(m_DollBackgroundProperty);
		}
	}

	protected virtual void Cleanup()
	{
		if ((bool)m_Camera)
		{
			m_Camera.ResetZoom();
		}
	}

	private void VisualSettingsOn()
	{
		EnsureRenderTexture();
		EnsureCamera();
		if (m_Camera != null && m_RenderTexture != null)
		{
			m_Camera.SetTargetTexture(m_RenderTexture);
			m_Camera.EnableCamera();
		}
		m_CameraStackState = CameraStackManager.Instance.State;
		CameraStackManager.Instance.State = CameraStackManager.CameraStackState.UiOnly;
		m_SavedActiveScene = SceneManager.GetActiveScene();
		SceneManager.SetActiveScene(base.gameObject.scene);
		m_SavedShadowDistance = QualitySettings.shadowDistance;
		ScreenSpaceDecalRenderer.IsGUIDecalsVisible = false;
		QualitySettings.shadowDistance = 10f;
	}

	private void EnsureCamera()
	{
		if (m_Camera == null)
		{
			m_Camera = GetComponentInChildren<DollCamera>();
		}
		if (m_Camera == null)
		{
			m_Camera = DollCamera.Current;
		}
	}

	private void VisualSettingsOff()
	{
		EnsureCamera();
		m_Camera.DisableCamera();
		CameraStackManager.Instance.State = m_CameraStackState;
		if (m_SavedActiveScene.isLoaded)
		{
			SceneManager.SetActiveScene(m_SavedActiveScene);
		}
		ScreenSpaceDecalRenderer.IsGUIDecalsVisible = true;
		QualitySettings.shadowDistance = m_SavedShadowDistance;
	}

	private void SetFogOfWarEnabled(bool enabled)
	{
		if (!(FogOfWarArea.Active == null))
		{
			if (enabled)
			{
				FogOfWarArea.Active.ApplyShaderManually = m_SavedApplyShaderManually;
				return;
			}
			m_SavedApplyShaderManually = FogOfWarArea.Active.ApplyShaderManually;
			FogOfWarArea.Active.ApplyShaderManually = true;
		}
	}

	void ISaveManagerHandler.HandleBeforeMadeScreenshot()
	{
		SetActiveLights(isActive: true);
		if (m_SavedActiveScene.isLoaded)
		{
			SceneManager.SetActiveScene(m_SavedActiveScene);
		}
	}

	void ISaveManagerHandler.HandleAfterMadeScreenshot()
	{
		SetActiveLights(isActive: false);
		SceneManager.SetActiveScene(base.gameObject.scene);
	}

	private void SetActiveLights(bool isActive)
	{
		m_DisabledLights?.ForEach(delegate(Light l)
		{
			if ((bool)l && l.enabled)
			{
				l.gameObject.SetActive(isActive);
			}
		});
	}
}

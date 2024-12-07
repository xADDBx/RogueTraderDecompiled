using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.LINQ;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Overrides.HBAO;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Settings.Graphics;

public class GraphicsSettingsController
{
	private const string PREF_SELECTED_DISPLAY = "UnitySelectMonitor";

	private const string PREF_RESOLUTION_WIDTH = "Screenmanager Resolution Width";

	private const string PREF_RESOLUTION_HEIGHT = "Screenmanager Resolution Heigh";

	private const string PREF_FULLSCREEN_MODE = "Screenmanager Fullscreen mode";

	private readonly GraphicsSettings m_Settings;

	private readonly IReadOnlyList<UnityEngine.Resolution> m_CommonResolutions = new List<UnityEngine.Resolution>
	{
		new UnityEngine.Resolution
		{
			width = 2560,
			height = 1440
		},
		new UnityEngine.Resolution
		{
			width = 2048,
			height = 1080
		},
		new UnityEngine.Resolution
		{
			width = 3840,
			height = 2160
		},
		new UnityEngine.Resolution
		{
			width = 1366,
			height = 768
		},
		new UnityEngine.Resolution
		{
			width = 1920,
			height = 1080
		},
		new UnityEngine.Resolution
		{
			width = 1440,
			height = 900
		},
		new UnityEngine.Resolution
		{
			width = 1280,
			height = 1024
		},
		new UnityEngine.Resolution
		{
			width = 1600,
			height = 900
		},
		new UnityEngine.Resolution
		{
			width = 1024,
			height = 768
		},
		new UnityEngine.Resolution
		{
			width = 1680,
			height = 1050
		},
		new UnityEngine.Resolution
		{
			width = 1920,
			height = 1200
		},
		new UnityEngine.Resolution
		{
			width = 1360,
			height = 768
		},
		new UnityEngine.Resolution
		{
			width = 1280,
			height = 720
		}
	};

	private readonly List<UnityEngine.Resolution> m_RelevantResolutions = new List<UnityEngine.Resolution>();

	private readonly List<DisplayInfo> m_DisplayLayout = new List<DisplayInfo>();

	private readonly IReadOnlySettingEntity<int> m_Width;

	private readonly IReadOnlySettingEntity<int> m_Height;

	private bool m_NeedUpdateScreen;

	private bool m_NeedUpdateDisplay;

	private bool m_NeedUpdateGraphics;

	private bool m_SuppressedExclusiveFullScreen;

	public IReadOnlyList<UnityEngine.Resolution> RelevantResolutions => m_RelevantResolutions;

	public IReadOnlyList<DisplayInfo> DisplayLayout => m_DisplayLayout;

	public AntialiasingMode AntialiasingMode => m_Settings.AntialiasingMode.GetValue();

	public QualityOption AntialiasingQuality => m_Settings.AntialiasingQuality.GetValue();

	public GraphicsSettingsController(UnityEngine.Resolution? maximumResolution)
	{
		m_Settings = SettingsRoot.Graphics;
		SettingsController.Instance.OnConfirmedAllSettings += OnSettingsConfirmed;
		m_Settings.Display.OnValueChanged += delegate
		{
			OnDisplaySettingsChanged();
		};
		m_Settings.FullScreenMode.OnValueChanged += delegate
		{
			OnScreenSettingsChanged();
		};
		m_Settings.ScreenResolution.OnValueChanged += delegate
		{
			OnScreenSettingsChanged();
		};
		m_Settings.VSyncMode.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.FrameRateLimitEnabled.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.FrameRateLimit.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.ShadowsQuality.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.TexturesQuality.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.DepthOfField.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.Bloom.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.SSAOQuality.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.SSRQuality.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.AntialiasingMode.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.AntialiasingQuality.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.FootprintsMode.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.FsrMode.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.FsrSharpness.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.VolumetricLightingQuality.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.ParticleSystemsShadowsEnabled.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.FilmGrainEnabled.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.CrowdQuality.OnValueChanged += delegate
		{
			OnGraphicsSettingsChanged();
		};
		m_Settings.WindowedCursorLock.OnValueChanged += OnWindowedCursorLockChanged;
		OnWindowedCursorLockChanged(m_Settings.WindowedCursorLock);
		GenerateResolutionsList(maximumResolution);
		m_Width = m_Settings.ScreenResolution.Select(GetWidthAtIndex);
		m_Height = m_Settings.ScreenResolution.Select(GetHeightAtIndex);
		GenerateDisplayLayout();
		InitScreen();
		m_Settings.Display.OnValueChanged += SetPlayerPrefsDisplay;
		m_Width.OnValueChanged += SetPlayerPrefsResolutionWidth;
		m_Height.OnValueChanged += SetPlayerPrefsResolutionHeight;
		m_Settings.FullScreenMode.OnValueChanged += SetPlayerPrefsFullscreenMode;
		MonoSingleton<ApplicationFocusObserver>.Instance.OnApplicationChangedFocus += OnApplicationChangedFocus;
		_ = MonoSingleton<TexturesQualityController>.Instance;
		m_NeedUpdateGraphics = true;
		OnSettingsConfirmed();
		WaaaghPipeline.VolumeManagerUpdated = (Action<Camera>)Delegate.Combine(WaaaghPipeline.VolumeManagerUpdated, new Action<Camera>(OnVolumeManagerUpdated));
		CameraStackManager.Instance.StackChanged += delegate
		{
			ApplyAntiAliasingSettings();
		};
	}

	private void OnDisplaySettingsChanged()
	{
		m_NeedUpdateDisplay = true;
	}

	private void OnScreenSettingsChanged()
	{
		m_NeedUpdateScreen = true;
	}

	private void OnGraphicsSettingsChanged()
	{
		m_NeedUpdateGraphics = true;
	}

	private void OnSettingsConfirmed()
	{
		if (m_NeedUpdateDisplay)
		{
			ApplyDisplaySettings();
		}
		if (m_NeedUpdateScreen)
		{
			ApplyScreenSettings();
		}
		if (m_NeedUpdateGraphics)
		{
			ApplyGraphicsSettings();
		}
		m_NeedUpdateDisplay = false;
		m_NeedUpdateScreen = false;
		m_NeedUpdateGraphics = false;
	}

	private void SetPlayerPrefsDisplay(int displayNum)
	{
		PlayerPrefs.SetInt("UnitySelectMonitor", displayNum);
	}

	private void SetPlayerPrefsResolutionWidth(int width)
	{
		PlayerPrefs.SetInt("Screenmanager Resolution Width", width);
	}

	private void SetPlayerPrefsResolutionHeight(int height)
	{
		PlayerPrefs.SetInt("Screenmanager Resolution Heigh", height);
	}

	private void SetPlayerPrefsFullscreenMode(FullScreenMode fullScreenMode)
	{
		PlayerPrefs.SetInt("Screenmanager Fullscreen mode", (int)fullScreenMode);
	}

	private void ApplyDisplaySettings()
	{
		if (!Application.isConsolePlatform)
		{
			DisplayInfo display = m_DisplayLayout[m_Settings.Display.GetValue()];
			Screen.MoveMainWindowTo(in display, GetSafeWindowPosition(in display));
		}
		else
		{
			RestartConfirmation();
		}
	}

	private Vector2Int GetSafeWindowPosition(in DisplayInfo displayInfo)
	{
		if (Screen.fullScreenMode == FullScreenMode.Windowed)
		{
			Vector2Int mainWindowPosition = Screen.mainWindowPosition;
			Vector2Int vector2Int = new Vector2Int(displayInfo.width - m_Width.GetValue(), displayInfo.height - m_Height.GetValue());
			if (vector2Int.x < 0)
			{
				vector2Int.x = 0;
			}
			if (vector2Int.y < 0)
			{
				vector2Int.y = 0;
			}
			if (mainWindowPosition.x > vector2Int.x)
			{
				mainWindowPosition.x = vector2Int.x;
			}
			if (mainWindowPosition.y > vector2Int.y)
			{
				mainWindowPosition.y = vector2Int.y;
			}
			return mainWindowPosition;
		}
		return Vector2Int.zero;
	}

	public void ApplyScreenSettings()
	{
		SetSafeResolution(m_Width.GetValue(), m_Height.GetValue(), m_Settings.FullScreenMode.GetValue());
	}

	private void OnVolumeManagerUpdated(Camera camera)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		Hbao component = stack.GetComponent<Hbao>();
		switch (m_Settings.SSAOQuality.GetValue())
		{
		case QualityOptionDisactivatable.Off:
			component.Intensity.value = 0f;
			break;
		case QualityOptionDisactivatable.Medium:
			component.Quality.value = Quality.Low;
			break;
		case QualityOptionDisactivatable.High:
			component.Quality.value = Quality.High;
			break;
		}
		BloomEnhanced component2 = stack.GetComponent<BloomEnhanced>();
		if (component2.IsActive() && !m_Settings.Bloom.GetValue())
		{
			component2.intensity.value = 0f;
		}
		DepthOfField component3 = stack.GetComponent<DepthOfField>();
		if (component3.IsActive() && !m_Settings.DepthOfField.GetValue())
		{
			component3.mode.value = DepthOfFieldMode.Off;
		}
		ScreenSpaceReflections component4 = stack.GetComponent<ScreenSpaceReflections>();
		if (component4.IsActive())
		{
			switch (m_Settings.SSRQuality.GetValue())
			{
			case QualityOptionDisactivatable.Off:
				component4.Quality.value = ScreenSpaceReflectionsQuality.None;
				break;
			case QualityOptionDisactivatable.Medium:
				component4.Quality.value = ScreenSpaceReflectionsQuality.Half;
				break;
			case QualityOptionDisactivatable.High:
				component4.Quality.value = ScreenSpaceReflectionsQuality.Full;
				break;
			}
		}
		FilmGrain component5 = stack.GetComponent<FilmGrain>();
		if (component5.IsActive() && !m_Settings.FilmGrainEnabled.GetValue())
		{
			component5.intensity.value = 0f;
		}
	}

	private void ApplyGraphicsSettings()
	{
		switch (m_Settings.VSyncMode.GetValue())
		{
		case VSyncModeOptions.EveryFrame:
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = -1;
			break;
		case VSyncModeOptions.EverySecondFrame:
			QualitySettings.vSyncCount = 2;
			Application.targetFrameRate = -1;
			break;
		case VSyncModeOptions.EveryThirdFrame:
			QualitySettings.vSyncCount = 3;
			Application.targetFrameRate = -1;
			break;
		case VSyncModeOptions.EveryFourthFrame:
			QualitySettings.vSyncCount = 4;
			Application.targetFrameRate = -1;
			break;
		default:
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = (m_Settings.FrameRateLimitEnabled ? Mathf.Max(m_Settings.FrameRateLimit.GetValue(), 30) : (-1));
			break;
		}
		if (BuildModeUtility.Data.ForceTextureMipLevel >= 0)
		{
			QualitySettings.globalTextureMipmapLimit = BuildModeUtility.Data.ForceTextureMipLevel;
		}
		else
		{
			switch (m_Settings.TexturesQuality.GetValue())
			{
			case QualityOption.Low:
				QualitySettings.globalTextureMipmapLimit = 2;
				break;
			case QualityOption.Medium:
				QualitySettings.globalTextureMipmapLimit = 1;
				break;
			case QualityOption.High:
				QualitySettings.globalTextureMipmapLimit = 0;
				break;
			}
		}
		if (TryGetRenderPipelineAsset(out var result))
		{
			ShadowSettings shadowSettings = result.ShadowSettings;
			shadowSettings.ShadowQuality = m_Settings.ShadowsQuality.GetValue() switch
			{
				QualityOptionDisactivatable.Off => ShadowQuality.Disable, 
				QualityOptionDisactivatable.Medium => ShadowQuality.HardOnly, 
				QualityOptionDisactivatable.High => ShadowQuality.All, 
				_ => ShadowQuality.Disable, 
			};
			result.UpscalingFilter = UpscalingFilterSelection.FSR;
			result.FsrOverrideSharpness = true;
			result.FsrSharpness = m_Settings.FsrSharpness.GetValue();
			WaaaghPipelineAsset waaaghPipelineAsset = result;
			waaaghPipelineAsset.RenderScale = m_Settings.FsrMode.GetValue() switch
			{
				FsrMode.Off => 1f, 
				FsrMode.UltraQuality => 0.77f, 
				FsrMode.Quality => 0.67f, 
				FsrMode.Balanced => 0.59f, 
				FsrMode.Performance => 0.5f, 
				_ => 1f, 
			};
			ScriptableRendererData[] rendererDataList = result.RendererDataList;
			for (int i = 0; i < rendererDataList.Length; i++)
			{
				foreach (ScriptableRendererFeature rendererFeature in rendererDataList[i].RendererFeatures)
				{
					if (rendererFeature is VolumetricLightingFeature volumetricLightingFeature)
					{
						switch (m_Settings.VolumetricLightingQuality.GetValue())
						{
						case QualityOption.High:
							volumetricLightingFeature.Settings.Slices = VolumetricLightingSlices.x128;
							volumetricLightingFeature.Settings.TricubicFilteringDeferred = true;
							break;
						case QualityOption.Medium:
							volumetricLightingFeature.Settings.Slices = VolumetricLightingSlices.x96;
							volumetricLightingFeature.Settings.TricubicFilteringDeferred = true;
							break;
						default:
							volumetricLightingFeature.Settings.Slices = VolumetricLightingSlices.x64;
							volumetricLightingFeature.Settings.TricubicFilteringDeferred = false;
							break;
						}
					}
				}
			}
		}
		ApplyAntiAliasingSettings();
		if (MainThreadDispatcher.IsInitialized)
		{
			MainThreadDispatcher.FrequentUpdateInterval = m_Settings.UIFrequentTimerInterval.GetValue();
			MainThreadDispatcher.InfrequentUpdateInterval = m_Settings.UIInfrequentTimerInterval.GetValue();
		}
	}

	private void ApplyAntiAliasingSettings()
	{
		Owlcat.Runtime.Visual.Waaagh.AntialiasingMode antialiasing = m_Settings.AntialiasingMode.GetValue() switch
		{
			AntialiasingMode.Off => Owlcat.Runtime.Visual.Waaagh.AntialiasingMode.None, 
			AntialiasingMode.SMAA => Owlcat.Runtime.Visual.Waaagh.AntialiasingMode.SubpixelMorphologicalAntiAliasing, 
			AntialiasingMode.TAA => Owlcat.Runtime.Visual.Waaagh.AntialiasingMode.TemporalAntialiasing, 
			_ => Owlcat.Runtime.Visual.Waaagh.AntialiasingMode.None, 
		};
		AntialiasingQuality antialiasingQuality = m_Settings.AntialiasingQuality.GetValue() switch
		{
			QualityOption.Low => Owlcat.Runtime.Visual.Waaagh.AntialiasingQuality.Low, 
			QualityOption.Medium => Owlcat.Runtime.Visual.Waaagh.AntialiasingQuality.Medium, 
			QualityOption.High => Owlcat.Runtime.Visual.Waaagh.AntialiasingQuality.High, 
			_ => Owlcat.Runtime.Visual.Waaagh.AntialiasingQuality.Low, 
		};
		List<CameraStackManager.CameraInfo> value;
		using (UnityEngine.Rendering.ListPool<CameraStackManager.CameraInfo>.Get(out value))
		{
			CameraStackManager.Instance.GetStack(value);
			foreach (CameraStackManager.CameraInfo item in value)
			{
				if (item.cameraStackType == CameraStackManager.CameraStackType.Main)
				{
					item.additionalCameraData.Antialiasing = antialiasing;
					item.additionalCameraData.AntialiasingQuality = antialiasingQuality;
				}
				else
				{
					item.additionalCameraData.Antialiasing = Owlcat.Runtime.Visual.Waaagh.AntialiasingMode.None;
					item.additionalCameraData.AntialiasingQuality = Owlcat.Runtime.Visual.Waaagh.AntialiasingQuality.Low;
				}
			}
		}
	}

	private bool TryGetRenderPipelineAsset(out WaaaghPipelineAsset result)
	{
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if ((object)asset != null)
		{
			result = asset;
			return true;
		}
		result = null;
		return false;
	}

	private void InitScreen()
	{
		PFLog.Settings.Log("Relevant Resolutions: " + string.Join(",", m_RelevantResolutions.Select((UnityEngine.Resolution r) => $"{r.width}x{r.height}")));
		PFLog.Settings.Log($"Current Screen Resolution: {Screen.width}x{Screen.height}");
		PFLog.Settings.Log($"Current Resolution ID: {m_Settings.ScreenResolution.GetValue()}");
		PFLog.Settings.Log($"Current Fullscreen Mode: {Screen.fullScreenMode}, Saved Fullscreen Mode: {m_Settings.FullScreenMode.GetValue()}");
		int currentWidth = Screen.width;
		int currentHeight = Screen.height;
		if (!m_Settings.ScreenResolutionWasTouched.GetValue() || (currentWidth < 640 && currentHeight < 480) || !m_RelevantResolutions.Any((UnityEngine.Resolution r) => r.width == currentWidth && r.height == currentHeight))
		{
			AutoDetectResolution();
		}
		if (m_Width.GetValue() != Screen.width || m_Height.GetValue() != Screen.height || m_Settings.FullScreenMode.GetValue() != Screen.fullScreenMode)
		{
			SetSafeResolution(m_Width.GetValue(), m_Height.GetValue(), m_Settings.FullScreenMode.GetValue());
			CorrectPS5ProGraphicsQualitySetting();
		}
	}

	private void AutoDetectResolution()
	{
		PFLog.Settings.Log("AutoDetectResolution - Start");
		Display main = Display.main;
		PFLog.Settings.Log($"Main Display SystemResolution: {main.systemWidth}x{main.systemHeight}");
		PFLog.Settings.Log($"Screen Current Resolution: {Screen.width}x{Screen.height}");
		PFLog.Settings.Log($"Saved Resolution: {m_Width.GetValue()}x{m_Height.GetValue()}");
		PFLog.Settings.Log($"Current display: {m_Settings.Display}");
		PFLog.Settings.Log($"Current display ID: {m_Settings.Display.GetValue()}");
		PFLog.Settings.Log($"Displays count: {Display.displays.Length}");
		int indexOfResolution = GetIndexOfResolution(main.systemWidth, main.systemHeight);
		m_Settings.ScreenResolution.SetTempValue(indexOfResolution);
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
		PFLog.Settings.Log($"Auto detected resolution ID: {m_Settings.ScreenResolution.GetValue()}");
		PFLog.Settings.Log($"Saved Resolution: {m_Width.GetValue()}x{m_Height.GetValue()}");
	}

	private void CorrectPS5ProGraphicsQualitySetting()
	{
		if (m_Width.GetValue() == 3840 && m_Height.GetValue() == 2160)
		{
			m_Settings.PS5ProGraphicsQuality.SetTempValue(PS5ProGraphicsQualityOption.Quality);
		}
		else
		{
			m_Settings.PS5ProGraphicsQuality.SetTempValue(PS5ProGraphicsQualityOption.Performance);
		}
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
	}

	private void OnWindowedCursorLockChanged(bool lockCursor)
	{
		CoroutineRunner.Start(UpdateCursorMode(lockCursor));
	}

	private void SetSafeResolution(int width, int height, FullScreenMode fullscreenMode)
	{
		CoroutineRunner.Start(SetResolution(width, height, fullscreenMode));
		CoroutineRunner.Start(UpdateCursorMode(m_Settings.WindowedCursorLock));
		PFLog.Settings.Log($"Changed screen resolution to: {width}x{height}, mode: {fullscreenMode}");
	}

	private IEnumerator SetResolution(int width, int height, FullScreenMode fullscreenMode)
	{
		if (fullscreenMode == FullScreenMode.MaximizedWindow)
		{
			fullscreenMode = FullScreenMode.Windowed;
		}
		if (Screen.fullScreenMode != fullscreenMode)
		{
			if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
			{
				Screen.SetResolution(Screen.width, Screen.height, fullscreenMode);
				yield return new WaitForEndOfFrame();
				yield return new WaitForEndOfFrame();
				Screen.SetResolution(width, height, fullscreenMode);
			}
			else
			{
				Screen.SetResolution(width, height, Screen.fullScreenMode);
				yield return new WaitForEndOfFrame();
				yield return new WaitForEndOfFrame();
				Screen.SetResolution(width, height, fullscreenMode);
			}
		}
		else
		{
			Screen.SetResolution(width, height, fullscreenMode);
		}
	}

	public void RestartConfirmation()
	{
		LocalizedString text = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI.RestartConfirmation;
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(text, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton b)
			{
				if (b == DialogMessageBoxBase.BoxButton.Yes)
				{
					SystemUtil.ApplicationRestart();
				}
			});
		});
	}

	private static IEnumerator UpdateCursorMode(bool lockCursor)
	{
		yield return new WaitForEndOfFrame();
		Cursor.lockState = CursorLockMode.None;
		yield return new WaitForEndOfFrame();
		if (lockCursor)
		{
			Cursor.lockState = CursorLockMode.Confined;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	private void GenerateDisplayLayout()
	{
		if (!Application.isConsolePlatform)
		{
			m_DisplayLayout.Clear();
			Screen.GetDisplayLayout(m_DisplayLayout);
		}
	}

	private void GenerateResolutionsList(UnityEngine.Resolution? maxResolution)
	{
		m_RelevantResolutions.Clear();
		int num = m_Settings.Display;
		if (num < 0)
		{
			num = 0;
		}
		if (num >= Display.displays.Length)
		{
			num = Display.displays.Length - 1;
		}
		int systemWidth = Display.displays[num].systemWidth;
		int systemHeight = Display.displays[num].systemHeight;
		float a2 = (float)systemWidth / (float)systemHeight;
		PFLog.Settings.Log("Displays Info:");
		for (int i = 0; i < Display.displays.Length; i++)
		{
			Display display = Display.displays[i];
			PFLog.Settings.Log($" - ({i}) system: {display.systemWidth}x{display.systemHeight}, rendering: {display.renderingWidth}x{display.renderingHeight}");
		}
		PFLog.Settings.Log($"Generating resolution list from Screen.resolutions (system: {systemWidth}x{systemHeight}, display id: {num})");
		m_RelevantResolutions.Add(new UnityEngine.Resolution
		{
			width = systemWidth,
			height = systemHeight
		});
		UnityEngine.Resolution[] resolutions = Screen.resolutions;
		for (int j = 0; j < resolutions.Length; j++)
		{
			UnityEngine.Resolution resolution = resolutions[j];
			PFLog.Settings.Log($"- {resolution.width}x{resolution.height} : {resolution.refreshRate}");
			float b2 = (float)resolution.width / (float)resolution.height;
			if ((Mathf.Approximately(a2, b2) || m_CommonResolutions.Contains((UnityEngine.Resolution r) => r.width == resolution.width && r.height == resolution.height)) && (!maxResolution.HasValue || (resolution.width <= maxResolution.Value.width && resolution.height <= maxResolution.Value.height)) && !m_RelevantResolutions.Contains((UnityEngine.Resolution r) => r.width == resolution.width && r.height == resolution.height))
			{
				m_RelevantResolutions.Add(resolution);
			}
		}
		m_RelevantResolutions.Sort((UnityEngine.Resolution a, UnityEngine.Resolution b) => (b.width * b.height).CompareTo(a.width * a.height));
	}

	private void OnApplicationChangedFocus(bool focus)
	{
		if (!focus)
		{
			if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
			{
				m_SuppressedExclusiveFullScreen = true;
				SetSafeResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow);
			}
		}
		else if (m_SuppressedExclusiveFullScreen)
		{
			m_SuppressedExclusiveFullScreen = false;
			SetSafeResolution(Screen.width, Screen.height, FullScreenMode.ExclusiveFullScreen);
		}
	}

	private int GetWidthAtIndex(int index)
	{
		if (m_RelevantResolutions == null || m_RelevantResolutions.Count == 0)
		{
			return 640;
		}
		if (index < 0)
		{
			index = 0;
		}
		if (index >= m_RelevantResolutions.Count)
		{
			index = m_RelevantResolutions.Count - 1;
		}
		return m_RelevantResolutions[index].width;
	}

	private int GetHeightAtIndex(int index)
	{
		if (m_RelevantResolutions == null || m_RelevantResolutions.Count == 0)
		{
			return 480;
		}
		if (index < 0)
		{
			index = 0;
		}
		if (index >= m_RelevantResolutions.Count)
		{
			index = m_RelevantResolutions.Count - 1;
		}
		return m_RelevantResolutions[index].height;
	}

	private int GetIndexOfResolution(int width, int height)
	{
		for (int i = 0; i < m_RelevantResolutions.Count; i++)
		{
			UnityEngine.Resolution resolution = m_RelevantResolutions[i];
			if (resolution.width == width && resolution.height == height)
			{
				return i;
			}
		}
		return -1;
	}
}

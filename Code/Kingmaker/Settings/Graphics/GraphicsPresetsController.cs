using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Settings.Graphics;

public class GraphicsPresetsController
{
	private interface IImplementation
	{
		Resolution? MaximumAllowedResolution { get; }

		void Initialize();
	}

	private sealed class DefaultImplementation : IImplementation
	{
		private readonly GraphicsSettings m_Settings;

		private readonly Dictionary<QualityPresetOption, GraphicsPreset> m_Presets;

		private GraphicsPreset m_LastAppliedPreset;

		public Resolution? MaximumAllowedResolution => null;

		public DefaultImplementation(GraphicsPresetsList graphicsPresetsList)
		{
			m_Settings = SettingsRoot.Graphics;
			m_Presets = (from asset in graphicsPresetsList.GraphicsPresets
				select asset.Preset into p
				where p.GraphicsQuality != QualityPresetOption.Custom
				select p).ToDictionary((GraphicsPreset p) => p.GraphicsQuality, (GraphicsPreset p) => p);
		}

		public void Initialize()
		{
			InitializeValues();
			InitializeQualityPresetCorrection();
		}

		private void InitializeValues()
		{
			if (m_Settings.GraphicsQualityWasTouched.GetValue())
			{
				if ((QualityPresetOption)m_Settings.GraphicsQuality != QualityPresetOption.Custom)
				{
					QualityPresetOption value = m_Settings.GraphicsQuality.GetValue();
					PFLog.Settings.Log("Applying selected graphics quality preset: {0}", value);
					if (m_Presets.TryGetValue(value, out var value2))
					{
						SetTempValuesFromPreset(value2);
						return;
					}
					PFLog.Settings.Error("Graphics quality preset not found: {0}", value);
				}
			}
			else
			{
				QualityPresetOption qualityPresetOption = AutodetectQuality();
				PFLog.Settings.Log("Graphics quality preset was not selected. Applying auto detected graphics quality preset: {0}", qualityPresetOption);
				if (m_Presets.TryGetValue(qualityPresetOption, out var value3))
				{
					SetTempValuesFromPreset(value3);
					return;
				}
				PFLog.Settings.Error("Graphics quality preset not found: {0}", qualityPresetOption);
			}
		}

		private void InitializeQualityPresetCorrection()
		{
			m_Settings.GraphicsQuality.OnTempValueChanged += delegate(QualityPresetOption quality)
			{
				if (m_Presets.TryGetValue(quality, out var value))
				{
					SetTempValuesFromPreset(value);
				}
			};
			m_Settings.VSyncMode.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.FrameRateLimitEnabled.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.FrameRateLimit.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.ShadowsQuality.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.TexturesQuality.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.DepthOfField.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.Bloom.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.SSAOQuality.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.SSRQuality.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.AntialiasingMode.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.AntialiasingQuality.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.FootprintsMode.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.FsrMode.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.FsrSharpness.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.VolumetricLightingQuality.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.ParticleSystemsLightingEnabled.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.ParticleSystemsShadowsEnabled.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.FilmGrainEnabled.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.UIFrequentTimerInterval.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.UIInfrequentTimerInterval.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
			m_Settings.CrowdQuality.OnTempValueChanged += delegate
			{
				TryCorrectGraphicsQuality();
			};
		}

		private static QualityPresetOption AutodetectQuality()
		{
			QualityPresetOption result = QualityPresetOption.Low;
			if (ApplicationHelper.IsRunOnSteamDeck)
			{
				result = QualityPresetOption.SteamDeck;
			}
			else
			{
				switch (HardwareConfigDetect.GetConfigIndex())
				{
				case HardwareConfigDetect.HardwareLevel.Low:
					result = QualityPresetOption.Low;
					break;
				case HardwareConfigDetect.HardwareLevel.Medium:
					result = QualityPresetOption.Medium;
					break;
				case HardwareConfigDetect.HardwareLevel.High:
					result = QualityPresetOption.High;
					break;
				}
			}
			return result;
		}

		private void SetTempValuesFromPreset(GraphicsPreset preset)
		{
			m_LastAppliedPreset = null;
			m_Settings.GraphicsQuality.SetTempValue(preset.GraphicsQuality);
			m_Settings.VSyncMode.SetTempValue(preset.VSyncMode);
			m_Settings.FrameRateLimitEnabled.SetTempValue(preset.FrameRateLimitEnabled);
			m_Settings.FrameRateLimit.SetTempValue(preset.FrameRateLimit);
			m_Settings.ShadowsQuality.SetTempValue(preset.ShadowsQuality);
			m_Settings.TexturesQuality.SetTempValue(preset.TexturesQuality);
			m_Settings.DepthOfField.SetTempValue(preset.DepthOfField);
			m_Settings.Bloom.SetTempValue(preset.Bloom);
			m_Settings.SSAOQuality.SetTempValue(preset.SSAOQuality);
			m_Settings.SSRQuality.SetTempValue(preset.SSRQuality);
			m_Settings.AntialiasingMode.SetTempValue(preset.AntialiasingMode);
			m_Settings.AntialiasingQuality.SetTempValue(preset.AntialiasingQuality);
			m_Settings.FootprintsMode.SetTempValue(preset.FootprintsMode);
			m_Settings.FsrMode.SetTempValue(preset.FsrMode);
			m_Settings.FsrSharpness.SetTempValue(preset.FsrSharpness);
			m_Settings.VolumetricLightingQuality.SetTempValue(preset.VolumetricLightingQuality);
			m_Settings.ParticleSystemsLightingEnabled.SetTempValue(preset.ParticleSystemsLightingEnabled);
			m_Settings.ParticleSystemsShadowsEnabled.SetTempValue(preset.ParticleSystemsShadowsEnabled);
			m_Settings.FilmGrainEnabled.SetTempValue(preset.FilmGrainEnabled);
			m_Settings.UIFrequentTimerInterval.SetTempValue(preset.UIFrequentTimerInterval);
			m_Settings.UIInfrequentTimerInterval.SetTempValue(preset.UIInfrequentTimerInterval);
			m_Settings.CrowdQuality.SetTempValue(preset.CrowdQuality);
			m_LastAppliedPreset = preset;
		}

		private void TryCorrectGraphicsQuality()
		{
			if (m_LastAppliedPreset != null)
			{
				QualityPresetOption tempValue = (AreTempValuesMatchesPreset(m_LastAppliedPreset) ? m_LastAppliedPreset.GraphicsQuality : QualityPresetOption.Custom);
				m_Settings.GraphicsQuality.SetTempValue(tempValue);
			}
		}

		private bool AreTempValuesMatchesPreset(GraphicsPreset preset)
		{
			if (preset.GraphicsQuality == m_Settings.GraphicsQuality.GetTempValue() && preset.VSyncMode == m_Settings.VSyncMode.GetTempValue() && preset.FrameRateLimitEnabled == m_Settings.FrameRateLimitEnabled.GetTempValue() && preset.FrameRateLimit == m_Settings.FrameRateLimit.GetTempValue() && preset.ShadowsQuality == m_Settings.ShadowsQuality.GetTempValue() && preset.TexturesQuality == m_Settings.TexturesQuality.GetTempValue() && preset.DepthOfField == m_Settings.DepthOfField.GetTempValue() && preset.Bloom == m_Settings.Bloom.GetTempValue() && preset.SSAOQuality == m_Settings.SSAOQuality.GetTempValue() && preset.SSRQuality == m_Settings.SSRQuality.GetTempValue() && preset.AntialiasingMode == m_Settings.AntialiasingMode.GetTempValue() && preset.AntialiasingQuality == m_Settings.AntialiasingQuality.GetTempValue() && preset.FootprintsMode == m_Settings.FootprintsMode.GetTempValue() && preset.FsrMode == m_Settings.FsrMode.GetTempValue() && Mathf.Approximately(preset.FsrSharpness, m_Settings.FsrSharpness.GetTempValue()) && preset.VolumetricLightingQuality == m_Settings.VolumetricLightingQuality.GetTempValue() && preset.ParticleSystemsLightingEnabled == m_Settings.ParticleSystemsLightingEnabled.GetTempValue() && preset.ParticleSystemsShadowsEnabled == m_Settings.ParticleSystemsShadowsEnabled.GetTempValue() && preset.FilmGrainEnabled == m_Settings.FilmGrainEnabled.GetTempValue() && Mathf.Approximately(preset.UIFrequentTimerInterval, m_Settings.UIFrequentTimerInterval.GetTempValue()) && Mathf.Approximately(preset.UIInfrequentTimerInterval, m_Settings.UIInfrequentTimerInterval.GetTempValue()))
			{
				return preset.CrowdQuality == m_Settings.CrowdQuality.GetTempValue();
			}
			return false;
		}
	}

	internal sealed class ConsoleImplementation : IImplementation
	{
		private readonly GraphicsPresetsList.UserControlledValues m_UserControlledValues;

		public Resolution? MaximumAllowedResolution
		{
			get
			{
				Resolution value;
				if (!IsTrinityMode())
				{
					Resolution resolution = default(Resolution);
					resolution.width = 1920;
					resolution.height = 1080;
					value = resolution;
				}
				else
				{
					Resolution resolution = default(Resolution);
					resolution.width = 3840;
					resolution.height = 2160;
					value = resolution;
				}
				return value;
			}
		}

		public ConsoleImplementation(GraphicsPresetsList graphicsPresetsList)
		{
			m_UserControlledValues = graphicsPresetsList.ConsoleUserControlledValues;
		}

		public void Initialize()
		{
			GraphicsSettings settings = SettingsRoot.Graphics;
			if (!m_UserControlledValues.VSyncMode)
			{
				settings.VSyncMode.ResetToDefault();
			}
			if (!m_UserControlledValues.FrameRateLimitEnabled)
			{
				settings.FrameRateLimitEnabled.ResetToDefault();
			}
			if (!m_UserControlledValues.FrameRateLimit)
			{
				settings.FrameRateLimit.ResetToDefault();
			}
			if (!m_UserControlledValues.ShadowsQuality)
			{
				settings.ShadowsQuality.ResetToDefault();
			}
			if (!m_UserControlledValues.TexturesQuality)
			{
				settings.TexturesQuality.ResetToDefault();
			}
			if (!m_UserControlledValues.DepthOfField)
			{
				settings.DepthOfField.ResetToDefault();
			}
			if (!m_UserControlledValues.Bloom)
			{
				settings.Bloom.ResetToDefault();
			}
			if (!m_UserControlledValues.SSAOQuality)
			{
				settings.SSAOQuality.ResetToDefault();
			}
			if (!m_UserControlledValues.SSRQuality)
			{
				settings.SSRQuality.ResetToDefault();
			}
			if (!m_UserControlledValues.AntialiasingMode)
			{
				settings.AntialiasingMode.ResetToDefault();
			}
			if (!m_UserControlledValues.AntialiasingQuality)
			{
				settings.AntialiasingQuality.ResetToDefault();
			}
			if (!m_UserControlledValues.FootprintsMode)
			{
				settings.FootprintsMode.ResetToDefault();
			}
			if (!m_UserControlledValues.FsrMode)
			{
				settings.FsrMode.ResetToDefault();
			}
			if (!m_UserControlledValues.FsrSharpness)
			{
				settings.FsrSharpness.ResetToDefault();
			}
			if (!m_UserControlledValues.VolumetricLightingQuality)
			{
				settings.VolumetricLightingQuality.ResetToDefault();
			}
			if (!m_UserControlledValues.ParticleSystemsLightingEnabled)
			{
				settings.ParticleSystemsLightingEnabled.ResetToDefault();
			}
			if (!m_UserControlledValues.ParticleSystemsShadowsEnabled)
			{
				settings.ParticleSystemsShadowsEnabled.ResetToDefault();
			}
			if (!m_UserControlledValues.FilmGrainEnabled)
			{
				settings.FilmGrainEnabled.ResetToDefault();
			}
			if (!m_UserControlledValues.UIFrequentTimerInterval)
			{
				settings.UIFrequentTimerInterval.ResetToDefault();
			}
			if (!m_UserControlledValues.UIInfrequentTimerInterval)
			{
				settings.UIInfrequentTimerInterval.ResetToDefault();
			}
			if (!m_UserControlledValues.CrowdQuality)
			{
				settings.CrowdQuality.ResetToDefault();
			}
			settings.PS5ProGraphicsQuality.OnTempValueChanged += delegate(PS5ProGraphicsQualityOption qualityValue)
			{
				switch (qualityValue)
				{
				case PS5ProGraphicsQualityOption.Quality:
				{
					int indexOfResolution2 = GetIndexOfResolution(3840, 2160, settings.ScreenResolution.GetValue());
					settings.ScreenResolution.SetTempValue(indexOfResolution2);
					break;
				}
				case PS5ProGraphicsQualityOption.Performance:
				{
					int indexOfResolution = GetIndexOfResolution(1920, 1080, settings.ScreenResolution.GetValue());
					settings.ScreenResolution.SetTempValue(indexOfResolution);
					break;
				}
				}
			};
			LogResetMessage();
		}

		private static bool IsTrinityMode()
		{
			return false;
		}

		private static int GetIndexOfResolution(int width, int height, int defaultIndex)
		{
			GraphicsSettingsController graphicsSettingsController = SettingsController.Instance.GraphicsSettingsController;
			if (graphicsSettingsController == null)
			{
				return defaultIndex;
			}
			List<Resolution> list = graphicsSettingsController.RelevantResolutions.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				Resolution resolution = list[i];
				if (resolution.width == width && resolution.height == height)
				{
					return i;
				}
			}
			return defaultIndex;
		}

		private void LogResetMessage()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("Keep graphics settings controlled by user:");
			AppendSettings(controlledByUser: true);
			builder.AppendLine("Reset graphics settings not controlled by user to default values:");
			AppendSettings(controlledByUser: false);
			PFLog.Settings.Log(builder.ToString());
			void AppendSetting(string name)
			{
				builder.Append("    ").Append(name).AppendLine();
			}
			void AppendSettings(bool controlledByUser)
			{
				if (controlledByUser == m_UserControlledValues.VSyncMode)
				{
					AppendSetting("VSyncMode");
				}
				if (controlledByUser == m_UserControlledValues.FrameRateLimitEnabled)
				{
					AppendSetting("FrameRateLimitEnabled");
				}
				if (controlledByUser == m_UserControlledValues.FrameRateLimit)
				{
					AppendSetting("FrameRateLimit");
				}
				if (controlledByUser == m_UserControlledValues.ShadowsQuality)
				{
					AppendSetting("ShadowsQuality");
				}
				if (controlledByUser == m_UserControlledValues.TexturesQuality)
				{
					AppendSetting("TexturesQuality");
				}
				if (controlledByUser == m_UserControlledValues.DepthOfField)
				{
					AppendSetting("DepthOfField");
				}
				if (controlledByUser == m_UserControlledValues.Bloom)
				{
					AppendSetting("Bloom");
				}
				if (controlledByUser == m_UserControlledValues.SSAOQuality)
				{
					AppendSetting("SSAOQuality");
				}
				if (controlledByUser == m_UserControlledValues.SSRQuality)
				{
					AppendSetting("SSRQuality");
				}
				if (controlledByUser == m_UserControlledValues.AntialiasingMode)
				{
					AppendSetting("AntialiasingMode");
				}
				if (controlledByUser == m_UserControlledValues.AntialiasingQuality)
				{
					AppendSetting("AntialiasingQuality");
				}
				if (controlledByUser == m_UserControlledValues.FootprintsMode)
				{
					AppendSetting("FootprintsMode");
				}
				if (controlledByUser == m_UserControlledValues.FsrMode)
				{
					AppendSetting("FsrMode");
				}
				if (controlledByUser == m_UserControlledValues.FsrSharpness)
				{
					AppendSetting("FsrSharpness");
				}
				if (controlledByUser == m_UserControlledValues.VolumetricLightingQuality)
				{
					AppendSetting("VolumetricLightingQuality");
				}
				if (controlledByUser == m_UserControlledValues.ParticleSystemsLightingEnabled)
				{
					AppendSetting("ParticleSystemsLightingEnabled");
				}
				if (controlledByUser == m_UserControlledValues.ParticleSystemsShadowsEnabled)
				{
					AppendSetting("ParticleSystemsShadowsEnabled");
				}
				if (controlledByUser == m_UserControlledValues.FilmGrainEnabled)
				{
					AppendSetting("FilmGrainEnabled");
				}
				if (controlledByUser == m_UserControlledValues.UIFrequentTimerInterval)
				{
					AppendSetting("UIFrequentTimerInterval");
				}
				if (controlledByUser == m_UserControlledValues.UIInfrequentTimerInterval)
				{
					AppendSetting("UIInfrequentTimerInterval");
				}
				if (controlledByUser == m_UserControlledValues.CrowdQuality)
				{
					AppendSetting("CrowdQuality");
				}
			}
		}
	}

	private readonly IImplementation m_Implementation;

	public Resolution? MaximumAllowedResolution => m_Implementation.MaximumAllowedResolution;

	public GraphicsPresetsController(GraphicsPresetsList graphicsPresetsList)
	{
		m_Implementation = new DefaultImplementation(graphicsPresetsList);
	}

	public void Initialize()
	{
		m_Implementation.Initialize();
	}
}

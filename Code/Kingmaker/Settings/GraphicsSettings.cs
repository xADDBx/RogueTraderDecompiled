using System.Linq;
using System.Text;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Graphics;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Settings.LINQ;
using UnityEngine;

namespace Kingmaker.Settings;

public class GraphicsSettings
{
	public readonly SettingsEntityInt Display;

	public readonly SettingsEntityEnum<FullScreenMode> FullScreenMode;

	public readonly SettingsEntityInt ScreenResolution;

	public readonly SettingsEntityBool WindowedCursorLock;

	public readonly SettingsEntityBool CameraShake;

	public readonly SettingsEntityEnum<QualityPresetOption> GraphicsQuality;

	public readonly SettingsEntityEnum<VSyncModeOptions> VSyncMode;

	public readonly SettingsEntityBool FrameRateLimitEnabled;

	public readonly SettingsEntityInt FrameRateLimit;

	public readonly SettingsEntityEnum<QualityOptionDisactivatable> ShadowsQuality;

	public readonly SettingsEntityEnum<QualityOption> TexturesQuality;

	public readonly SettingsEntityBool DepthOfField;

	public readonly SettingsEntityBool Bloom;

	public readonly SettingsEntityEnum<QualityOptionDisactivatable> SSAOQuality;

	public readonly SettingsEntityEnum<QualityOptionDisactivatable> SSRQuality;

	public readonly SettingsEntityEnum<AntialiasingMode> AntialiasingMode;

	public readonly SettingsEntityEnum<QualityOption> AntialiasingQuality;

	public readonly SettingsEntityEnum<FootprintsMode> FootprintsMode;

	public readonly SettingsEntityEnum<FsrMode> FsrMode;

	public readonly SettingsEntityFloat FsrSharpness;

	public readonly SettingsEntityEnum<QualityOption> VolumetricLightingQuality;

	public readonly SettingsEntityBool ParticleSystemsLightingEnabled;

	public readonly SettingsEntityBool ParticleSystemsShadowsEnabled;

	public readonly SettingsEntityBool FilmGrainEnabled;

	public readonly SettingsEntityEnum<CrowdQualityOptions> CrowdQuality;

	public readonly SettingsEntityFloat UIFrequentTimerInterval;

	public readonly SettingsEntityFloat UIInfrequentTimerInterval;

	public readonly IReadOnlySettingEntity<bool> GraphicsQualityWasTouched;

	public readonly IReadOnlySettingEntity<bool> ScreenResolutionWasTouched;

	public GraphicsSettings(ISettingsController settingsController, SettingsValues settingsValues)
	{
		GraphicsPreset defaultPreset = GetDefaultPreset(settingsValues);
		GraphicsQuality = new SettingsEntityEnum<QualityPresetOption>(settingsController, "quality", defaultPreset.GraphicsQuality);
		VSyncMode = new SettingsEntityEnum<VSyncModeOptions>(settingsController, "vsync-mode", defaultPreset.VSyncMode);
		FrameRateLimitEnabled = new SettingsEntityBool(settingsController, "fps-limit-enabled", defaultPreset.FrameRateLimitEnabled);
		FrameRateLimit = new SettingsEntityInt(settingsController, "fps-limit", defaultPreset.FrameRateLimit);
		ShadowsQuality = new SettingsEntityEnum<QualityOptionDisactivatable>(settingsController, "shadow-quality", defaultPreset.ShadowsQuality);
		TexturesQuality = new SettingsEntityEnum<QualityOption>(settingsController, "textures", defaultPreset.TexturesQuality);
		DepthOfField = new SettingsEntityBool(settingsController, "depth-of-field", defaultPreset.DepthOfField);
		Bloom = new SettingsEntityBool(settingsController, "bloom", defaultPreset.Bloom);
		SSAOQuality = new SettingsEntityEnum<QualityOptionDisactivatable>(settingsController, "ssao", defaultPreset.SSAOQuality);
		SSRQuality = new SettingsEntityEnum<QualityOptionDisactivatable>(settingsController, "ssr", defaultPreset.SSRQuality);
		AntialiasingMode = new SettingsEntityEnum<AntialiasingMode>(settingsController, "antialiasing-option", defaultPreset.AntialiasingMode);
		AntialiasingQuality = new SettingsEntityEnum<QualityOption>(settingsController, "antialiasing-quality", defaultPreset.AntialiasingQuality);
		FootprintsMode = new SettingsEntityEnum<FootprintsMode>(settingsController, "footprints-mode", defaultPreset.FootprintsMode);
		FsrMode = new SettingsEntityEnum<FsrMode>(settingsController, "fsr-mode", defaultPreset.FsrMode);
		FsrSharpness = new SettingsEntityFloat(settingsController, "fsr-sharpness", defaultPreset.FsrSharpness);
		VolumetricLightingQuality = new SettingsEntityEnum<QualityOption>(settingsController, "volumetric-lighting-quality", defaultPreset.VolumetricLightingQuality);
		ParticleSystemsLightingEnabled = new SettingsEntityBool(settingsController, "particle-systems-lighting", defaultPreset.ParticleSystemsLightingEnabled);
		ParticleSystemsShadowsEnabled = new SettingsEntityBool(settingsController, "particle-systems-shadows", defaultPreset.ParticleSystemsShadowsEnabled);
		FilmGrainEnabled = new SettingsEntityBool(settingsController, "film-grain-enabled", defaultPreset.FilmGrainEnabled);
		UIFrequentTimerInterval = new SettingsEntityFloat(settingsController, "ui-frequent-timer-interval", defaultPreset.UIFrequentTimerInterval);
		UIInfrequentTimerInterval = new SettingsEntityFloat(settingsController, "ui-infrequent-timer-interval", defaultPreset.UIInfrequentTimerInterval);
		CrowdQuality = new SettingsEntityEnum<CrowdQualityOptions>(settingsController, "crowfd-quality", defaultPreset.CrowdQuality);
		GraphicsSettingsDefaultValues graphics = settingsValues.SettingsDefaultValues.Graphics;
		WindowedCursorLock = new SettingsEntityBool(settingsController, "windowed-cursor-lock", graphics.WindowedCursorLock);
		CameraShake = new SettingsEntityBool(settingsController, "camera-shake", graphics.CameraShake);
		Display = new SettingsEntityInt(settingsController, "display-num", 0, saveDependent: false, requireReboot: true);
		FullScreenMode = new SettingsEntityEnum<FullScreenMode>(settingsController, "full-screen-mode", UnityEngine.FullScreenMode.FullScreenWindow);
		ScreenResolution = new SettingsEntityInt(settingsController, "resolution", 0);
		ScreenResolutionWasTouched = ScreenResolution.WasTouched();
		GraphicsQualityWasTouched = GraphicsQuality.WasTouched();
	}

	public void DumpToLog()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("Graphics settings:");
		AppendSetting("VSyncMode", VSyncMode);
		AppendSetting("FrameRateLimitEnabled", FrameRateLimitEnabled);
		AppendSetting("FrameRateLimit", FrameRateLimit);
		AppendSetting("ShadowsQuality", ShadowsQuality);
		AppendSetting("TexturesQuality", TexturesQuality);
		AppendSetting("DepthOfField", DepthOfField);
		AppendSetting("Bloom", Bloom);
		AppendSetting("SSAOQuality", SSAOQuality);
		AppendSetting("SSRQuality", SSRQuality);
		AppendSetting("AntialiasingMode", AntialiasingMode);
		AppendSetting("AntialiasingQuality", AntialiasingQuality);
		AppendSetting("FootprintsMode", FootprintsMode);
		AppendSetting("FsrMode", FsrMode);
		AppendSetting("FsrSharpness", FsrSharpness);
		AppendSetting("VolumetricLightingQuality", VolumetricLightingQuality);
		AppendSetting("ParticleSystemsLightingEnabled", ParticleSystemsLightingEnabled);
		AppendSetting("ParticleSystemsShadowsEnabled", ParticleSystemsShadowsEnabled);
		AppendSetting("FilmGrainEnabled", FilmGrainEnabled);
		AppendSetting("UIFrequentTimerInterval", UIFrequentTimerInterval);
		AppendSetting("UIInfrequentTimerInterval", UIInfrequentTimerInterval);
		AppendSetting("CrowdQuality", CrowdQuality);
		PFLog.Settings.Log(builder.ToString());
		void AppendSetting(string name, ISettingsEntity settingsEntity)
		{
			builder.Append("    ").Append(name).Append(": ")
				.Append(settingsEntity.GetStringValue())
				.AppendLine();
		}
	}

	private static GraphicsPreset GetDefaultPreset(SettingsValues settingsValues)
	{
		if (ShouldUseConsolePreset())
		{
			return settingsValues.GraphicsPresetsList.ConsoleGraphicsPreset.Preset;
		}
		QualityPresetOption defaultGraphicsQuality = settingsValues.SettingsDefaultValues.Graphics.GraphicsQuality;
		return settingsValues.GraphicsPresetsList.GraphicsPresets.FirstOrDefault((GraphicsPresetAsset p) => p.Preset.GraphicsQuality == defaultGraphicsQuality)?.Preset;
		static bool ShouldUseConsolePreset()
		{
			return false;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;
using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIGraphicsSettings : IUISettingsSheet
{
	public UISettingsEntityDropdownInt Display;

	public UISettingsEntityDropdownFullScreenMode FullScreenMode;

	public UISettingsEntityDropdownInt ScreenResolution;

	public UISettingsEntityBool WindowedMouseLock;

	public UISettingsEntityBool CameraShake;

	public UISettingsEntityDropdownGraphicsPreset GraphicsQuality;

	public UISettingsEntityDropdownVSyncMode VSyncMode;

	public UISettingsEntityBool FrameRateLimitEnabled;

	public UISettingsEntitySliderInt FrameRateLimit;

	public UISettingsEntityDropdownFsrMode FsrMode;

	public UISettingsEntitySliderFloat FsrSharpness;

	public UISettingsEntityDropdownQuality VolumetricLightingQuality;

	public UISettingsEntityBool ParticleSystemsLightingEnabled;

	public UISettingsEntityBool ParticleSystemsShadowsEnabled;

	public UISettingsEntityDropdownQualityDisactivatable ShadowsQuality;

	public UISettingsEntityDropdownQuality TexturesQuality;

	public UISettingsEntityBool DepthOfField;

	public UISettingsEntityBool Bloom;

	public UISettingsEntityDropdownQualityDisactivatable SSAOQuality;

	public UISettingsEntityDropdownQualityDisactivatable SSRQuality;

	public UISettingsEntityBool FilmGrainEnabled;

	public UISettingsEntityDropdownAntialiasingMode AntialiasingMode;

	public UISettingsEntityDropdownQuality AntialiasingQuality;

	public UISettingsEntityDropdownFootprintsMode FootprintsMode;

	public UISettingsEntityDropdownCrowdQuality CrowdQuality;

	public void LinkToSettings()
	{
		Display.LinkSetting(SettingsRoot.Graphics.Display);
		FullScreenMode.LinkSetting(SettingsRoot.Graphics.FullScreenMode);
		ScreenResolution.LinkSetting(SettingsRoot.Graphics.ScreenResolution);
		WindowedMouseLock.LinkSetting(SettingsRoot.Graphics.WindowedCursorLock);
		CameraShake.LinkSetting(SettingsRoot.Graphics.CameraShake);
		GraphicsQuality.LinkSetting(SettingsRoot.Graphics.GraphicsQuality);
		VSyncMode.LinkSetting(SettingsRoot.Graphics.VSyncMode);
		FrameRateLimitEnabled.LinkSetting(SettingsRoot.Graphics.FrameRateLimitEnabled);
		FrameRateLimit.LinkSetting(SettingsRoot.Graphics.FrameRateLimit);
		FsrMode.LinkSetting(SettingsRoot.Graphics.FsrMode);
		FsrSharpness.LinkSetting(SettingsRoot.Graphics.FsrSharpness);
		VolumetricLightingQuality.LinkSetting(SettingsRoot.Graphics.VolumetricLightingQuality);
		ParticleSystemsLightingEnabled.LinkSetting(SettingsRoot.Graphics.ParticleSystemsLightingEnabled);
		ParticleSystemsShadowsEnabled.LinkSetting(SettingsRoot.Graphics.ParticleSystemsShadowsEnabled);
		ShadowsQuality.LinkSetting(SettingsRoot.Graphics.ShadowsQuality);
		TexturesQuality.LinkSetting(SettingsRoot.Graphics.TexturesQuality);
		DepthOfField.LinkSetting(SettingsRoot.Graphics.DepthOfField);
		Bloom.LinkSetting(SettingsRoot.Graphics.Bloom);
		SSAOQuality.LinkSetting(SettingsRoot.Graphics.SSAOQuality);
		SSRQuality.LinkSetting(SettingsRoot.Graphics.SSRQuality);
		FilmGrainEnabled.LinkSetting(SettingsRoot.Graphics.FilmGrainEnabled);
		AntialiasingMode.LinkSetting(SettingsRoot.Graphics.AntialiasingMode);
		AntialiasingQuality.LinkSetting(SettingsRoot.Graphics.AntialiasingQuality);
		FootprintsMode.LinkSetting(SettingsRoot.Graphics.FootprintsMode);
		CrowdQuality.LinkSetting(SettingsRoot.Graphics.CrowdQuality);
	}

	public void InitializeSettings()
	{
		IReadOnlyList<DisplayInfo> displayLayout = SettingsController.Instance.GraphicsSettingsController.DisplayLayout;
		List<string> localizedValues = ((displayLayout.Count > 0) ? displayLayout.Select((DisplayInfo displayInfo, int i) => $"{i + 1} - {displayInfo.name}") : UnityEngine.Display.displays.Select((Display _, int i) => $"Display {i + 1}")).ToList();
		Display.SetLocalizedValues(localizedValues);
		List<string> list = new List<string>();
		for (int j = 0; j < SettingsController.Instance.GraphicsSettingsController.RelevantResolutions.Count; j++)
		{
			Resolution resolution = SettingsController.Instance.GraphicsSettingsController.RelevantResolutions[j];
			list.Add($"{resolution.width}x{resolution.height}");
		}
		ScreenResolution.SetLocalizedValues(list);
		UpdateInteractable();
	}

	public void UpdateInteractable()
	{
		FrameRateLimitEnabled.ModificationAllowedCheck = () => SettingsRoot.Graphics.VSyncMode.GetTempValue() == VSyncModeOptions.Off;
		FrameRateLimitEnabled.ModificationAllowedReason = UIStrings.Instance.InteractableSettingsReasons.GetLabelByOrigin(SettingsNotInteractableReasonType.FrameRateLimitEnabled);
		if (SettingsRoot.Graphics.VSyncMode.GetTempValue() != 0)
		{
			FrameRateLimitEnabled.SetTempValue(value: false);
		}
		FrameRateLimit.ModificationAllowedCheck = () => SettingsRoot.Graphics.VSyncMode.GetTempValue() == VSyncModeOptions.Off && SettingsRoot.Graphics.FrameRateLimitEnabled.GetTempValue();
		FrameRateLimit.ModificationAllowedReason = UIStrings.Instance.InteractableSettingsReasons.GetLabelByOrigin(SettingsNotInteractableReasonType.FrameRateLimit);
		if (SettingsRoot.Graphics.VSyncMode.GetTempValue() != 0 || !SettingsRoot.Graphics.FrameRateLimitEnabled.GetTempValue())
		{
			FrameRateLimit.ResetToDefault();
		}
		FsrSharpness.ModificationAllowedCheck = () => SettingsRoot.Graphics.FsrMode.GetTempValue() != Kingmaker.Settings.FsrMode.Off;
		FsrSharpness.ModificationAllowedReason = UIStrings.Instance.InteractableSettingsReasons.GetLabelByOrigin(SettingsNotInteractableReasonType.FsrSharpness);
		if (SettingsRoot.Graphics.FsrMode.GetTempValue() == Kingmaker.Settings.FsrMode.Off)
		{
			FsrSharpness.SetTempValue(0f);
		}
		AntialiasingQuality.ModificationAllowedCheck = () => SettingsRoot.Graphics.AntialiasingMode.GetTempValue() == Kingmaker.Settings.AntialiasingMode.SMAA || SettingsRoot.Graphics.AntialiasingMode.GetTempValue() == Kingmaker.Settings.AntialiasingMode.TAA;
		AntialiasingQuality.ModificationAllowedReason = UIStrings.Instance.InteractableSettingsReasons.GetLabelByOrigin(SettingsNotInteractableReasonType.AntialiasingQuality);
		if (SettingsRoot.Graphics.AntialiasingMode.GetTempValue() != Kingmaker.Settings.AntialiasingMode.SMAA && SettingsRoot.Graphics.AntialiasingMode.GetTempValue() != Kingmaker.Settings.AntialiasingMode.TAA)
		{
			AntialiasingQuality.SetTempValue(QualityOption.Low);
		}
	}
}

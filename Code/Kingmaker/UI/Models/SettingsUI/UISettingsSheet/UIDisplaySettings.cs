using System;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIDisplaySettings : IUISettingsSheet
{
	public UISettingsEntityGammaCorrection GammaCorrection;

	public UISettingsEntityGammaCorrection Brightness;

	public UISettingsEntitySliderFloat Contrast;

	public UISettingsEntitySliderInt SafeZoneOffset;

	public void LinkToSettings()
	{
		GammaCorrection.LinkSetting(SettingsRoot.Display.GammaCorrection);
		Brightness.LinkSetting(SettingsRoot.Display.Brightness);
		Contrast.LinkSetting(SettingsRoot.Display.Contrast);
		SafeZoneOffset.LinkSetting(SettingsRoot.Display.SafeZoneOffset);
	}

	public void InitializeSettings()
	{
	}
}

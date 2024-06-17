using System;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIAccessiabilitySettings : IUISettingsSheet
{
	public UISettingsEntitySliderFloat Protanopia;

	public UISettingsEntitySliderFloat Deuteranopia;

	public UISettingsEntitySliderFloat Tritanopia;

	public UISettingsEntitySliderFontSize FontSize;

	public void LinkToSettings()
	{
		Protanopia.LinkSetting(SettingsRoot.Accessiability.Protanopia);
		Deuteranopia.LinkSetting(SettingsRoot.Accessiability.Deuteranopia);
		Tritanopia.LinkSetting(SettingsRoot.Accessiability.Tritanopia);
		FontSize.LinkSetting(SettingsRoot.Accessiability.GetFontSizeSettingsEntity());
	}

	public void InitializeSettings()
	{
	}
}

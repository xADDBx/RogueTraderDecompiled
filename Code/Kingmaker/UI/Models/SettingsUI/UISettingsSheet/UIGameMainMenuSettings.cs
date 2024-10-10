using System;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIGameMainMenuSettings
{
	public UISettingsEntityDropdownMainMenuTheme MainMenuTheme;

	public void LinkToSettings()
	{
		MainMenuTheme.LinkSetting(SettingsRoot.Game.MainMenu.MainMenuTheme);
	}

	public void InitializeSettings()
	{
	}
}

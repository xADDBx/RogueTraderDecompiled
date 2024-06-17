using System;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIGameAutopauseSettings : IUISettingsSheet
{
	public UISettingsEntityBool PauseOnLostFocus;

	public UISettingsEntityBool PauseOnTrapDetected;

	public UISettingsEntityBool PauseOnHiddenObjectDetected;

	public UISettingsEntityBool PauseWhenAreaLoaded;

	public UISettingsEntityBool PauseOnLoadingScreen;

	public void LinkToSettings()
	{
		PauseOnLostFocus.LinkSetting(SettingsRoot.Game.Autopause.PauseOnLostFocus);
		PauseOnTrapDetected.LinkSetting(SettingsRoot.Game.Autopause.PauseOnTrapDetected);
		PauseOnHiddenObjectDetected.LinkSetting(SettingsRoot.Game.Autopause.PauseOnHiddenObjectDetected);
		PauseWhenAreaLoaded.LinkSetting(SettingsRoot.Game.Autopause.PauseOnAreaLoaded);
		PauseOnLoadingScreen.LinkSetting(SettingsRoot.Game.Autopause.PauseOnLoadingScreen);
	}

	public void InitializeSettings()
	{
	}
}

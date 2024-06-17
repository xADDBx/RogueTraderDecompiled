using System;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIKeybindGeneralSettings : IUISettingsSheet
{
	public UISettingsEntityKeyBinding HighlightObjects;

	public UISettingsEntityKeyBinding Hold;

	public UISettingsEntityKeyBinding OpenCharacterScreen;

	public UISettingsEntityKeyBinding OpenInventory;

	public UISettingsEntityKeyBinding OpenJournal;

	public UISettingsEntityKeyBinding OpenMap;

	public UISettingsEntityKeyBinding OpenEncyclopedia;

	public UISettingsEntityKeyBinding OpenColonyManagement;

	public UISettingsEntityKeyBinding OpenShipCustomization;

	public UISettingsEntityKeyBinding OpenCargoManagement;

	public UISettingsEntityKeyBinding OpenFormation;

	public UISettingsEntityKeyBinding Pause;

	public UISettingsEntityKeyBinding QuickLoad;

	public UISettingsEntityKeyBinding QuickSave;

	public UISettingsEntityKeyBinding Screenshot;

	public UISettingsEntityKeyBinding Stop;

	public UISettingsEntityKeyBinding Unpause;

	public UISettingsEntityKeyBinding CameraUp;

	public UISettingsEntityKeyBinding CameraDown;

	public UISettingsEntityKeyBinding CameraLeft;

	public UISettingsEntityKeyBinding CameraRight;

	public UISettingsEntityKeyBinding CameraRotateLeft;

	public UISettingsEntityKeyBinding CameraRotateRight;

	public UISettingsEntityKeyBinding FollowUnit;

	public UISettingsEntityKeyBinding SkipBark;

	public UISettingsEntityKeyBinding SkipCutscene;

	public UISettingsEntityKeyBinding OpenModificationsWindow;

	public UISettingsEntityKeyBinding SpeedUpEnemiesTurn;

	public UISettingsEntityKeyBinding SwitchUIVisibility;

	public void LinkToSettings()
	{
		HighlightObjects.LinkSetting(SettingsRoot.Controls.Keybindings.General.HighlightObjects);
		Hold.LinkSetting(SettingsRoot.Controls.Keybindings.General.Hold);
		OpenCharacterScreen.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenCharacterScreen);
		OpenInventory.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenInventory);
		OpenJournal.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenJournal);
		OpenMap.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenMap);
		OpenEncyclopedia.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenEncyclopedia);
		OpenColonyManagement.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenColonyManagement);
		OpenShipCustomization.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenShipCustomization);
		OpenCargoManagement.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenCargoManagement);
		OpenFormation.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenFormation);
		Pause.LinkSetting(SettingsRoot.Controls.Keybindings.General.Pause);
		QuickLoad.LinkSetting(SettingsRoot.Controls.Keybindings.General.QuickLoad);
		QuickSave.LinkSetting(SettingsRoot.Controls.Keybindings.General.QuickSave);
		Screenshot.LinkSetting(SettingsRoot.Controls.Keybindings.General.Screenshot);
		Stop.LinkSetting(SettingsRoot.Controls.Keybindings.General.Stop);
		Unpause.LinkSetting(SettingsRoot.Controls.Keybindings.General.Unpause);
		CameraUp.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraUp);
		CameraDown.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraDown);
		CameraLeft.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraLeft);
		CameraRight.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraRight);
		CameraRotateLeft.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraRotateLeft);
		CameraRotateRight.LinkSetting(SettingsRoot.Controls.Keybindings.General.CameraRotateRight);
		FollowUnit.LinkSetting(SettingsRoot.Controls.Keybindings.General.FollowUnit);
		SkipBark.LinkSetting(SettingsRoot.Controls.Keybindings.General.SkipBark);
		SkipCutscene.LinkSetting(SettingsRoot.Controls.Keybindings.General.SkipCutscene);
		OpenModificationsWindow.LinkSetting(SettingsRoot.Controls.Keybindings.General.OpenModificationWindow);
		SpeedUpEnemiesTurn.LinkSetting(SettingsRoot.Controls.Keybindings.General.SpeedUpEnemiesTurn);
		SwitchUIVisibility.LinkSetting(SettingsRoot.Controls.Keybindings.General.SwitchUIVisibility);
	}

	public void InitializeSettings()
	{
	}
}

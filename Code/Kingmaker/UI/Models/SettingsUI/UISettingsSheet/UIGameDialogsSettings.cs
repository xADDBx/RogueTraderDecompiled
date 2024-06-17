using System;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIGameDialogsSettings : IUISettingsSheet
{
	public UISettingsEntityBool ShowItemsReceivedNotification;

	public UISettingsEntityBool ShowLocationRevealedNotification;

	public UISettingsEntityBool ShowXPGainedNotification;

	public UISettingsEntityBool ShowAlignmentShiftsInAnswer;

	public UISettingsEntityBool ShowAlignmentShiftsNotifications;

	public UISettingsEntityBool ShowAlignmentRequirements;

	public UISettingsEntityBool ShowSkillcheckDC;

	public UISettingsEntityBool ShowSkillcheckResult;

	public void LinkToSettings()
	{
		ShowItemsReceivedNotification.LinkSetting(SettingsRoot.Game.Dialogs.ShowItemsReceivedNotification);
		ShowLocationRevealedNotification.LinkSetting(SettingsRoot.Game.Dialogs.ShowLocationRevealedNotification);
		ShowXPGainedNotification.LinkSetting(SettingsRoot.Game.Dialogs.ShowXPGainedNotification);
		ShowAlignmentShiftsInAnswer.LinkSetting(SettingsRoot.Game.Dialogs.ShowAlignmentShiftsInAnswer);
		ShowAlignmentShiftsNotifications.LinkSetting(SettingsRoot.Game.Dialogs.ShowAlignmentShiftsNotifications);
		ShowAlignmentRequirements.LinkSetting(SettingsRoot.Game.Dialogs.ShowAlignmentRequirements);
		ShowSkillcheckDC.LinkSetting(SettingsRoot.Game.Dialogs.ShowSkillcheckDC);
		ShowSkillcheckResult.LinkSetting(SettingsRoot.Game.Dialogs.ShowSkillcheckResult);
	}

	public void InitializeSettings()
	{
	}
}

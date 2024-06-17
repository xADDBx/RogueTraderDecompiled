using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class GameDialogsSettings
{
	public readonly SettingsEntityBool ShowItemsReceivedNotification;

	public readonly SettingsEntityBool ShowLocationRevealedNotification;

	public readonly SettingsEntityBool ShowXPGainedNotification;

	public readonly SettingsEntityBool ShowAlignmentShiftsInAnswer;

	public readonly SettingsEntityBool ShowAlignmentShiftsNotifications;

	public readonly SettingsEntityBool ShowAlignmentRequirements;

	public readonly SettingsEntityBool ShowSkillcheckDC;

	public readonly SettingsEntityBool ShowSkillcheckResult;

	public GameDialogsSettings(ISettingsController settingsController, GameDialogsSettingsDefaultValues defaultValues)
	{
		ShowItemsReceivedNotification = new SettingsEntityBool(settingsController, "show-items-received-notifications", defaultValues.ShowItemsReceivedNotification);
		ShowLocationRevealedNotification = new SettingsEntityBool(settingsController, "show-location-reveal-notifications", defaultValues.ShowLocationRevealedNotification);
		ShowXPGainedNotification = new SettingsEntityBool(settingsController, "show-xp-gained-notifications", defaultValues.ShowXPGainedNotification);
		ShowAlignmentShiftsInAnswer = new SettingsEntityBool(settingsController, "show-alignment-shifts-in-answer", defaultValues.ShowAlignmentShiftsInAnswer);
		ShowAlignmentShiftsNotifications = new SettingsEntityBool(settingsController, "show-alignment-shift-notifications", defaultValues.ShowAlignmentShiftsNotifications);
		ShowAlignmentRequirements = new SettingsEntityBool(settingsController, "show-alignment-requirements", defaultValues.ShowAlignmentRequirements);
		ShowSkillcheckDC = new SettingsEntityBool(settingsController, "show-skillcheck-dc", defaultValues.ShowSkillcheckDC);
		ShowSkillcheckResult = new SettingsEntityBool(settingsController, "show-skillcheck-result", defaultValues.ShowSkillcheckResult);
	}
}

using Kingmaker.Settings.ConstructionHelpers.KeyPrefix;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class ControlsKeybindingsSettings
{
	public readonly ControlsKeybindingsGeneralSettings General;

	public readonly ControlsKeybindingsActionBarSettings ActionBar;

	public readonly ControlsKeybindingsDialogSettings Dialog;

	public readonly ControlsKeybindingsSelectCharacterSettings SelectCharacter;

	public ControlsKeybindingsSettings(ISettingsController settingsController, ControlsKeybindingsSettingsDefaultValues defaultValues)
	{
		using (new SettingsKeyPrefix("general"))
		{
			General = new ControlsKeybindingsGeneralSettings(settingsController, defaultValues.General);
		}
		using (new SettingsKeyPrefix("action-bar"))
		{
			ActionBar = new ControlsKeybindingsActionBarSettings(settingsController, defaultValues.ActionBar);
		}
		using (new SettingsKeyPrefix("dialog"))
		{
			Dialog = new ControlsKeybindingsDialogSettings(settingsController, defaultValues.Dialog);
		}
		using (new SettingsKeyPrefix("select-character"))
		{
			SelectCharacter = new ControlsKeybindingsSelectCharacterSettings(settingsController, defaultValues.SelectCharacter);
		}
	}
}

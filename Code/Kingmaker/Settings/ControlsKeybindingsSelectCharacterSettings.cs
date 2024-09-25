using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class ControlsKeybindingsSelectCharacterSettings
{
	public readonly SettingsEntityKeyBindingPair[] SelectCharacter;

	public readonly SettingsEntityKeyBindingPair SelectAll;

	public ControlsKeybindingsSelectCharacterSettings(ISettingsController settingsController, ControlsKeybindingsSelectCharacterSettingsDefaultValues defaultValues)
	{
		SelectCharacter = new SettingsEntityKeyBindingPair[6];
		for (int i = 0; i < SelectCharacter.Length; i++)
		{
			SelectCharacter[i] = new SettingsEntityKeyBindingPair(settingsController, "select-character-" + i, defaultValues.SelectCharacter[i]);
		}
		SelectAll = new SettingsEntityKeyBindingPair(settingsController, "select-all", defaultValues.SelectAll);
	}
}

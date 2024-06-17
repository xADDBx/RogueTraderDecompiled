using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class ControlsKeybindingsDialogSettings
{
	public readonly SettingsEntityKeyBindingPair[] DialogChoices;

	public readonly SettingsEntityKeyBindingPair NextOrEnd;

	public ControlsKeybindingsDialogSettings(ISettingsController settingsController, ControlsKeybindingsDialogSettingsDefaultValues defaultValues)
	{
		DialogChoices = new SettingsEntityKeyBindingPair[10];
		for (int i = 0; i < DialogChoices.Length; i++)
		{
			DialogChoices[i] = new SettingsEntityKeyBindingPair(settingsController, "dialog-choice-" + i, defaultValues.DialogChoices[i]);
		}
		NextOrEnd = new SettingsEntityKeyBindingPair(settingsController, "next-or-end", defaultValues.NextOrEnd);
	}
}

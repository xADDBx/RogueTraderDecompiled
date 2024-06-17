using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class GameSaveSettings
{
	public readonly SettingsEntityBool AutosaveEnabled;

	public readonly SettingsEntityInt AutosaveSlots;

	public readonly SettingsEntityInt QuicksaveSlots;

	public GameSaveSettings(ISettingsController settingsController, GameSaveSettingsDefaultValues defaultValues)
	{
		AutosaveEnabled = new SettingsEntityBool(settingsController, "autosave-enabled", defaultValues.AutosaveEnabled);
		AutosaveSlots = new SettingsEntityInt(settingsController, "autosave-slots", defaultValues.AutosaveSlots);
		QuicksaveSlots = new SettingsEntityInt(settingsController, "quicksave-slots", defaultValues.QuicksaveSlots);
	}
}

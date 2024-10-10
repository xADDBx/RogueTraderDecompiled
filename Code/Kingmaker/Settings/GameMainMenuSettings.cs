using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class GameMainMenuSettings
{
	public readonly SettingsEntityEnum<MainMenuTheme> MainMenuTheme;

	public GameMainMenuSettings(ISettingsController settingsController, GameMainMenuSettingsDefaultValues defaultValues)
	{
		MainMenuTheme = new SettingsEntityEnum<MainMenuTheme>(settingsController, "main-menu-theme", defaultValues.MainMenuTheme, saveDependent: false, requireReboot: true);
	}
}

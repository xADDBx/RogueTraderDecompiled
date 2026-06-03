using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Tutorial;

namespace Kingmaker.Settings;

public class GameSwitchSettings
{
	public readonly SettingsEntityBool SwitchJoyConAsMouse;

	public BlueprintTutorial JoyConDeattachTutorial;

	public GameSwitchSettings(ISettingsController settingsController, GameSwitchSettingsDefaultValues defaultValues)
	{
		SwitchJoyConAsMouse = new SettingsEntityBool(settingsController, "switch-joy-con-as-mouse", defaultValues.SwitchJoyConAsMouse);
	}
}

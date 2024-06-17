using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class GameAutopauseSettings
{
	public readonly SettingsEntityBool PauseOnLostFocus;

	public readonly SettingsEntityBool PauseOnTrapDetected;

	public readonly SettingsEntityBool PauseOnHiddenObjectDetected;

	public readonly SettingsEntityBool PauseOnAreaLoaded;

	public readonly SettingsEntityBool PauseOnLoadingScreen;

	public GameAutopauseSettings(ISettingsController settingsController, GameAutopauseSettingsDefaultValues defaultValues)
	{
		PauseOnLostFocus = new SettingsEntityBool(settingsController, "pause-on-lost-focus", defaultValues.PauseOnLostFocus);
		PauseOnTrapDetected = new SettingsEntityBool(settingsController, "pause-on-trap-detected", defaultValues.PauseOnTrapDetected);
		PauseOnHiddenObjectDetected = new SettingsEntityBool(settingsController, "pause-on-hidden-object-detected", defaultValues.PauseOnHiddenObjectDetected);
		PauseOnAreaLoaded = new SettingsEntityBool(settingsController, "pause-on-area-loaded", defaultValues.PauseOnAreaLoaded);
		PauseOnLoadingScreen = new SettingsEntityBool(settingsController, "pause-on-loading-screen", defaultValues.PauseOnLoadingScreen);
	}

	public AutoPauseValues ExtractValues()
	{
		return new AutoPauseValues
		{
			PauseOnLostFocus = PauseOnLostFocus,
			PauseOnTrapDetected = PauseOnTrapDetected,
			PauseOnHiddenObjectDetected = PauseOnHiddenObjectDetected,
			PauseOnAreaLoaded = PauseOnAreaLoaded,
			PauseOnLoadingScreen = PauseOnLoadingScreen
		};
	}

	public void SetValuesAndConfirm(AutoPauseValues values)
	{
		PauseOnLostFocus.SetValueAndConfirm(values.PauseOnLostFocus);
		PauseOnTrapDetected.SetValueAndConfirm(values.PauseOnTrapDetected);
		PauseOnHiddenObjectDetected.SetValueAndConfirm(values.PauseOnHiddenObjectDetected);
		PauseOnAreaLoaded.SetValueAndConfirm(values.PauseOnAreaLoaded);
		PauseOnLoadingScreen.SetValueAndConfirm(values.PauseOnLoadingScreen);
	}
}

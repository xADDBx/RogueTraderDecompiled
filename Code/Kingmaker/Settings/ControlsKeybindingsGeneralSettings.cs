using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class ControlsKeybindingsGeneralSettings
{
	public readonly SettingsEntityKeyBindingPair HighlightObjects;

	public readonly SettingsEntityKeyBindingPair Hold;

	public readonly SettingsEntityKeyBindingPair OpenCharacterScreen;

	public readonly SettingsEntityKeyBindingPair OpenInventory;

	public readonly SettingsEntityKeyBindingPair OpenJournal;

	public readonly SettingsEntityKeyBindingPair OpenMap;

	public readonly SettingsEntityKeyBindingPair OpenEncyclopedia;

	public readonly SettingsEntityKeyBindingPair OpenColonyManagement;

	public readonly SettingsEntityKeyBindingPair OpenShipCustomization;

	public readonly SettingsEntityKeyBindingPair OpenCargoManagement;

	public readonly SettingsEntityKeyBindingPair OpenFormation;

	public readonly SettingsEntityKeyBindingPair Pause;

	public readonly SettingsEntityKeyBindingPair QuickLoad;

	public readonly SettingsEntityKeyBindingPair QuickSave;

	public readonly SettingsEntityKeyBindingPair Screenshot;

	public readonly SettingsEntityKeyBindingPair Stop;

	public readonly SettingsEntityKeyBindingPair Unpause;

	public readonly SettingsEntityKeyBindingPair CameraUp;

	public readonly SettingsEntityKeyBindingPair CameraDown;

	public readonly SettingsEntityKeyBindingPair CameraLeft;

	public readonly SettingsEntityKeyBindingPair CameraRight;

	public readonly SettingsEntityKeyBindingPair CameraRotateLeft;

	public readonly SettingsEntityKeyBindingPair CameraRotateRight;

	public readonly SettingsEntityKeyBindingPair FollowUnit;

	public readonly SettingsEntityKeyBindingPair SkipBark;

	public readonly SettingsEntityKeyBindingPair SkipCutscene;

	public readonly SettingsEntityKeyBindingPair OpenModificationWindow;

	public readonly SettingsEntityKeyBindingPair SpeedUpEnemiesTurn;

	public readonly SettingsEntityKeyBindingPair SwitchUIVisibility;

	public ControlsKeybindingsGeneralSettings(ISettingsController settingsController, ControlsKeybindingsGeneralSettingsDefaultValues defaultValues)
	{
		HighlightObjects = new SettingsEntityKeyBindingPair(settingsController, "highlight-objects", defaultValues.HighlightObjects);
		Hold = new SettingsEntityKeyBindingPair(settingsController, "hold", defaultValues.Hold);
		OpenCharacterScreen = new SettingsEntityKeyBindingPair(settingsController, "open-character-screen", defaultValues.OpenCharacterScreen);
		OpenInventory = new SettingsEntityKeyBindingPair(settingsController, "open-inventory", defaultValues.OpenInventory);
		OpenJournal = new SettingsEntityKeyBindingPair(settingsController, "open-journal", defaultValues.OpenJournal);
		OpenMap = new SettingsEntityKeyBindingPair(settingsController, "open-map", defaultValues.OpenMap);
		OpenEncyclopedia = new SettingsEntityKeyBindingPair(settingsController, "open-encyclopedia", defaultValues.OpenEncyclopedia);
		OpenColonyManagement = new SettingsEntityKeyBindingPair(settingsController, "open-colony-management", defaultValues.OpenColonyManagement);
		OpenShipCustomization = new SettingsEntityKeyBindingPair(settingsController, "open-ship-customization", defaultValues.OpenShipCustomization);
		OpenCargoManagement = new SettingsEntityKeyBindingPair(settingsController, "open-cargo-management", defaultValues.OpenCargoManagement);
		OpenFormation = new SettingsEntityKeyBindingPair(settingsController, "open-formation", defaultValues.OpenFormation);
		Pause = new SettingsEntityKeyBindingPair(settingsController, "pause", defaultValues.Pause);
		Unpause = new SettingsEntityKeyBindingPair(settingsController, "unpause", defaultValues.Unpause);
		QuickLoad = new SettingsEntityKeyBindingPair(settingsController, "quick-load", defaultValues.QuickLoad);
		QuickSave = new SettingsEntityKeyBindingPair(settingsController, "quick-save", defaultValues.QuickSave);
		Screenshot = new SettingsEntityKeyBindingPair(settingsController, "screenshot", defaultValues.Screenshot);
		Stop = new SettingsEntityKeyBindingPair(settingsController, "stop", defaultValues.Stop);
		CameraUp = new SettingsEntityKeyBindingPair(settingsController, "camera-up", defaultValues.CameraUp);
		CameraDown = new SettingsEntityKeyBindingPair(settingsController, "camera-down", defaultValues.CameraDown);
		CameraLeft = new SettingsEntityKeyBindingPair(settingsController, "camera-left", defaultValues.CameraLeft);
		CameraRight = new SettingsEntityKeyBindingPair(settingsController, "camera-right", defaultValues.CameraRight);
		CameraRotateLeft = new SettingsEntityKeyBindingPair(settingsController, "camera-rotate-left", defaultValues.CameraRotateLeft);
		CameraRotateRight = new SettingsEntityKeyBindingPair(settingsController, "camera-rotate-right", defaultValues.CameraRotateRight);
		FollowUnit = new SettingsEntityKeyBindingPair(settingsController, "follow-unit", defaultValues.FollowUnit);
		SkipBark = new SettingsEntityKeyBindingPair(settingsController, "skip-bark", defaultValues.SkipBark);
		SkipCutscene = new SettingsEntityKeyBindingPair(settingsController, "skip-cutscene", defaultValues.SkipCutscene);
		OpenModificationWindow = new SettingsEntityKeyBindingPair(settingsController, "open-modification-window", defaultValues.OpenModificationWindow);
		SpeedUpEnemiesTurn = new SettingsEntityKeyBindingPair(settingsController, "speed-up-enemies-turn", defaultValues.SpeedUpEnemiesTurn);
		SwitchUIVisibility = new SettingsEntityKeyBindingPair(settingsController, "switch-ui-visibility", defaultValues.SwitchUIVisibility);
	}
}

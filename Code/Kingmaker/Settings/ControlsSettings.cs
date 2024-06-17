using Kingmaker.Settings.ConstructionHelpers.KeyPrefix;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class ControlsSettings
{
	public readonly SettingsEntityFloat MouseSensitivity;

	public readonly SettingsEntityFloat MouseClickDelay;

	public readonly SettingsEntityBool ScreenEdgeScrolling;

	public readonly SettingsEntityFloat CameraScrollSpeedEdge;

	public readonly SettingsEntityFloat CameraScrollSpeedKeyboard;

	public readonly SettingsEntityFloat CameraRotationSpeedEdge;

	public readonly SettingsEntityFloat CameraRotationSpeedKeyboard;

	public readonly SettingsEntityBool CameraScrollOutOfScreenEnabled;

	public readonly SettingsEntityEnum<MouseRightButtonFunction> MouseRightButtonFunction;

	public readonly SettingsEntityBool ConvertSnapLogic;

	public readonly SettingsEntityBool CameraFollowsUnit;

	public readonly ControlsKeybindingsSettings Keybindings;

	public ControlsSettings(ISettingsController settingsController, ControlsSettingsDefaultValues defaultValues)
	{
		MouseSensitivity = new SettingsEntityFloat(settingsController, "mouse-sensitivity", defaultValues.MouseSensitivity);
		MouseClickDelay = new SettingsEntityFloat(settingsController, "mouse-click-delay", defaultValues.MouseClickDelay);
		ScreenEdgeScrolling = new SettingsEntityBool(settingsController, "screen-edge-scrolling", defaultValues.ScreenEdgeScrolling);
		CameraScrollSpeedEdge = new SettingsEntityFloat(settingsController, "camera-scroll-speed-edge", defaultValues.CameraScrollSpeedEdge);
		CameraScrollSpeedKeyboard = new SettingsEntityFloat(settingsController, "camera-scroll-speed-keyboard", defaultValues.CameraScrollSpeedKeyboard);
		CameraRotationSpeedEdge = new SettingsEntityFloat(settingsController, "camera-rotation-speed-edge", defaultValues.CameraRotationSpeedEdge);
		CameraRotationSpeedKeyboard = new SettingsEntityFloat(settingsController, "camera-rotation-speed-keyboard", defaultValues.CameraRotationSpeedKeyboard);
		CameraScrollOutOfScreenEnabled = new SettingsEntityBool(settingsController, "camera-scroll-out-of-screen-enabled", defaultValues.CameraScrollOutOfScreenEnabled);
		MouseRightButtonFunction = new SettingsEntityEnum<MouseRightButtonFunction>(settingsController, "mouse-right-button-function", defaultValues.MouseRightButtonFunction);
		ConvertSnapLogic = new SettingsEntityBool(settingsController, "convert-snap-logic", defaultValues.ConvertSnapLogic);
		CameraFollowsUnit = new SettingsEntityBool(settingsController, "camera-follow-unit", defaultValues.CameraFollowsUnit);
		using (new SettingsKeyPrefix("keybindings"))
		{
			Keybindings = new ControlsKeybindingsSettings(settingsController, defaultValues.Keybindings);
		}
	}
}

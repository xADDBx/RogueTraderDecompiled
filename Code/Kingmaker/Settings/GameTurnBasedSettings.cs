using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class GameTurnBasedSettings
{
	public readonly bool EnableTurnBasedMode = true;

	public readonly SettingsEntityBool AutoEndTurn;

	public readonly SettingsEntityBool CameraFollowUnit;

	public readonly SettingsEntityBool CameraScrollToCurrentUnit;

	public readonly SettingsEntityEnum<SpeedUpMode> SpeedUpMode;

	public readonly SettingsEntityBool FastMovement;

	public readonly SettingsEntityBool FastPartyCast;

	public readonly SettingsEntityBool DisableActionCamera;

	public readonly SettingsEntityFloat TimeScaleInPlayerTurn;

	public readonly SettingsEntityFloat TimeScaleInNonPlayerTurn;

	public readonly SettingsEntityBool AutoSelectWeaponAbility;

	public GameTurnBasedSettings(ISettingsController settingsController, GameTurnBasedSettingsDefaultValues defaultValues)
	{
		CameraFollowUnit = new SettingsEntityBool(settingsController, "camera-follow-unit", defaultValues.CameraFollowUnit);
		CameraScrollToCurrentUnit = new SettingsEntityBool(settingsController, "camera-scroll-to-current-unit", defaultValues.CameraScrollToCurrentUnit);
		SpeedUpMode = new SettingsEntityEnum<SpeedUpMode>(settingsController, "speed-up-mode-2", defaultValues.SpeedUpMode);
		FastMovement = new SettingsEntityBool(settingsController, "fast-movement-2", defaultValues.FastMovement);
		FastPartyCast = new SettingsEntityBool(settingsController, "fast-party-cast-2", defaultValues.FastPartyCast);
		DisableActionCamera = new SettingsEntityBool(settingsController, "disable-action-camera", defaultValues.DisableActionCamera);
		TimeScaleInPlayerTurn = new SettingsEntityFloat(settingsController, "time-scale-player-2", defaultValues.TimeScaleInPlayerTurn);
		TimeScaleInNonPlayerTurn = new SettingsEntityFloat(settingsController, "time-scale-non-player-2", defaultValues.TimeScaleInNonPlayerTurn);
		AutoSelectWeaponAbility = new SettingsEntityBool(settingsController, "auto-select-weapon-ability", defaultValues.AutoSelectWeaponAbility);
		AutoEndTurn = new SettingsEntityBool(settingsController, "auto-end-turn-surface", defaultValues.AutoEndTurn);
	}

	public GameTurnBasedValues ExtractValues()
	{
		return new GameTurnBasedValues
		{
			SpeedUpMode = SpeedUpMode,
			FastMovement = FastMovement,
			FastPartyCast = FastPartyCast,
			DisableActionCamera = DisableActionCamera,
			TimeScaleInPlayerTurn = TimeScaleInPlayerTurn,
			TimeScaleInNonPlayerTurn = TimeScaleInNonPlayerTurn
		};
	}

	public void SetValuesAndConfirm(GameTurnBasedValues values)
	{
		SpeedUpMode.SetValueAndConfirm(values.SpeedUpMode);
		FastMovement.SetValueAndConfirm(values.FastMovement);
		FastPartyCast.SetValueAndConfirm(values.FastPartyCast);
		DisableActionCamera.SetValueAndConfirm(values.DisableActionCamera);
		TimeScaleInPlayerTurn.SetValueAndConfirm(values.TimeScaleInPlayerTurn);
		TimeScaleInNonPlayerTurn.SetValueAndConfirm(values.TimeScaleInNonPlayerTurn);
	}
}

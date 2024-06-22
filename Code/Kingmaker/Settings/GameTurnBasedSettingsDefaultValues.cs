using System;

namespace Kingmaker.Settings;

[Serializable]
public class GameTurnBasedSettingsDefaultValues
{
	public bool EnableTurnBasedMode;

	public bool AutoEndTurn = true;

	public bool AutoStopAfterFirstMoveAction;

	public bool CameraFollowUnit = true;

	public bool CameraScrollToCurrentUnit = true;

	public bool EnableTurnBaseCombatText = true;

	public SpeedUpMode SpeedUpMode = SpeedUpMode.OnDemand;

	public bool FastMovement = true;

	public bool FastPartyCast = true;

	public bool DisableActionCamera;

	public float TimeScaleInPlayerTurn = 5f;

	public float TimeScaleInNonPlayerTurn = 5f;

	public bool AutoSelectWeaponAbility = true;
}

using System;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIGameTurnBasedSettings : IUISettingsSheet
{
	public UISettingsEntityBool AutoEndTurn;

	public UISettingsEntityBool CameraFollowUnit;

	public UISettingsEntityBool CameraScrollToCurrentUnit;

	public UISettingsEntityDropdownSpeedUpMode SpeedUpMode;

	public UISettingsEntityBool FastMovement;

	public UISettingsEntityBool FastPartyCast;

	public UISettingsEntitySliderFloat TimeScaleInPlayerTurn;

	public UISettingsEntitySliderFloat TimeScaleInNonPlayerTurn;

	public UISettingsEntityBool AutoSelectWeaponAbility;

	public void LinkToSettings()
	{
		AutoEndTurn.LinkSetting(SettingsRoot.Game.TurnBased.AutoEndTurn);
		CameraFollowUnit.LinkSetting(SettingsRoot.Game.TurnBased.CameraFollowUnit);
		CameraScrollToCurrentUnit.LinkSetting(SettingsRoot.Game.TurnBased.CameraScrollToCurrentUnit);
		SpeedUpMode.LinkSetting(SettingsRoot.Game.TurnBased.SpeedUpMode);
		FastMovement.LinkSetting(SettingsRoot.Game.TurnBased.FastMovement);
		FastPartyCast.LinkSetting(SettingsRoot.Game.TurnBased.FastPartyCast);
		TimeScaleInPlayerTurn.LinkSetting(SettingsRoot.Game.TurnBased.TimeScaleInPlayerTurn);
		TimeScaleInNonPlayerTurn.LinkSetting(SettingsRoot.Game.TurnBased.TimeScaleInNonPlayerTurn);
		AutoSelectWeaponAbility.LinkSetting(SettingsRoot.Game.TurnBased.AutoSelectWeaponAbility);
	}

	public void InitializeSettings()
	{
	}
}

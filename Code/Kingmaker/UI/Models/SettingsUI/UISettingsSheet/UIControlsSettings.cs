using System;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIControlsSettings : IUISettingsSheet
{
	public UISettingsEntitySliderFloat MouseSensitivity;

	public UISettingsEntitySliderFloat MouseClickDelay;

	public UISettingsEntityBool ScreenEdgeScrolling;

	public UISettingsEntitySliderFloat CameraScrollSpeedEdge;

	public UISettingsEntitySliderFloat CameraScrollSpeedKeyboard;

	public UISettingsEntitySliderFloat CameraRotationSpeedEdge;

	public UISettingsEntitySliderFloat CameraRotationSpeedKeyboard;

	public UISettingsEntityBool CameraScrollOutOfScreenEnabled;

	public UISettingsEntityDropdownMouseRightButtonFunction MouseRightButtonFunction;

	public UISettingsEntityBool ConvertSnapLogic;

	public UISettingsEntityBool CameraFollowsUnit;

	public void LinkToSettings()
	{
		MouseSensitivity.LinkSetting(SettingsRoot.Controls.MouseSensitivity);
		MouseClickDelay.LinkSetting(SettingsRoot.Controls.MouseClickDelay);
		ScreenEdgeScrolling.LinkSetting(SettingsRoot.Controls.ScreenEdgeScrolling);
		CameraScrollSpeedEdge.LinkSetting(SettingsRoot.Controls.CameraScrollSpeedEdge);
		CameraScrollSpeedKeyboard.LinkSetting(SettingsRoot.Controls.CameraScrollSpeedKeyboard);
		CameraRotationSpeedEdge.LinkSetting(SettingsRoot.Controls.CameraRotationSpeedEdge);
		CameraRotationSpeedKeyboard.LinkSetting(SettingsRoot.Controls.CameraRotationSpeedKeyboard);
		CameraScrollOutOfScreenEnabled.LinkSetting(SettingsRoot.Controls.CameraScrollOutOfScreenEnabled);
		MouseRightButtonFunction.LinkSetting(SettingsRoot.Controls.MouseRightButtonFunction);
		ConvertSnapLogic.LinkSetting(SettingsRoot.Controls.ConvertSnapLogic);
		CameraFollowsUnit.LinkSetting(SettingsRoot.Controls.CameraFollowsUnit);
	}

	public void InitializeSettings()
	{
	}
}

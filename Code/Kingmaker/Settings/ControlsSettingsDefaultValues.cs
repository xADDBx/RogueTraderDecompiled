using System;

namespace Kingmaker.Settings;

[Serializable]
public class ControlsSettingsDefaultValues : IValidatable
{
	public float MouseSensitivity;

	public float MouseClickDelay;

	public bool ScreenEdgeScrolling;

	public float CameraScrollSpeedEdge;

	public float CameraScrollSpeedKeyboard;

	public float CameraRotationSpeedEdge;

	public float CameraRotationSpeedKeyboard;

	public bool CameraScrollOutOfScreenEnabled;

	public MouseRightButtonFunction MouseRightButtonFunction;

	public bool ConvertSnapLogic;

	public bool CameraFollowsUnit;

	public ControlsKeybindingsSettingsDefaultValues Keybindings;

	public void OnValidate()
	{
		Keybindings.OnValidate();
	}
}

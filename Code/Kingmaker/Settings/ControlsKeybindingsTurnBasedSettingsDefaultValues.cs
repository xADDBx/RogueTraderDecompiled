using System;
using Kingmaker.Settings.Entities;

namespace Kingmaker.Settings;

[Serializable]
public class ControlsKeybindingsTurnBasedSettingsDefaultValues : IValidatable
{
	public KeyBindingPair ChangeCursorAction;

	public KeyBindingPair ModifyMovementLimit;

	public KeyBindingPair SwitchTBM;

	public void OnValidate()
	{
	}
}

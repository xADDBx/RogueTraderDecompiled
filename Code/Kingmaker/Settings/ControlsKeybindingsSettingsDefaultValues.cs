using System;

namespace Kingmaker.Settings;

[Serializable]
public class ControlsKeybindingsSettingsDefaultValues : IValidatable
{
	public ControlsKeybindingsGeneralSettingsDefaultValues General;

	public ControlsKeybindingsActionBarSettingsDefaultValues ActionBar;

	public ControlsKeybindingsDialogSettingsDefaultValues Dialog;

	public ControlsKeybindingsSelectCharacterSettingsDefaultValues SelectCharacter;

	public ControlsKeybindingsTurnBasedSettingsDefaultValues TurnBased;

	public void OnValidate()
	{
		ActionBar.OnValidate();
		Dialog.OnValidate();
		SelectCharacter.OnValidate();
		TurnBased.OnValidate();
	}
}

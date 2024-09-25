using System;
using Kingmaker.Settings.Entities;

namespace Kingmaker.Settings;

[Serializable]
public class ControlsKeybindingsDialogSettingsDefaultValues : IValidatable
{
	public KeyBindingPair[] DialogChoices;

	public KeyBindingPair NextOrEnd;

	public void OnValidate()
	{
		if (DialogChoices.Length != 10)
		{
			DialogChoices = new KeyBindingPair[10];
		}
	}
}

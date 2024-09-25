using System;
using Kingmaker.Settings.Entities;

namespace Kingmaker.Settings;

[Serializable]
public class ControlsKeybindingsSelectCharacterSettingsDefaultValues : IValidatable
{
	public KeyBindingPair[] SelectCharacter;

	public KeyBindingPair SelectAll;

	public void OnValidate()
	{
		if (SelectCharacter.Length != 6)
		{
			SelectCharacter = new KeyBindingPair[6];
		}
	}
}

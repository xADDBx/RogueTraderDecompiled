using System;
using Kingmaker.Settings.Entities;
using UnityEngine.Serialization;

namespace Kingmaker.Settings;

[Serializable]
public class ControlsKeybindingsActionBarSettingsDefaultValues : IValidatable
{
	[FormerlySerializedAs("ActionBarWeaponSet")]
	public KeyBindingPair ChangeWeaponSet;

	public KeyBindingPair[] ActionBarConsumables;

	public KeyBindingPair[] ActionBarWeapons;

	public KeyBindingPair[] ActionBarAbilities;

	public void OnValidate()
	{
		if (ActionBarConsumables.Length != 4)
		{
			ActionBarConsumables = new KeyBindingPair[4];
		}
		if (ActionBarWeapons.Length != 12)
		{
			ActionBarWeapons = new KeyBindingPair[12];
		}
		if (ActionBarAbilities.Length != 20)
		{
			ActionBarAbilities = new KeyBindingPair[20];
		}
	}
}

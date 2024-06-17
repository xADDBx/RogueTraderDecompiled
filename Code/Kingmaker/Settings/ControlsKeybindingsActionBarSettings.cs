using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Settings;

public class ControlsKeybindingsActionBarSettings
{
	public readonly SettingsEntityKeyBindingPair ChangeWeaponSet;

	public readonly SettingsEntityKeyBindingPair[] ActionBarConsumables;

	public readonly SettingsEntityKeyBindingPair[] ActionBarWeapons;

	public readonly SettingsEntityKeyBindingPair[] ActionBarAbilities;

	private readonly string m_ConsumableKeyBindingPrefix = "action-bar-consumable-button-";

	private readonly string m_WeaponKeyBindingPrefix = "action-bar-weapon-button-";

	private readonly string m_AbilityKeyBindingPrefix = "action-bar-ability-button-";

	private readonly ISettingsController m_SettingsController;

	public ControlsKeybindingsActionBarSettings(ISettingsController settingsController, ControlsKeybindingsActionBarSettingsDefaultValues defaultValues)
	{
		m_SettingsController = settingsController;
		ChangeWeaponSet = new SettingsEntityKeyBindingPair(settingsController, "change-weapon-set", defaultValues.ChangeWeaponSet);
		ActionBarConsumables = new SettingsEntityKeyBindingPair[4];
		ActionBarWeapons = new SettingsEntityKeyBindingPair[12];
		ActionBarAbilities = new SettingsEntityKeyBindingPair[20];
		InitSettingsEntityKeyBindingPairs(defaultValues.ActionBarConsumables, ActionBarConsumables, m_ConsumableKeyBindingPrefix);
		InitSettingsEntityKeyBindingPairs(defaultValues.ActionBarWeapons, ActionBarWeapons, m_WeaponKeyBindingPrefix);
		InitSettingsEntityKeyBindingPairs(defaultValues.ActionBarAbilities, ActionBarAbilities, m_AbilityKeyBindingPrefix);
	}

	public SettingsEntityKeyBindingPair GetBindingPair(string key)
	{
		return ActionBarConsumables.FindOrDefault((SettingsEntityKeyBindingPair b) => b.Key.Contains(key)) ?? ActionBarWeapons.FindOrDefault((SettingsEntityKeyBindingPair b) => b.Key.Contains(key)) ?? ActionBarAbilities.FindOrDefault((SettingsEntityKeyBindingPair b) => b.Key.Contains(key));
	}

	public SettingsEntityKeyBindingPair GetConsumableBindingPair(string key)
	{
		return ActionBarConsumables.FindOrDefault((SettingsEntityKeyBindingPair b) => b.Key.Contains(key));
	}

	public SettingsEntityKeyBindingPair GetWeaponBindingPair(string key)
	{
		return ActionBarWeapons.FindOrDefault((SettingsEntityKeyBindingPair b) => b.Key.Contains(key));
	}

	public SettingsEntityKeyBindingPair GetAbilityBindingPair(string key)
	{
		return ActionBarAbilities.FindOrDefault((SettingsEntityKeyBindingPair b) => b.Key.Contains(key));
	}

	private void InitSettingsEntityKeyBindingPairs(KeyBindingPair[] defaultValues, SettingsEntityKeyBindingPair[] keyBindingPairs, string keyBindingPrefix)
	{
		for (int i = 0; i < keyBindingPairs.Length; i++)
		{
			keyBindingPairs[i] = new SettingsEntityKeyBindingPair(m_SettingsController, keyBindingPrefix + i, defaultValues[i]);
		}
	}
}

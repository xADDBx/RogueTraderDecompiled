using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class GameCombatTextsSettings
{
	public readonly SettingsEntityEnum<EntitiesType> ShowSpellName;

	public readonly SettingsEntityEnum<EntitiesType> ShowAvoid;

	public readonly SettingsEntityEnum<EntitiesType> ShowMiss;

	public readonly SettingsEntityEnum<EntitiesType> ShowAttackOfOpportunity;

	public readonly SettingsEntityEnum<EntitiesType> ShowCriticalHit;

	public readonly SettingsEntityEnum<EntitiesType> ShowSneakAttack;

	public readonly SettingsEntityEnum<EntitiesType> ShowDamage;

	public readonly SettingsEntityEnum<EntitiesType> ShowSaves;

	public GameCombatTextsSettings(ISettingsController settingsController, GameCombatTextsSettingsDefaultValues defaultValues)
	{
		ShowSpellName = new SettingsEntityEnum<EntitiesType>(settingsController, "show-spell-name", defaultValues.ShowSpellName);
		ShowAvoid = new SettingsEntityEnum<EntitiesType>(settingsController, "show-avoid", defaultValues.ShowAvoid);
		ShowMiss = new SettingsEntityEnum<EntitiesType>(settingsController, "show-miss", defaultValues.ShowMiss);
		ShowAttackOfOpportunity = new SettingsEntityEnum<EntitiesType>(settingsController, "show-attack-of-opportunity", defaultValues.ShowAttackOfOpportunity);
		ShowCriticalHit = new SettingsEntityEnum<EntitiesType>(settingsController, "show-critical-hit", defaultValues.ShowCriticalHit);
		ShowSneakAttack = new SettingsEntityEnum<EntitiesType>(settingsController, "show-sneak-attack", defaultValues.ShowSneakAttack);
		ShowDamage = new SettingsEntityEnum<EntitiesType>(settingsController, "show-damage", defaultValues.ShowDamage);
		ShowSaves = new SettingsEntityEnum<EntitiesType>(settingsController, "show-saves", defaultValues.ShowSaves);
	}
}

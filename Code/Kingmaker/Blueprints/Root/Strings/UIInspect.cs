using System;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIInspect
{
	[Header("Surface")]
	public LocalizedString Wounds;

	public LocalizedString ParryChance;

	public LocalizedString CoverMagnitude;

	public LocalizedString DamageDeflection;

	public LocalizedString Armor;

	public LocalizedString Dodge;

	public LocalizedString MovePoints;

	public LocalizedString CharacterStatsTitle;

	public LocalizedString StatusEffectsTitle;

	public LocalizedString NoStatusEffects;

	public LocalizedString EffectsAlly;

	public LocalizedString EffectsEnemy;

	public LocalizedString EffectsDOT;

	public LocalizedString WeaponsTitle;

	public LocalizedString AbilitiesTitle;

	public LocalizedString ActiveAbilitiesTitle;

	public LocalizedString PassiveAbilitiesTitle;

	public LocalizedString NoAbilities;

	public LocalizedString FeaturesTitle;

	public LocalizedString NoFeatures;

	public LocalizedString UltimateAbilitiesTitle;

	public LocalizedString ToggleSquad;

	public LocalizedString UnconditionalModifiers;

	[Header("Space")]
	public LocalizedString ShipHP;

	public LocalizedString Number;

	public LocalizedString Evasion;

	public LocalizedString HitChance;

	public LocalizedString HitChanceDescription;

	public LocalizedString CriticalChance;

	public LocalizedString CriticalChanceDescription;

	public LocalizedString ArmoursAndShields;

	public LocalizedString Fore;

	public LocalizedString Aft;

	public LocalizedString Port;

	public LocalizedString Starboard;

	public LocalizedString Armours;

	public LocalizedString ArmourFore;

	public LocalizedString ArmourAft;

	public LocalizedString ArmourPort;

	public LocalizedString ArmourStarboard;

	public LocalizedString Shields;

	public LocalizedString ShieldFore;

	public LocalizedString ShieldAft;

	public LocalizedString ShieldPort;

	public LocalizedString ShieldStarboard;

	public LocalizedString WeaponSlotPort;

	public LocalizedString WeaponSlotStarboard;

	public LocalizedString WeaponSlotKeel;

	public LocalizedString PsyRating;

	public string GetArmorStringByType(StarshipHitLocation type)
	{
		return type switch
		{
			StarshipHitLocation.Fore => ArmourFore, 
			StarshipHitLocation.Aft => ArmourAft, 
			StarshipHitLocation.Port => ArmourPort, 
			StarshipHitLocation.Starboard => ArmourStarboard, 
			_ => null, 
		};
	}

	public string GetShieldStringByType(StarshipSectorShieldsType type)
	{
		return type switch
		{
			StarshipSectorShieldsType.Fore => ShieldFore, 
			StarshipSectorShieldsType.Aft => ShieldAft, 
			StarshipSectorShieldsType.Port => ShieldPort, 
			StarshipSectorShieldsType.Starboard => ShieldStarboard, 
			_ => null, 
		};
	}

	public string GetWeaponSlotStringByType(WeaponSlotType type)
	{
		return type switch
		{
			WeaponSlotType.Dorsal => UIStrings.Instance.SpaceCombatTexts.DorsalAbilitiesGroupLabel, 
			WeaponSlotType.Prow => UIStrings.Instance.SpaceCombatTexts.ProwAbilitiesGroupLabel, 
			WeaponSlotType.Port => WeaponSlotPort, 
			WeaponSlotType.Starboard => WeaponSlotStarboard, 
			WeaponSlotType.Keel => WeaponSlotKeel, 
			_ => null, 
		};
	}
}

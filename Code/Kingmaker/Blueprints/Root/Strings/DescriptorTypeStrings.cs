using System;
using Kingmaker.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

public class DescriptorTypeStrings : StringsContainer
{
	public LocalizedString None;

	public LocalizedString Racial;

	public LocalizedString Difficulty;

	public LocalizedString BaseStatBonus;

	public LocalizedString Polymorph;

	public LocalizedString UniqueItem;

	public LocalizedString RighteousFury;

	public LocalizedString ArmorAbsorption;

	public LocalizedString ArmorDeflection;

	public LocalizedString AreaOfEffectAbilityMiss;

	public LocalizedString DegreeOfSuccess;

	public LocalizedString ScatterShot;

	public LocalizedString ArmorPenalty;

	public LocalizedString AttackerPerception;

	public LocalizedString WeaponSkillDifference;

	public LocalizedString BurstFirePenalty;

	public LocalizedString Weapon;

	public LocalizedString AttackerWeaponSkill;

	public LocalizedString OriginAdvancement;

	public LocalizedString CareerAdvancement;

	public LocalizedString OtherAdvancement;

	public LocalizedString Immunity;

	public LocalizedString HitChanceOverkill;

	public LocalizedString DistanceToTarget;

	public LocalizedString BaseValue;

	public LocalizedString UntypedStackable;

	public LocalizedString UntypedUnstackable;

	public LocalizedString Wounds;

	public LocalizedString Trauma;

	public LocalizedString AttackerAgility;

	public LocalizedString ShipSystemComponent;

	public string GetText(ModifierDescriptor type)
	{
		return type switch
		{
			ModifierDescriptor.None => None, 
			ModifierDescriptor.Racial => Racial, 
			ModifierDescriptor.Difficulty => Difficulty, 
			ModifierDescriptor.BaseStatBonus => BaseStatBonus, 
			ModifierDescriptor.Polymorph => Polymorph, 
			ModifierDescriptor.UniqueItem => UniqueItem, 
			ModifierDescriptor.RighteousFury => RighteousFury, 
			ModifierDescriptor.ArmorAbsorption => ArmorAbsorption, 
			ModifierDescriptor.ArmorDeflection => ArmorDeflection, 
			ModifierDescriptor.AreaOfEffectAbilityMiss => AreaOfEffectAbilityMiss, 
			ModifierDescriptor.DegreeOfSuccess => DegreeOfSuccess, 
			ModifierDescriptor.ScatterShot => ScatterShot, 
			ModifierDescriptor.ArmorPenalty => ArmorPenalty, 
			ModifierDescriptor.AttackerPerception => AttackerPerception, 
			ModifierDescriptor.WeaponSkillDifference => WeaponSkillDifference, 
			ModifierDescriptor.BurstFirePenalty => BurstFirePenalty, 
			ModifierDescriptor.Weapon => Weapon, 
			ModifierDescriptor.AttackerWeaponSkill => AttackerWeaponSkill, 
			ModifierDescriptor.OriginAdvancement => OriginAdvancement, 
			ModifierDescriptor.CareerAdvancement => CareerAdvancement, 
			ModifierDescriptor.OtherAdvancement => OtherAdvancement, 
			ModifierDescriptor.Immunity => Immunity, 
			ModifierDescriptor.HitChanceOverkill => HitChanceOverkill, 
			ModifierDescriptor.DistanceToTarget => DistanceToTarget, 
			ModifierDescriptor.BaseValue => BaseValue, 
			ModifierDescriptor.UntypedStackable => UntypedStackable, 
			ModifierDescriptor.UntypedUnstackable => UntypedUnstackable, 
			ModifierDescriptor.Wounds => Wounds, 
			ModifierDescriptor.Trauma => Trauma, 
			ModifierDescriptor.AttackerAgility => AttackerAgility, 
			ModifierDescriptor.ShipSystemComponent => ShipSystemComponent, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}

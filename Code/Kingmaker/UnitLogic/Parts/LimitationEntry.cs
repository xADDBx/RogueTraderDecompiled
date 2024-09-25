using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Parts;

public class LimitationEntry
{
	public List<BlueprintAbility> ForbiddenAbilities = new List<BlueprintAbility>();

	public List<BlueprintAbilityGroup> ForbiddenAbilityGroups = new List<BlueprintAbilityGroup>();

	public List<BlueprintAbility> AbilityExceptions = new List<BlueprintAbility>();

	public WarhammerAbilityParamsSource AbilityParamsSource = WarhammerAbilityParamsSource.None;

	public UnitFact Reason;

	public int LowerCostException;

	public BaseUnitEntity Target;

	public ItemEntityWeapon Weapon;

	public bool CheapestAbilityException;

	public bool EntryNotPassed(AbilityData ability, TargetWrapper target)
	{
		return EntryNotPassed(ability, target.Entity);
	}

	public bool EntryNotPassed(AbilityData ability)
	{
		return EntryNotPassed(ability, ability.Caster);
	}

	public bool EntryNotPassed(AbilityData ability, MechanicEntity unit)
	{
		bool num = ForbiddenAbilities.Contains(ability.Blueprint);
		bool flag = ForbiddenAbilityGroups.Any((BlueprintAbilityGroup p) => ability.Blueprint.AbilityGroups.Contains(p));
		bool flag2 = AbilityParamsSource != WarhammerAbilityParamsSource.None && (ability.Blueprint.AbilityParamsSource & AbilityParamsSource) != 0;
		bool flag3 = num || flag || flag2;
		RuleCalculateAbilityActionPointCost ruleCalculateAbilityActionPointCost = Rulebook.Trigger(new RuleCalculateAbilityActionPointCost(ability.Caster, ability));
		bool flag4 = LowerCostException > 0 && ruleCalculateAbilityActionPointCost.Result > LowerCostException;
		bool flag5 = CheapestAbilityException && ability.Weapon?.Blueprint.WeaponAbilities.FirstOrDefault((WeaponAbility p) => p.Ability == ability.Blueprint)?.AP > ability.Weapon?.Blueprint.WeaponAbilities.Ability1.AP;
		bool flag6 = AbilityExceptions.Contains(ability.Blueprint);
		bool num2 = Target != null && unit != null && unit != ability.Caster && unit != Target;
		bool flag7 = ability.Weapon != null && Weapon != null && ability.Weapon != Weapon;
		if (num2 || flag3 || flag7 || flag4 || flag5)
		{
			return !flag6;
		}
		return false;
	}
}

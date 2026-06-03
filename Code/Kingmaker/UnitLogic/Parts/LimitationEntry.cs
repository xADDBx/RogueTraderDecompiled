using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Parts;

public class LimitationEntry
{
	public BlueprintAbilityGroup ForbiddenAbilityGroup;

	public BlueprintAbilityGroup AbilityGroupException;

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
		if (IsExceptionGroup(ability))
		{
			return false;
		}
		if (!ForbiddenByBlueprint(ability) && !TargetIncorrect(ability, unit) && !WeaponIncorrect(ability) && !NotCheapestAbility(ability))
		{
			return OverLimitCost(ability);
		}
		return true;
	}

	private bool IsExceptionGroup(AbilityData ability)
	{
		return IsInAbilityGroups(ability, AbilityGroupException);
	}

	private bool ForbiddenByBlueprint(AbilityData ability)
	{
		if (!IsInAbilityGroups(ability, ForbiddenAbilityGroup))
		{
			if (AbilityParamsSource != WarhammerAbilityParamsSource.None)
			{
				return (ability.Blueprint.AbilityParamsSource & AbilityParamsSource) != 0;
			}
			return false;
		}
		return true;
	}

	private static bool IsInAbilityGroups(AbilityData ability, BlueprintAbilityGroup group)
	{
		if (group != null)
		{
			return ability.Blueprint.AbilityGroups.Contains(group);
		}
		return false;
	}

	private bool TargetIncorrect(AbilityData ability, MechanicEntity unit)
	{
		if (Target != null && unit != null && unit != ability.Caster)
		{
			return unit != Target;
		}
		return false;
	}

	private bool WeaponIncorrect(AbilityData ability)
	{
		if (Weapon != null && ability.Weapon != null)
		{
			return ability.Weapon != Weapon;
		}
		return false;
	}

	private bool NotCheapestAbility(AbilityData ability)
	{
		if (CheapestAbilityException)
		{
			return ability.Weapon?.Blueprint.WeaponAbilities.FirstOrDefault((WeaponAbility p) => p.Ability == ability.Blueprint)?.AP > ability.Weapon?.Blueprint.WeaponAbilities.Ability1.AP;
		}
		return false;
	}

	private bool OverLimitCost(AbilityData ability)
	{
		if (LowerCostException <= 0)
		{
			return false;
		}
		return ability.CalculateActionPointCost() > LowerCostException;
	}
}

using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartForbiddenAbilities : BaseUnitPart, IHashable
{
	public List<LimitationEntry> Limitations = new List<LimitationEntry>();

	public void AddEntry(List<BlueprintAbility> forbiddenAbilities, List<BlueprintAbilityGroup> forbiddenAbilityGroups, List<BlueprintAbility> abilityExceptions, int lowerCostException, UnitFact reason, BaseUnitEntity target = null)
	{
		LimitationEntry limitationEntry = new LimitationEntry();
		foreach (BlueprintAbility forbiddenAbility in forbiddenAbilities)
		{
			limitationEntry.ForbiddenAbilities.Add(forbiddenAbility);
		}
		foreach (BlueprintAbilityGroup forbiddenAbilityGroup in forbiddenAbilityGroups)
		{
			limitationEntry.ForbiddenAbilityGroups.Add(forbiddenAbilityGroup);
		}
		foreach (BlueprintAbility abilityException in abilityExceptions)
		{
			limitationEntry.AbilityExceptions.Add(abilityException);
		}
		limitationEntry.Target = target;
		limitationEntry.LowerCostException = lowerCostException;
		limitationEntry.Reason = reason;
		Limitations.Add(limitationEntry);
	}

	public void AddEntry(BlueprintAbilityGroup forbiddenAbilityGroup, int lowerCostException, UnitFact reason, bool cheapestAbilityException, ItemEntityWeapon weapon = null)
	{
		LimitationEntry limitationEntry = new LimitationEntry();
		limitationEntry.ForbiddenAbilityGroups.Add(forbiddenAbilityGroup);
		limitationEntry.Reason = reason;
		limitationEntry.LowerCostException = lowerCostException;
		limitationEntry.Weapon = weapon;
		limitationEntry.CheapestAbilityException = cheapestAbilityException;
		Limitations.Add(limitationEntry);
	}

	public void AddEntry(BlueprintAbilityGroup forbiddenAbilityGroup, UnitFact reason, ItemEntityWeapon weapon = null)
	{
		LimitationEntry limitationEntry = new LimitationEntry();
		limitationEntry.ForbiddenAbilityGroups.Add(forbiddenAbilityGroup);
		limitationEntry.Reason = reason;
		limitationEntry.Weapon = weapon;
		Limitations.Add(limitationEntry);
	}

	public void AddEntry(ItemEntityWeapon weapon, UnitFact reason)
	{
		LimitationEntry limitationEntry = new LimitationEntry();
		limitationEntry.Reason = reason;
		limitationEntry.Weapon = weapon;
		Limitations.Add(limitationEntry);
	}

	public void AddEntry(BaseUnitEntity target, UnitFact reason)
	{
		LimitationEntry limitationEntry = new LimitationEntry();
		limitationEntry.Target = target;
		limitationEntry.Reason = reason;
		Limitations.Add(limitationEntry);
	}

	public void AddEntry(WarhammerAbilityParamsSource abilityParamsSource, UnitFact reason)
	{
		LimitationEntry limitationEntry = new LimitationEntry();
		limitationEntry.AbilityParamsSource = abilityParamsSource;
		limitationEntry.Reason = reason;
		Limitations.Add(limitationEntry);
	}

	public void RemoveEntry(UnitFact reason)
	{
		Limitations.RemoveAll((LimitationEntry p) => p.Reason == reason);
	}

	public bool AbilityAllowed(AbilityData ability, TargetWrapper target)
	{
		return !Limitations.Any((LimitationEntry p1) => p1.EntryNotPassed(ability, target));
	}

	public bool AbilityAllowed(AbilityData ability)
	{
		return !Limitations.Any((LimitationEntry p1) => p1.EntryNotPassed(ability));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

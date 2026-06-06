using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartForbiddenAbilities : BaseUnitPart, IHashable
{
	public List<LimitationEntry> Limitations = new List<LimitationEntry>();

	public void AddEntry(BlueprintAbilityGroup forbiddenAbilityGroup, int lowerCostException, UnitFact reason, bool cheapestAbilityException, ItemEntityWeapon weapon = null)
	{
		LimitationEntry limitationEntry = new LimitationEntry();
		limitationEntry.ForbiddenAbilityGroup = forbiddenAbilityGroup;
		limitationEntry.Reason = reason;
		limitationEntry.LowerCostException = lowerCostException;
		limitationEntry.Weapon = weapon;
		limitationEntry.CheapestAbilityException = cheapestAbilityException;
		Limitations.Add(limitationEntry);
	}

	public void AddEntry(BlueprintAbilityGroup forbiddenAbilityGroup, BlueprintAbilityGroup exceptionAbilityGroup, UnitFact reason, ItemEntityWeapon weapon = null)
	{
		LimitationEntry limitationEntry = new LimitationEntry();
		limitationEntry.ForbiddenAbilityGroup = forbiddenAbilityGroup;
		limitationEntry.AbilityGroupException = exceptionAbilityGroup;
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
		foreach (LimitationEntry limitation in Limitations)
		{
			if (limitation.EntryNotPassed(ability, target))
			{
				return false;
			}
		}
		return true;
	}

	public bool AbilityAllowed(AbilityData ability)
	{
		foreach (LimitationEntry limitation in Limitations)
		{
			if (limitation.EntryNotPassed(ability))
			{
				return false;
			}
		}
		return true;
	}

	public IEnumerable<UnitFact> GetAllFactsForbiddingAbility(AbilityData ability)
	{
		return (from p in Limitations
			where p.EntryNotPassed(ability)
			select p.Reason).Distinct();
	}

	public IEnumerable<UnitFact> GetGroupLimitationBuffsForbidding(AbilityData ability)
	{
		return (from p in Limitations
			where p.EntryNotPassed(ability) && p.Reason?.Blueprint.GetComponent<AbilityGroupLimitation>() != null
			select p.Reason).Distinct();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

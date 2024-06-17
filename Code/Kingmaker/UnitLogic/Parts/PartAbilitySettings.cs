using System.Collections.Generic;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartAbilitySettings : UnitPart, IHashable
{
	private readonly List<(EntityFactComponent Runtime, OverrideAbilityThreatenedAreaSetting Component)> m_ThreatenedAreaEntries = new List<(EntityFactComponent, OverrideAbilityThreatenedAreaSetting)>();

	public static BlueprintAbility.UsingInThreateningAreaType GetThreatenedAreaSetting(AbilityData ability)
	{
		PartAbilitySettings abilitySettingsOptional = ability.Caster.GetAbilitySettingsOptional();
		if (abilitySettingsOptional == null)
		{
			return ability.Blueprint.UsingInThreateningArea;
		}
		foreach (var threatenedAreaEntry in abilitySettingsOptional.m_ThreatenedAreaEntries)
		{
			using (threatenedAreaEntry.Runtime.RequestEventContext())
			{
				BlueprintAbility.UsingInThreateningAreaType? threatenedAreaRule = threatenedAreaEntry.Component.GetThreatenedAreaRule(ability);
				if (threatenedAreaRule.HasValue)
				{
					return threatenedAreaRule.GetValueOrDefault();
				}
			}
		}
		return ability.Blueprint.UsingInThreateningArea;
	}

	public void Add(OverrideAbilityThreatenedAreaSetting component)
	{
		m_ThreatenedAreaEntries.Add((component.Runtime, component));
	}

	public void Remove(OverrideAbilityThreatenedAreaSetting component)
	{
		m_ThreatenedAreaEntries.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_ThreatenedAreaEntries.Empty())
		{
			RemoveSelf();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

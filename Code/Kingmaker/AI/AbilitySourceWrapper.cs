using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using UnityEngine;

namespace Kingmaker.AI;

[Serializable]
public class AbilitySourceWrapper
{
	public enum AbilitySourceType
	{
		Ability,
		Equipment,
		RandomGroup
	}

	[HideInInspector]
	public AbilitySourceType Type;

	[SerializeField]
	[HideInInspector]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	[HideInInspector]
	private BlueprintItemEquipmentReference m_Equipment;

	[SerializeField]
	[HideInInspector]
	private List<BlueprintAbilityReference> m_RandomGroup;

	private List<BlueprintAbility> m_Abilities;

	public BlueprintAbility Ability => m_Ability?.Get();

	public BlueprintItemEquipment Equipment => m_Equipment?.Get();

	public List<BlueprintAbility> Abilities
	{
		get
		{
			if (m_Abilities != null)
			{
				return m_Abilities;
			}
			if (Type == AbilitySourceType.Ability && Ability != null)
			{
				m_Abilities = new List<BlueprintAbility> { Ability };
			}
			if (Type == AbilitySourceType.Equipment && Equipment != null)
			{
				m_Abilities = Equipment.Abilities.ToList();
			}
			if (Type == AbilitySourceType.RandomGroup)
			{
				m_Abilities = m_RandomGroup.Select((BlueprintAbilityReference a) => a.Get()).ToList();
			}
			return m_Abilities;
		}
	}

	private bool IsFirstAbilityBetter(BlueprintAbility first, BlueprintAbility second)
	{
		if ((first.GetComponent<WarhammerEndTurn>() != null || second.GetComponent<WarhammerEndTurn>() == null) && (first.PatternSettings == null || second.PatternSettings != null))
		{
			if (first.Range != 0)
			{
				return second.Range == AbilityRange.Personal;
			}
			return false;
		}
		return true;
	}

	public IEnumerable<BlueprintAbility> GetSorted(BaseUnitEntity unit)
	{
		List<BlueprintAbility> abilities = Abilities;
		if (Type == AbilitySourceType.Equipment)
		{
			abilities.Sort(delegate(BlueprintAbility a, BlueprintAbility b)
			{
				if (IsFirstAbilityBetter(a, b))
				{
					return -1;
				}
				return IsFirstAbilityBetter(b, a) ? 1 : (CalculateActionPointCost(unit, b) - CalculateActionPointCost(unit, a));
			});
		}
		if (Type == AbilitySourceType.RandomGroup)
		{
			for (int i = 0; i < abilities.Count - 1; i++)
			{
				int index = unit.Random.Range(i, abilities.Count);
				BlueprintAbility value = abilities[i];
				abilities[i] = abilities[index];
				abilities[index] = value;
			}
		}
		return abilities;
	}

	private int CalculateActionPointCost(BaseUnitEntity unit, BlueprintAbility blueprint)
	{
		return unit.Abilities.RawFacts.Find((Ability ab) => ab.Blueprint == blueprint)?.Data.CalculateActionPointCost() ?? blueprint.ActionPointCost;
	}

	public override string ToString()
	{
		if (Type != 0)
		{
			if (Type != AbilitySourceType.Equipment)
			{
				return "Random Group";
			}
			return "Equipment [" + (Equipment?.ToString() ?? "<null>") + "]";
		}
		return "Ability [" + (Ability?.ToString() ?? "<null>") + "]";
	}
}

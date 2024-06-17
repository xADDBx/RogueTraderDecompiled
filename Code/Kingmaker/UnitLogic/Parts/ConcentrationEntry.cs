using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class ConcentrationEntry : IHashable
{
	public int ActionPointCost;

	public int MovementPointCost;

	public BlueprintAbility Ability;

	public BaseUnitEntity Target;

	public List<Buff> Buffs = new List<Buff>();

	public int Index;

	[JsonConstructor]
	private ConcentrationEntry()
	{
	}

	public ConcentrationEntry(BlueprintAbility ability, BaseUnitEntity target)
	{
		Ability = ability;
		Target = target;
		WarhammerConcentrationAbility component = ability.GetComponent<WarhammerConcentrationAbility>();
		if (component != null)
		{
			ActionPointCost = component.ActionPointCost;
			MovementPointCost = component.MovementPointCost;
		}
	}

	public void RemoveEntry()
	{
		Buff[] array = Buffs.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Remove();
		}
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}

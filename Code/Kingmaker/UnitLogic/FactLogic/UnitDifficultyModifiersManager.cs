using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("2e982373853d4e26b7e61354b88923e0")]
public abstract class UnitDifficultyModifiersManager : UnitFactComponentDelegate, IDifficultyChangedClassHandler, ISubscriber, IUnitChangeAttackFactionsHandler, ISubscriber<IBaseUnitEntity>, IHashable
{
	private class TransientData : IEntityFactComponentTransientData
	{
		public readonly List<ModifiableValue.Modifier> Modifiers = new List<ModifiableValue.Modifier>();
	}

	protected override void OnActivateOrPostLoad()
	{
		UpdateModifiers();
	}

	protected override void OnDeactivate()
	{
		RemoveModifiers();
	}

	public void HandleDifficultyChanged()
	{
		UpdateModifiers();
	}

	protected virtual void UpdateModifiers()
	{
	}

	protected void RemoveModifiers()
	{
		TransientData transientData = RequestTransientData<TransientData>();
		transientData.Modifiers.ForEach(delegate(ModifiableValue.Modifier i)
		{
			i.Remove();
		});
		transientData.Modifiers.Clear();
	}

	protected void AddPercentModifier(StatType statType, int percentModifier)
	{
		ModifiableValue statOptional = base.Owner.GetStatOptional(statType);
		if (statOptional != null)
		{
			int value = Mathf.FloorToInt((float)statOptional.BaseValue * ((float)percentModifier / 100f));
			ModifiableValue.Modifier modifier = statOptional.AddModifier(value, base.Runtime, ModifierDescriptor.Difficulty);
			if (modifier != null)
			{
				RequestTransientData<TransientData>().Modifiers.Add(modifier);
			}
		}
	}

	protected void AddModifier(StatType statType, int flatModifier)
	{
		ModifiableValue statOptional = base.Owner.GetStatOptional(statType);
		if (statOptional != null)
		{
			ModifiableValue.Modifier item = statOptional.AddModifier(flatModifier, base.Runtime, ModifierDescriptor.Difficulty);
			RequestTransientData<TransientData>().Modifiers.Add(item);
		}
	}

	public void HandleUnitChangeAttackFactions(MechanicEntity unit)
	{
		if (unit == base.Owner)
		{
			PartStarshipNavigation starshipNavigationOptional = unit.GetStarshipNavigationOptional();
			if (starshipNavigationOptional == null || !starshipNavigationOptional.IsSoftUnit)
			{
				UpdateModifiers();
			}
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

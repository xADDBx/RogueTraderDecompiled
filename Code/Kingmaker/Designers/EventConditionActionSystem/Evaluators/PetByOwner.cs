using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("9e61684a16394f879f8b2de216fcb960")]
public class PetByOwner : AbstractUnitEvaluator
{
	[SerializeReference]
	public AbstractUnitEvaluator OwnerEvaluator;

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		AbstractUnitEntity value = OwnerEvaluator.GetValue();
		if (value.Facts.HasComponent<PetOwner>())
		{
			return value.GetOrCreate<UnitPartPetOwner>().PetUnit;
		}
		return null;
	}

	public override string GetCaption()
	{
		return "Get pet of ";
	}
}

using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("63f3fa5b72a34ef79f4222eac2ed976b")]
public class OwnerByPet : AbstractUnitEvaluator
{
	[SerializeReference]
	public AbstractUnitEvaluator PetEvaluator;

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return (PetEvaluator.GetValue() as BaseUnitEntity)?.Master;
	}

	public override string GetCaption()
	{
		return "Get owner of ";
	}
}

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Spawners;
using Kingmaker.View.Spawners.Components;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("90fe1cd67265f0249a38caef02130066")]
public class CorpseInteractionEvaluator : MapObjectEvaluator
{
	[AllowedEntityType(typeof(UnitSpawner))]
	[ValidateNotEmpty]
	[SerializeField]
	private EntityReference m_UnitSpawner;

	protected override MapObjectEntity GetMapObjectInternal()
	{
		return (((m_UnitSpawner.FindData() as UnitSpawnerBase.MyData)?.SpawnedUnit)?.Entity.ToAbstractUnitEntity().Parts.GetOptional<SpawnerCorpseInteraction.CorpseInteractionPart>())?.InteractionObjectRef.Entity;
	}

	public override string GetCaption()
	{
		return "Corpse Interaction from" + m_UnitSpawner;
	}
}

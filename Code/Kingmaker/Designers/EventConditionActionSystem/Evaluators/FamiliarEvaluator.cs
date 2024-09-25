using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("69a4e9c15933491a9c28ef40e0800630")]
public class FamiliarEvaluator : AbstractFamiliarEvaluator
{
	[NotNull]
	[SerializeReference]
	[AllowedEntityType(typeof(BaseUnitEntity))]
	public AbstractUnitEvaluator LeaderEvaluator;

	protected override BaseUnitEntity Leader => LeaderEvaluator.GetValue() as BaseUnitEntity;

	public override string GetCaption()
	{
		return "Familiar of " + LeaderEvaluator?.GetCaption();
	}
}

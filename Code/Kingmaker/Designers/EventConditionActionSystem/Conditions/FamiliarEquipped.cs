using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("a2904b6d847f462a912b2e9bb9978ca5")]
public class FamiliarEquipped : AbstractFamiliarEquipped
{
	[NotNull]
	[SerializeReference]
	[AllowedEntityType(typeof(BaseUnitEntity))]
	public AbstractUnitEvaluator LeaderEvaluator;

	protected override BaseUnitEntity Leader => LeaderEvaluator.GetValue() as BaseUnitEntity;

	protected override string GetConditionCaption()
	{
		if (base.Unit != null)
		{
			return $"{LeaderEvaluator?.GetCaption()} has equipped {base.Unit} familiar";
		}
		return LeaderEvaluator?.GetCaption() + " has no equipped familiar";
	}
}

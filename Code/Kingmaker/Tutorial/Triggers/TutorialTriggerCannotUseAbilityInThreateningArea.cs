using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("90fb04ed00a24c1ebf59ba65bd4b054f")]
public class TutorialTriggerCannotUseAbilityInThreateningArea : TutorialTrigger, IAbilityCannotUseInThreateningArea, ISubscriber, IHashable
{
	public void HandleCannotUseAbilityInThreateningArea(AbilityData ability)
	{
		TryToTrigger(null, delegate(TutorialContext context)
		{
			context.SourceAbility = ability;
			context.SourceUnit = ability.Caster as BaseUnitEntity;
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

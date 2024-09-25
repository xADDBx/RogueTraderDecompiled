using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("54a86f92b75a4fda91624eecaebe6ad2")]
public class AbilityApplyEffectTriggerTarget : AbilityTrigger, IApplyAbilityEffectHandler, ISubscriber, IHashable
{
	[SerializeField]
	private bool AssignCasterAsTarget;

	[SerializeField]
	private bool m_AssignContextFromAbility;

	public void OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		if (!(target.Target != base.Owner) && Restrictions.IsPassed(base.Fact, context, null, context.Ability, target.Target.Entity))
		{
			RunAction(context.Ability.Blueprint, context, target.Target, AssignCasterAsTarget, m_AssignContextFromAbility);
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

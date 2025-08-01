using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[TypeId("9cb8cadd214341dbbc558d4097cdf57c")]
public class AbilityRuleTriggerInitiator : AbilityTrigger, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public bool AssignOwnerAsTarget;

	public bool AssignContextFromAbility;

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Spell))
		{
			if (AssignContextFromAbility)
			{
				RunAction(evt.Context.Ability.Blueprint, evt.Context, AssignOwnerAsTarget ? ((TargetWrapper)base.Owner) : evt.SpellTarget, AssignOwnerAsTarget, assignContextFromAbility: true);
			}
			else
			{
				RunAction(evt.Spell.Blueprint, evt.ConcreteInitiator, evt.SpellTarget, AssignOwnerAsTarget, this);
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

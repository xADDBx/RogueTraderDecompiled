using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("1390271155944ca8838cab659c6d52b9")]
public class AbilityRuleTriggerTarget : AbilityTrigger, ITargetRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public bool AssignCasterAsTarget;

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Spell))
		{
			RunAction(evt.Spell.Blueprint, evt.ConcreteInitiator, evt.SpellTarget, AssignCasterAsTarget, this);
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

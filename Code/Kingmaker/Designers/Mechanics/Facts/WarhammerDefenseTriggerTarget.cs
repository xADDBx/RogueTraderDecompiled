using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Block;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("fca6137076d25cf4ebcb31fb2c6efe6a")]
public class WarhammerDefenseTriggerTarget : WarhammerDefenseTriggerBase, IInitiatorRulebookHandler<RulePerformDodge>, IRulebookHandler<RulePerformDodge>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleRollParry>, IRulebookHandler<RuleRollParry>, IInitiatorRulebookHandler<RuleRollBlock>, IRulebookHandler<RuleRollBlock>, ITargetRulebookHandler<RuleRollCoverHit>, IRulebookHandler<RuleRollCoverHit>, ITargetRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RulePerformDodge evt)
	{
	}

	public void OnEventDidTrigger(RulePerformDodge evt)
	{
		TryTriggerDodgeActions(evt);
	}

	public void OnEventAboutToTrigger(RuleRollParry evt)
	{
	}

	public void OnEventDidTrigger(RuleRollParry evt)
	{
		TryTriggerParryActions(evt);
	}

	public void OnEventAboutToTrigger(RuleRollCoverHit evt)
	{
	}

	public void OnEventDidTrigger(RuleRollCoverHit evt)
	{
		TryTriggerCoverActions(evt);
	}

	public void OnEventAboutToTrigger(RuleRollBlock evt)
	{
	}

	public void OnEventDidTrigger(RuleRollBlock evt)
	{
		TryTriggerBlockActions(evt);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

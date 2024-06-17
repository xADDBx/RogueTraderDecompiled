using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("2d1677e33023ef347be1b51d3daceaf0")]
public class WarhammerDefenseTriggerInitiator : WarhammerDefenseTriggerBase, ITargetRulebookHandler<RulePerformDodge>, IRulebookHandler<RulePerformDodge>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleRollParry>, IRulebookHandler<RuleRollParry>, ITargetRulebookHandler<RuleRollCoverHit>, IRulebookHandler<RuleRollCoverHit>, IHashable
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

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

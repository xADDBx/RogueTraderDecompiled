using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.HitChance;

[Serializable]
[TypeId("428580af48b84509b97be533f03c7759")]
public class WarhammerHitChanceModifierInitiator : WarhammerHitChanceModifier, IInitiatorRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateCoverHitChance>, IRulebookHandler<RuleCalculateCoverHitChance>, IInitiatorRulebookHandler<RuleCalculateScatterShotHitDirectionProbability>, IRulebookHandler<RuleCalculateScatterShotHitDirectionProbability>, IInitiatorRulebookHandler<RuleCalculateRighteousFuryChance>, IRulebookHandler<RuleCalculateRighteousFuryChance>, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateCoverHitChance evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateCoverHitChance evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateRighteousFuryChance evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateRighteousFuryChance evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateScatterShotHitDirectionProbability evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateScatterShotHitDirectionProbability evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

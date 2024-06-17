using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.HitChance;

[Serializable]
[TypeId("4633613525bb42aeb9996511f056ce0b")]
public class WarhammerHitChanceModifierTarget : WarhammerHitChanceModifier, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleCalculateCoverHitChance>, IRulebookHandler<RuleCalculateCoverHitChance>, ITargetRulebookHandler<RuleCalculateRighteousFuryChance>, IRulebookHandler<RuleCalculateRighteousFuryChance>, IHashable
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

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

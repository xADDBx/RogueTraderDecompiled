using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("d89d9a7a51e048c680a95ebd444879b7")]
public class TargetHealModifier : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateHeal>, IRulebookHandler<RuleCalculateHeal>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public ContextValue FlatBonus;

	public ContextValue PercentBonus;

	public bool OnlyAgainstTargetsWithHalfOrLessHealth;

	public void OnEventAboutToTrigger(RuleCalculateHeal evt)
	{
		if (evt.TargetHealth != null && (!OnlyAgainstTargetsWithHalfOrLessHealth || evt.TargetHealth.HitPointsLeft <= evt.TargetHealth.MaxHitPoints / 2))
		{
			evt.FlatBonus += FlatBonus.Calculate(base.Context);
			evt.PercentBonus += PercentBonus.Calculate(base.Context);
		}
	}

	public void OnEventDidTrigger(RuleCalculateHeal evt)
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

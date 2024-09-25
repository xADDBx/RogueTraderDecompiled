using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("a6fb48de4771436fa5d6db25ba686fd1")]
public class InitiatorHealModifier : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateHeal>, IRulebookHandler<RuleCalculateHeal>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public int FlatBonus;

	public int PercentBonus;

	public bool OnlyAgainstTargetsWithHalfOrLessHealth;

	public void OnEventAboutToTrigger(RuleCalculateHeal evt)
	{
		if (evt.TargetHealth != null && (!OnlyAgainstTargetsWithHalfOrLessHealth || evt.TargetHealth.HitPointsLeft <= evt.TargetHealth.MaxHitPoints / 2))
		{
			evt.FlatBonus += FlatBonus;
			evt.PercentBonus += PercentBonus;
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

using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UnitLogic.FactLogic;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("a5cbfd1546727ec418590630a6ea2400")]
public class WarhammerDamageTriggerInitiator : WarhammerDamageTrigger, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleRollDamage>, IRulebookHandler<RuleRollDamage>, IHashable
{
	public void OnEventAboutToTrigger(RuleDealDamage rule)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage rule)
	{
		if (!TriggerBeforeDamageHappens)
		{
			TryTrigger(rule);
		}
	}

	public void OnEventAboutToTrigger(RuleRollDamage rule)
	{
	}

	public void OnEventDidTrigger(RuleRollDamage rule)
	{
		if (TriggerBeforeDamageHappens)
		{
			TryTrigger(rule);
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

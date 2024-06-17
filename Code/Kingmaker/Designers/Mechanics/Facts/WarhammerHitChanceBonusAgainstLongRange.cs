using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("1ab746cecbdf4360a26a0f60fcf45409")]
public class WarhammerHitChanceBonusAgainstLongRange : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public int HitChanceModifier;

	public int MinimalRange;

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		if (evt.ConcreteInitiator.DistanceToInCells(evt.ConcreteTarget) >= MinimalRange)
		{
			evt.HitChanceValueModifiers.Add(-HitChanceModifier, base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
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

using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("67fb56d109f58d4458c9f30b5f07c0b2")]
public class AllowRicochetWithoutHittingInitiator : AllowRicochetWithoutHitting, IInitiatorRulebookHandler<RuleCalculateOverpenetration>, IRulebookHandler<RuleCalculateOverpenetration>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateOverpenetration rule)
	{
		ApplyToEvent(rule);
	}

	public void OnEventDidTrigger(RuleCalculateOverpenetration rule)
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

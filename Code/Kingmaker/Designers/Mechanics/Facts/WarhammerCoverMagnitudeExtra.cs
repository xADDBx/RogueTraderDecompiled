using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
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
[TypeId("a890259ae728aa2478c1c8b387b7a1ae")]
public class WarhammerCoverMagnitudeExtra : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateCoverHitChance>, IRulebookHandler<RuleCalculateCoverHitChance>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public float CoverMagnitudeExtra;

	public void OnEventAboutToTrigger(RuleCalculateCoverHitChance evt)
	{
		evt.ChanceValueModifiers.Add(Mathf.RoundToInt(CoverMagnitudeExtra * 100f), base.Fact);
	}

	public void OnEventDidTrigger(RuleCalculateCoverHitChance evt)
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

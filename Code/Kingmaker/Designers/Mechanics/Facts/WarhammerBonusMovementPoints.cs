using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("6d4d8e393e06464abf749d2b80d67adc")]
public class WarhammerBonusMovementPoints : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateMovementPoints>, IRulebookHandler<RuleCalculateMovementPoints>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public int Bonus;

	public float Modifier = 1f;

	public bool SetUpperLimit;

	[ShowIf("SetUpperLimit")]
	public int UpperLimitValue;

	public ContextValue Value;

	public void OnEventAboutToTrigger(RuleCalculateMovementPoints evt)
	{
		int bonus = Bonus;
		bonus += Value.Calculate(base.Context);
		float num = Modifier;
		if ((bool)base.Owner.Features.ImmuneToMovementPointReduction)
		{
			bonus = Math.Max(0, bonus);
			num = Math.Max(1f, num);
		}
		evt.Bonus += bonus;
		evt.Modifier *= num;
		if (SetUpperLimit && !base.Owner.Features.ImmuneToMovementPointReduction)
		{
			evt.SetUpperLimit = true;
			evt.UppLimitValue = Math.Max(evt.UppLimitValue, UpperLimitValue);
		}
	}

	public void OnEventDidTrigger(RuleCalculateMovementPoints evt)
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

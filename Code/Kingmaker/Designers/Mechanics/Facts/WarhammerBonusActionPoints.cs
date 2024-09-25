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
[TypeId("54ca5dc35f1c41189b3bf1833ae2b98f")]
public class WarhammerBonusActionPoints : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateActionPoints>, IRulebookHandler<RuleCalculateActionPoints>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public int MaxPointsBonus;

	public int RegenBonus;

	public bool SetUpperLimit;

	[ShowIf("SetUpperLimit")]
	public int UpperLimitValue = 1000;

	public ContextValue MaxPointsValue = new ContextValue();

	public ContextValue RegenValue = new ContextValue();

	public void OnEventAboutToTrigger(RuleCalculateActionPoints evt)
	{
		evt.MaxPointsBonus += MaxPointsBonus + MaxPointsValue.Calculate(base.Context);
		evt.RegenBonus += RegenBonus + RegenValue.Calculate(base.Context);
		if (SetUpperLimit)
		{
			evt.SetUpperLimit = true;
			evt.UppLimitValue = Math.Min(evt.UppLimitValue, UpperLimitValue);
		}
	}

	public void OnEventDidTrigger(RuleCalculateActionPoints evt)
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

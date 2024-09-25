using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("4628eabc04834caabc2fcd52e1bc9f3b")]
public class WarhammerDodgeAndParryBonus : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDodgeChance>, IRulebookHandler<RuleCalculateDodgeChance>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateParryChance>, IRulebookHandler<RuleCalculateParryChance>, IHashable
{
	public ContextValue DodgeBonus;

	public ContextValue ParryBonus;

	public void OnEventAboutToTrigger(RuleCalculateDodgeChance evt)
	{
		evt.DodgeValueModifiers.Add(DodgeBonus.Calculate(base.Context), base.Fact);
	}

	public void OnEventDidTrigger(RuleCalculateDodgeChance evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateParryChance evt)
	{
		evt.ParryValueModifiers.Add(DodgeBonus.Calculate(base.Context), base.Fact);
	}

	public void OnEventDidTrigger(RuleCalculateParryChance evt)
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

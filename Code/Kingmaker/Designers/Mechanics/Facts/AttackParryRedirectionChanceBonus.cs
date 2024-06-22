using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b289902b439148078739644ba99c207d")]
public class AttackParryRedirectionChanceBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateParryChance>, IRulebookHandler<RuleCalculateParryChance>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValue RedirectionChanceBonus;

	public void OnEventAboutToTrigger(RuleCalculateParryChance evt)
	{
		evt.DefenderAttackRedirectionChanceModifiers.Add(RedirectionChanceBonus.Calculate(base.Context), base.Fact);
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

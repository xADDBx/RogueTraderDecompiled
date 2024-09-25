using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[ComponentName("Buffs/AddEffect/FastHealing")]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("1801bcdfc4c7d9e41a5d80adcd2055a0")]
public class AddEffectContextFastHealing : UnitBuffComponentDelegate, ITickEachRound, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public ContextValue Bonus;

	void ITickEachRound.OnNewRound()
	{
		if (!base.Owner.LifeState.IsDead && base.Owner.Health.Damage > 0)
		{
			int amount = Bonus.Calculate(base.Context);
			GameHelper.HealDamage(base.Owner, base.Owner, amount);
		}
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
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

using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("a5d93269cb2a4401a32be0c7824b77b5")]
public class DestroyCoversInstantly : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleRollDamage>, IRulebookHandler<RuleRollDamage>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, IHashable
{
	public void OnEventAboutToTrigger(RuleRollDamage evt)
	{
		if (evt.Target is DestructibleEntity destructibleEntity)
		{
			evt.Damage.Modifiers.Add(ModifierType.ValAdd, destructibleEntity.Health.MaxHitPoints, base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleRollDamage evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (evt.MaybeTarget is DestructibleEntity destructibleEntity)
		{
			evt.ValueModifiers.Add(ModifierType.ValAdd, destructibleEntity.Health.MaxHitPoints, base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
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

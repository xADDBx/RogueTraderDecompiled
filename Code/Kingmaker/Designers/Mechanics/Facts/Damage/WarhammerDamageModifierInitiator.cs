using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[AllowMultipleComponents]
[TypeId("cf4a4d014c2610548a3f9213c3de882e")]
public class WarhammerDamageModifierInitiator : WarhammerDamageModifier, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		TryApply(evt);
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

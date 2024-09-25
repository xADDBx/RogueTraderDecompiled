using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[TypeId("0c2563db2e994d42a0c9b78011ab0820")]
public class WarhammerCriticalDamageModifierTarget : WarhammerCriticalDamageModifier, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
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

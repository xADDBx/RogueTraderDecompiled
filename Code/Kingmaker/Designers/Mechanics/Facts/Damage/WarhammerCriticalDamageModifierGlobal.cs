using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[TypeId("65f98b372438436fb56c271386675f39")]
public class WarhammerCriticalDamageModifierGlobal : WarhammerCriticalDamageModifier, IGlobalRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public bool OnlyAgainstAllies;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		MechanicEntity mechanicEntity = (MechanicEntity)evt.Initiator;
		MechanicEntity maybeTarget = evt.MaybeTarget;
		if (!OnlyAgainstAllies || (maybeTarget != null && mechanicEntity != null && mechanicEntity.IsAlly(maybeTarget) && maybeTarget.IsAlly(base.Owner)))
		{
			TryApply(evt);
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

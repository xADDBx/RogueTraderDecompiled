using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.ParryChance;

[TypeId("4b271a6d810c85c46a2750d3a2b50de9")]
public class WarhammerParryChanceModifierAttacker : WarhammerParryChanceModifier, ITargetRulebookHandler<RuleCalculateParryChance>, IRulebookHandler<RuleCalculateParryChance>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateParryChance evt)
	{
		TryApply(evt);
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

using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.ParryChance;

[TypeId("0bde26e1c00620f4b9a960c1110b7b30")]
public class WarhammerParryChanceModifierDefender : WarhammerParryChanceModifier, IInitiatorRulebookHandler<RuleCalculateParryChance>, IRulebookHandler<RuleCalculateParryChance>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
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

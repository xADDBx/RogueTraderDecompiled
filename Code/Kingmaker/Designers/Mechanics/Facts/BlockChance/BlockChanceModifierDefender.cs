using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Block;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.BlockChance;

[TypeId("d3092219e0964c2f96138a84033fc57b")]
public class BlockChanceModifierDefender : BlockChanceModifier, IInitiatorRulebookHandler<RuleCalculateBlockChance>, IRulebookHandler<RuleCalculateBlockChance>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateBlockChance evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateBlockChance evt)
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

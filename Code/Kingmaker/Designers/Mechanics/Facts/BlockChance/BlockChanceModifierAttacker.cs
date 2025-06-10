using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Block;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.BlockChance;

[TypeId("607a58399d9a479eb9e824d3285866fa")]
public class BlockChanceModifierAttacker : BlockChanceModifier, ITargetRulebookHandler<RuleCalculateBlockChance>, IRulebookHandler<RuleCalculateBlockChance>, ISubscriber, ITargetRulebookSubscriber, IHashable
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

using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.DodgeChance;

[TypeId("0b4684d3bfe5495bb8e279792b7f961b")]
public class WarhammerDodgeChanceStatReplacement : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDodgeChance>, IRulebookHandler<RuleCalculateDodgeChance>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public StatType AgilityReplacementStat = StatType.WarhammerAgility;

	public void OnEventAboutToTrigger(RuleCalculateDodgeChance evt)
	{
		evt.AgilityReplacementStats.Add(AgilityReplacementStat);
	}

	public void OnEventDidTrigger(RuleCalculateDodgeChance evt)
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

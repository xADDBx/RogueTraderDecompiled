using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.RuleSystem.Rules.Starships;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("f0aa776050074a97bf985dbaf7896d36")]
public class TutorialTriggerPlayerStarshipPerformAttack : TutorialTriggerRulebookEvent<RuleStarshipPerformAttack>, IHashable
{
	protected override bool ShouldTrigger(RuleStarshipPerformAttack rule)
	{
		return rule.Initiator.Faction.IsPlayer;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

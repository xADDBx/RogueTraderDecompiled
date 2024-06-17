using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("3854932d386a433886d803f4a23828b7")]
public class TutorialTriggerScatterAttack : TutorialTriggerRulebookEvent<RulePerformAttack>, IHashable
{
	protected override bool ShouldTrigger(RulePerformAttack rule)
	{
		if (rule.Initiator.IsPlayerFaction)
		{
			return rule.Ability.IsScatter;
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("43d83d5013c44c4ca7ad55885eae16d1")]
public class TutorialTriggerOverpenetration : TutorialTriggerRulebookEvent<RulePerformAttack>, IHashable
{
	protected override bool ShouldTrigger(RulePerformAttack rule)
	{
		if (rule.ResultDamageRule != null && rule.Target.IsPlayerFaction)
		{
			return rule.IsOverpenetration;
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

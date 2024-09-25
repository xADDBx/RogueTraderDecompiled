using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("32e3059dbb8b4aefb4861400f90f8ba8")]
public class TutorialTriggerRighteousFury : TutorialTriggerRulebookEvent<RulePerformAttack>, IHashable
{
	protected override bool ShouldTrigger(RulePerformAttack rule)
	{
		if (rule.ResultIsHit)
		{
			return rule.RollPerformAttackRule.ResultIsRighteousFury;
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

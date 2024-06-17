using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("4fb23e9b0df24b5e9465cbb60e0f8ca7")]
public class TutorialTriggerPsychicPhenomena : TutorialTriggerRulebookEvent<RuleCalculatePsychicPhenomenaEffect>, IHashable
{
	protected override bool ShouldTrigger(RuleCalculatePsychicPhenomenaEffect rule)
	{
		return rule.ResultPsychicPhenomena != null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

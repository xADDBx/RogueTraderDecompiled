using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("fe119a0d81774716ae30a24bcb77fdb5")]
public class TutorialTriggerForceMoveCollision : TutorialTriggerRulebookEvent<RulePerformCollision>, IHashable
{
	protected override bool ShouldTrigger(RulePerformCollision rule)
	{
		if (!rule.Pushed.IsInPlayerParty)
		{
			return rule.Pusher.IsInPlayerParty;
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

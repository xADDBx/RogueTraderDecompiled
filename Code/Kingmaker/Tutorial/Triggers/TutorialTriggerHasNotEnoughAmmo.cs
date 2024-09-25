using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[ClassInfoBox("Tags: 't|SolutionAbility' - attack ability \n 't|TargetUnit' - unit who has been attacked \n 't|SolutionUnit' - unit who can cast ability")]
[TypeId("4602a5047beb4a3e9a64fea584369769")]
public class TutorialTriggerHasNotEnoughAmmo : TutorialTriggerRulebookEvent<RulePerformAttack>, IHashable
{
	protected override bool ShouldTrigger(RulePerformAttack rule)
	{
		return !rule.Ability.HasEnoughAmmo;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

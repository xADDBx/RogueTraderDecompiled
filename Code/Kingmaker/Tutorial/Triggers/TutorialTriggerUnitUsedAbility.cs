using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("e66c2a26dea143018509e7d5f079aca1")]
public class TutorialTriggerUnitUsedAbility : TutorialTriggerRulebookEvent<RulePerformAbility>, IHashable
{
	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	private BlueprintAbility AbilityBlueprint => m_Ability.Get();

	protected override bool ShouldTrigger(RulePerformAbility rule)
	{
		if (rule.Context.Caster.IsPlayerEnemy)
		{
			return rule.Spell.Blueprint == AbilityBlueprint;
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

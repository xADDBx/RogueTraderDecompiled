using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("07ccf425d1ce6694b9a15eac6e39462c")]
public class WarhammerContextConditionProvokesOverwatch : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Check if ability provokes overwatch";
	}

	protected override bool CheckCondition()
	{
		BlueprintAbility blueprintAbility = base.Context.SourceAbilityContext?.Ability?.Blueprint;
		if (blueprintAbility == null)
		{
			return false;
		}
		return blueprintAbility.UsingInOverwatchArea == BlueprintAbility.UsingInOverwatchAreaType.WillCauseAttack;
	}
}

using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Warhammer.SpaceCombat.AI;

public static class PartUnitBrainStarshipExtension
{
	public static AbilitySettings GetStarshipAbilitySettings(this PartUnitBrain brain, BlueprintAbility ability)
	{
		return ((BlueprintStarshipBrain)(brain?.Blueprint))?.GetAbilitySettings(ability);
	}
}

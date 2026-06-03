using System.Collections.Generic;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityGetFirstSingleTargetForTooltip : IAbilityGetTooltipTarget
{
	IEnumerable<AbilityData> TooltipSingleTarget(AbilityData rootAbility);
}

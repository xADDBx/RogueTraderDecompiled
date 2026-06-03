using System.Collections.Generic;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityGetAllTargetsForTooltip : IAbilityGetTooltipTarget
{
	IEnumerable<AbilityData> TooltipMultipleTargets(AbilityData rootAbility);
}

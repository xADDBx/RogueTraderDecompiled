using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityMultiTarget
{
	bool TryGetNextTargetAbilityAndCaster(AbilityData rootAbility, int targetIndex, out BlueprintAbility ability, out MechanicEntity caster);

	IEnumerable<AbilityData> GetAllTargetsForTooltip(AbilityData rootAbility);
}

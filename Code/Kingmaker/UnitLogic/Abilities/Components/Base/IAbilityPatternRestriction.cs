using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityPatternRestriction
{
	bool IsPatternRestrictionPassed(AbilityData ability, MechanicEntity caster, TargetWrapper target);

	string GetAbilityPatternRestrictionUIText(AbilityData ability, MechanicEntity caster, TargetWrapper target);
}

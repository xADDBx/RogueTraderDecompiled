using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Parts;

public static class PartAbilityImmunityExtension
{
	public static bool HasAbilityImmunity(this MechanicEntity entity, BlueprintAbility ability)
	{
		return entity.GetOptional<PartAbilityImmunity>()?.IsImmuneTo(ability) ?? false;
	}
}

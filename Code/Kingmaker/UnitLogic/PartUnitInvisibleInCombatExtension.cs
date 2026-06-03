using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic;

public static class PartUnitInvisibleInCombatExtension
{
	[CanBeNull]
	public static PartUnitInvisibleInCombat GetUnitInvisibleInCombatOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitInvisibleInCombat>();
	}

	public static bool IsInvisibleInCombat(this MechanicEntity entity)
	{
		return entity.GetUnitInvisibleInCombatOptional()?.IsGhosted ?? false;
	}
}

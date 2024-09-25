using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartUnitCombatStateExtension
{
	[CanBeNull]
	public static PartStatsContainer GetStatsContainerOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartStatsContainer>();
	}
}

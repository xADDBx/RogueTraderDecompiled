using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartStarshipMoraleExtension
{
	[CanBeNull]
	public static PartStarshipMorale GetStarshipMoraleOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartStarshipMorale>();
	}
}

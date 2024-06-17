using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Squads;

public static class PartSquadExtension
{
	[CanBeNull]
	public static PartSquad GetSquadOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartSquad>();
	}
}

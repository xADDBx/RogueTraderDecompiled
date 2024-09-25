using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public static class PartStarshipShieldsExtension
{
	[CanBeNull]
	public static PartStarshipShields GetStarshipShieldsOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartStarshipShields>();
	}
}

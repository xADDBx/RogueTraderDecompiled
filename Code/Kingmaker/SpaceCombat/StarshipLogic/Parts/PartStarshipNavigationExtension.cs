using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public static class PartStarshipNavigationExtension
{
	[CanBeNull]
	public static PartStarshipNavigation GetStarshipNavigationOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartStarshipNavigation>();
	}
}

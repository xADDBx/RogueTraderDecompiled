using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public static class PartStarshipProgressionExtension
{
	[CanBeNull]
	public static PartStarshipProgression GetStarshipProgressionOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartStarshipProgression>();
	}
}

using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public static class PartStarshipEngineExtension
{
	[CanBeNull]
	public static PartStarshipEngine GetStarshipEngineOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartStarshipEngine>();
	}
}

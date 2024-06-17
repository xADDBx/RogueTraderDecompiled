using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic;

public static class PartAbilityResourceCollectionExtension
{
	[CanBeNull]
	public static PartAbilityResourceCollection GetAbilityResourcesOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartAbilityResourceCollection>();
	}
}

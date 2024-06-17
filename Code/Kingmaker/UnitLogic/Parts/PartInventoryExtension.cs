using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartInventoryExtension
{
	[CanBeNull]
	public static PartInventory GetInventoryOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartInventory>();
	}
}

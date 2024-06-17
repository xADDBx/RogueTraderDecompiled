using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartWarhammerMovementApPerCellThreateningAreaExtension
{
	[CanBeNull]
	public static PartWarhammerMovementApPerCellThreateningArea GetWarhammerMovementApPerCellThreateningAreaOptional(this BaseUnitEntity entity)
	{
		return entity.GetOptional<PartWarhammerMovementApPerCellThreateningArea>();
	}
}

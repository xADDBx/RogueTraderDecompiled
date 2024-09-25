using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Commands;

public static class PartUnitCommandsExtension
{
	[CanBeNull]
	public static PartUnitCommands GetCommandsOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitCommands>();
	}
}

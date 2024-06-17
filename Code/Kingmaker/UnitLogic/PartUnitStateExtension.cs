using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic;

public static class PartUnitStateExtension
{
	[CanBeNull]
	public static PartUnitState GetStateOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitState>();
	}
}

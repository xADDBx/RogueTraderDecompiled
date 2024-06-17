using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic;

public static class PartUnitBrainExtension
{
	[CanBeNull]
	public static PartUnitBrain GetBrainOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitBrain>();
	}
}

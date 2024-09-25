using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic;

public static class PartUnitProgressionExtension
{
	[CanBeNull]
	public static PartUnitProgression GetProgressionOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitProgression>();
	}
}

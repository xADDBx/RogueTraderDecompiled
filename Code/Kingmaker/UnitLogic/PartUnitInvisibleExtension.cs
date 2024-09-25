using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic;

public static class PartUnitInvisibleExtension
{
	[CanBeNull]
	public static PartUnitInvisible GetInvisibilityOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitInvisible>();
	}

	public static bool IsInvisible(this MechanicEntity entity)
	{
		return entity.GetInvisibilityOptional() != null;
	}
}

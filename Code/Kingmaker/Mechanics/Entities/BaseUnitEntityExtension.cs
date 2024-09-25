using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility;

namespace Kingmaker.Mechanics.Entities;

public static class BaseUnitEntityExtension
{
	public static TargetWrapper ToTargetWrapper(this BaseUnitEntity entity)
	{
		return entity;
	}

	public static ITargetWrapper ToITargetWrapper(this MechanicEntity entity)
	{
		return (TargetWrapper)entity;
	}

	public static bool IsPreview([CanBeNull] this MechanicEntity entity)
	{
		if (entity is BaseUnitEntity baseUnitEntity)
		{
			return baseUnitEntity.IsPreviewUnit;
		}
		return false;
	}
}

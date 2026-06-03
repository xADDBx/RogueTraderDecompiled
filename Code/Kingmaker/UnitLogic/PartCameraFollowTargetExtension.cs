using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic;

public static class PartCameraFollowTargetExtension
{
	[CanBeNull]
	public static PartCameraFollowTarget GetCameraFollowTargetOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartCameraFollowTarget>();
	}

	public static bool IsForceIgnoreCameraFollow(this MechanicEntity entity)
	{
		return entity.GetCameraFollowTargetOptional()?.ForceIgnore ?? false;
	}
}

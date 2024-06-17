using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.EntitySystem;

public static class EntityHelper
{
	public static bool InRangeInCells([NotNull] this MechanicEntity entity, [NotNull] MechanicEntity center, int radius)
	{
		return entity.DistanceToInCells(center) <= radius;
	}

	public static bool InRangeInCells(this MechanicEntity entity, Vector3 center, int radius)
	{
		return entity.DistanceToInCells(center) <= radius;
	}

	public static bool InRangeInCells(this MechanicEntity entity, Vector3 center, IntRect targetSize, int radius)
	{
		return entity.DistanceToInCells(center, targetSize) <= radius;
	}

	public static bool InRangeInCells([NotNull] this MechanicEntity entity, [NotNull] TargetWrapper center, int radius)
	{
		if (!center.IsPoint)
		{
			if (center.Entity != null)
			{
				return entity.InRangeInCells(center.Entity, radius);
			}
			return false;
		}
		return entity.InRangeInCells(center.Point, radius);
	}

	public static int DistanceToInCells([NotNull] this MechanicEntity entity, [NotNull] MechanicEntity other)
	{
		return entity.DistanceToInCells(other.Position, other.SizeRect, other.Forward);
	}

	public static float DistanceTo(this MechanicEntity entity, Vector3 point)
	{
		return GeometryUtils.MechanicsDistance(entity.Position, point);
	}

	public static int DistanceToInCells(this MechanicEntity entity, Vector3 point)
	{
		return entity.DistanceToInCells(point, default(IntRect));
	}

	public static int DistanceToInCells(this MechanicEntity entity, Vector3 point, IntRect targetSize)
	{
		return WarhammerGeometryUtils.DistanceToInCells(entity.Position, entity.SizeRect, entity.Forward, point, targetSize, Vector3.forward);
	}

	public static int DistanceToInCells([NotNull] this MechanicEntity entity, Vector3 point, IntRect targetSize, Vector3 targetForward)
	{
		return WarhammerGeometryUtils.DistanceToInCells(entity.Position, entity.SizeRect, entity.Forward, point, targetSize, targetForward);
	}

	public static float DistanceTo(this MechanicEntity entity, MechanicEntity other)
	{
		return entity.DistanceTo(other.Position);
	}

	public static float SqrDistanceTo(this MechanicEntity entity, MechanicEntity other)
	{
		return entity.SqrDistanceTo(other.Position);
	}

	public static float SqrDistanceTo(this MechanicEntity entity, Vector3 point)
	{
		return GeometryUtils.SqrMechanicsDistance(entity.Position, point);
	}
}

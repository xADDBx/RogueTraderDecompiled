using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.EntitySystem;

public static class IEntityHelper
{
	public static bool InRangeInCells(this IMechanicEntity entity, IMechanicEntity center, int radius)
	{
		return entity.DistanceToInCells(center) <= radius;
	}

	public static bool InRangeInCells(this IMechanicEntity entity, Vector3 center, int radius)
	{
		return entity.DistanceToInCells(center) <= radius;
	}

	public static bool InRangeInCells(this IMechanicEntity entity, Vector3 center, IntRect targetSize, int radius)
	{
		return entity.DistanceToInCells(center, targetSize) <= radius;
	}

	public static bool InRangeInCells(this IMechanicEntity entity, ITargetWrapper center, int radius)
	{
		if (!center.IsPoint)
		{
			return entity.InRangeInCells(center.IEntity, radius);
		}
		return entity.InRangeInCells(center.Point, radius);
	}

	public static int DistanceToInCells(this IMechanicEntity entity, IMechanicEntity other)
	{
		return entity.DistanceToInCells(other.Position, other.SizeRect, other.Forward);
	}

	public static float DistanceTo(this IMechanicEntity entity, Vector3 point)
	{
		return GeometryUtils.MechanicsDistance(entity.Position, point);
	}

	public static int DistanceToInCells(this IMechanicEntity entity, Vector3 point)
	{
		return entity.DistanceToInCells(point, default(IntRect));
	}

	public static int DistanceToInCells(this IMechanicEntity entity, Vector3 point, IntRect targetSize)
	{
		return WarhammerGeometryUtils.DistanceToInCells(entity.Position, entity.SizeRect, entity.Forward, point, targetSize, Vector3.forward);
	}

	public static int DistanceToInCells(this IMechanicEntity entity, Vector3 point, IntRect targetSize, Vector3 targetForward)
	{
		return WarhammerGeometryUtils.DistanceToInCells(entity.Position, entity.SizeRect, entity.Forward, point, targetSize, targetForward);
	}

	public static float DistanceTo(this IMechanicEntity entity, IMechanicEntity other)
	{
		return entity.DistanceTo(other.Position);
	}

	public static float SqrDistanceTo(this IMechanicEntity entity, IMechanicEntity other)
	{
		return entity.SqrDistanceTo(other.Position);
	}

	public static float SqrDistanceTo(this IMechanicEntity entity, Vector3 point)
	{
		return GeometryUtils.SqrMechanicsDistance(entity.Position, point);
	}

	public static Entity ToEntity(this IEntity entity)
	{
		return (Entity)entity;
	}
}

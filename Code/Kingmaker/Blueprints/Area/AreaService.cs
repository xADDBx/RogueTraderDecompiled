using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Area;

public static class AreaService
{
	private readonly struct MinAggregation : IComparable<MinAggregation>
	{
		public readonly Vector3 Target;

		public readonly float Distance;

		public readonly BlueprintAreaPart Part;

		public MinAggregation(Vector3 target)
		{
			Target = target;
			Distance = float.MaxValue;
			Part = null;
		}

		public MinAggregation(Vector3 target, BlueprintAreaPart part)
		{
			Target = target;
			Distance = GeometryUtils.ManhattanDistance2D(target, part.Bounds.MechanicBounds);
			Part = part;
		}

		public int CompareTo(MinAggregation other)
		{
			float distance = Distance;
			return distance.CompareTo(other.Distance);
		}

		public static bool operator <(MinAggregation a, MinAggregation b)
		{
			return a.CompareTo(b) < 0;
		}

		public static bool operator >(MinAggregation a, MinAggregation b)
		{
			return a.CompareTo(b) > 0;
		}
	}

	private static readonly Func<MinAggregation, BlueprintAreaPart, MinAggregation> MinByMechanic = delegate(MinAggregation currentMin, BlueprintAreaPart part)
	{
		if (!(part?.Bounds))
		{
			return currentMin;
		}
		MinAggregation minAggregation = new MinAggregation(currentMin.Target, part);
		return (!(minAggregation < currentMin)) ? currentMin : minAggregation;
	};

	public static BlueprintAreaPart FindMechanicBoundsContainsPoint(Vector3 point, bool fallbackToClosest = true)
	{
		return FindMechanicBoundsContainsPoint(Game.Instance.CurrentlyLoadedArea, point, fallbackToClosest);
	}

	[CanBeNull]
	public static BlueprintAreaPart FindMechanicBoundsContainsPoint([NotNull] BlueprintArea area, Vector3 point, bool fallbackToClosest = true)
	{
		using (ProfileScope.NewScope("FindMechanicBoundsContainsPoint"))
		{
			MinAggregation minAggregation = area.PartsAndSelf.Aggregate(new MinAggregation(point), MinByMechanic);
			return (minAggregation.Distance <= 0f || fallbackToClosest) ? minAggregation.Part : null;
		}
	}

	public static bool IsInMechanicBounds(Vector3 point)
	{
		AreaPartBounds areaPartBounds = Game.Instance.CurrentlyLoadedAreaPart?.Bounds;
		if ((bool)areaPartBounds)
		{
			return areaPartBounds.MechanicBounds.ContainsXZ(point);
		}
		return false;
	}
}

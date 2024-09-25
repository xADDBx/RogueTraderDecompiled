using System;
using Kingmaker.Utility.GeometryExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class PathExtras
{
	public static bool IsDoneAndPostProcessed(this Path path)
	{
		return PathState.Returned <= path.PipelineState;
	}

	public static bool IsDiagonal(CustomGridNodeBase a, CustomGridNodeBase b)
	{
		if (a.XCoordinateInGrid != b.XCoordinateInGrid)
		{
			return a.ZCoordinateInGrid != b.ZCoordinateInGrid;
		}
		return false;
	}

	public static int LengthInCells(this Path path, bool startFromOddDiagonal = false, int pointsToTake = int.MaxValue)
	{
		int num = Math.Min(path.vectorPath.Count, pointsToTake);
		if (num < 2)
		{
			return 0;
		}
		int num2 = path.DiagonalsCount(pointsToTake);
		return num - 1 + (num2 + (startFromOddDiagonal ? 1 : 0)) / 2;
	}

	public static int DiagonalsCount(this Path path, int pointsToTake = int.MaxValue)
	{
		int num = Math.Min(path.vectorPath.Count, pointsToTake);
		if (num < 2)
		{
			return 0;
		}
		int num2 = 0;
		for (int i = 1; i < num; i++)
		{
			Vector2 a = path.vectorPath[i - 1].To2D();
			Vector2 b = path.vectorPath[i].To2D();
			if (a.IsDiagonal(b))
			{
				num2++;
			}
		}
		return num2;
	}
}

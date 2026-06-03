using System;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Utility;

public static class WarhammerGeometryUtils
{
	public readonly struct SquaredFootprint
	{
		public readonly IntRect SubRect;

		public readonly int Count;

		public readonly int DirX;

		public readonly int DirZ;

		public readonly int DirIdx;

		public SquaredFootprint(IntRect size, Vector3 forward)
		{
			Vector3 vector = forward.ToCellSizedVector();
			SubRect = new IntRect(size.xmin, size.ymin, size.xmax, size.ymin + size.Width - 1);
			Count = Math.Max(1, size.Height - size.Width + 1);
			DirX = Math.Sign(vector.x);
			DirZ = Math.Sign(vector.z);
			DirIdx = CustomGraphHelper.GuessDirection(forward);
		}

		public Vector2Int SubRectOffset(int index)
		{
			return new Vector2Int(-DirX * index, -DirZ * index);
		}
	}

	public static int DistanceToInCells(Vector2Int delta, IntRect fromSize, IntRect toSize)
	{
		delta.x = ((delta.x > 0) ? Math.Max(delta.x - fromSize.xmax + toSize.xmin, 0) : Math.Min(delta.x - fromSize.xmin + toSize.xmax, 0));
		delta.y = ((delta.y > 0) ? Math.Max(delta.y - fromSize.ymax + toSize.ymin, 0) : Math.Min(delta.y - fromSize.ymin + toSize.ymax, 0));
		int b = Mathf.Abs(delta.y);
		int a = Mathf.Abs(delta.x);
		int num = Mathf.Max(a, b);
		int num2 = Mathf.Min(a, b);
		return num + num2 / 2;
	}

	public static int DistanceToInCells(Vector2Int from, IntRect fromSize, Vector2Int to, IntRect toSize)
	{
		return DistanceToInCells(to - from, fromSize, toSize);
	}

	public static int DistanceToInCells(Vector3 from, IntRect fromSize, Vector3 to, IntRect toSize)
	{
		return DistanceToInCells(Vector2Int.RoundToInt((to.To2D() - from.To2D()) / GraphParamsMechanicsCache.GridCellSize), fromSize, toSize);
	}

	public static int DistanceToInCells(Vector3 from, IntRect fromSize, Vector3 fromDir, Vector3 to, IntRect toSize, Vector3 toDir)
	{
		using (ProfileScope.New("WarhammerGeometryUtils.DistanceToInCells_0"))
		{
			SquaredFootprint footprint = new SquaredFootprint(fromSize, fromDir);
			SquaredFootprint footprint2 = new SquaredFootprint(toSize, toDir);
			from += ComputeCenteringShift(fromSize, footprint);
			to += ComputeCenteringShift(toSize, footprint2);
			float gridCellSize = GraphParamsMechanicsCache.GridCellSize;
			int num = int.MaxValue;
			for (int i = 0; i < footprint.Count; i++)
			{
				Vector2Int vector2Int = footprint.SubRectOffset(i);
				Vector3 from2 = from + new Vector3((float)vector2Int.x * gridCellSize, 0f, (float)vector2Int.y * gridCellSize);
				for (int j = 0; j < footprint2.Count; j++)
				{
					Vector2Int vector2Int2 = footprint2.SubRectOffset(j);
					Vector3 to2 = to + new Vector3((float)vector2Int2.x * gridCellSize, 0f, (float)vector2Int2.y * gridCellSize);
					int val = DistanceToInCells(from2, footprint.SubRect, to2, footprint2.SubRect);
					num = Math.Min(num, val);
				}
			}
			return num;
		}
	}

	public static Vector2Int ComputeCenteringShiftCells(IntRect size, SquaredFootprint footprint)
	{
		if (size.Height <= size.Width)
		{
			return Vector2Int.zero;
		}
		Vector2Int vector2Int = footprint.SubRectOffset(footprint.Count - 1);
		int num = footprint.SubRect.xmin + Math.Min(0, vector2Int.x);
		int num2 = footprint.SubRect.ymin + Math.Min(0, vector2Int.y);
		IntRect bounds = GridAreaHelper.GetOffsets(size, footprint.DirIdx).Bounds;
		return new Vector2Int(bounds.xmin - num, bounds.ymin - num2);
	}

	public static Vector2Int ComputeCenteringShiftCells(IntRect size, Vector3 forward)
	{
		return ComputeCenteringShiftCells(size, new SquaredFootprint(size, forward));
	}

	private static Vector3 ComputeCenteringShift(IntRect size, SquaredFootprint footprint)
	{
		Vector2Int vector2Int = ComputeCenteringShiftCells(size, footprint);
		if (vector2Int == Vector2Int.zero)
		{
			return Vector3.zero;
		}
		float gridCellSize = GraphParamsMechanicsCache.GridCellSize;
		return new Vector3((float)vector2Int.x * gridCellSize, 0f, (float)vector2Int.y * gridCellSize);
	}
}

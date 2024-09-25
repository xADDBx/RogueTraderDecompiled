using System;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Utility;

public static class WarhammerGeometryUtils
{
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
			fromDir = fromDir.ToCellSizedVector();
			toDir = toDir.ToCellSizedVector();
			int num = int.MaxValue;
			IntRect fromSize2 = new IntRect(fromSize.xmin, fromSize.ymin, fromSize.xmax, fromSize.ymin + fromSize.Width - 1);
			IntRect toSize2 = new IntRect(toSize.xmin, toSize.ymin, toSize.xmax, toSize.ymin + toSize.Width - 1);
			for (int i = 0; i <= fromSize.Height - fromSize.Width; i++)
			{
				for (int j = 0; j <= toSize.Height - toSize.Width; j++)
				{
					int val = DistanceToInCells(from - fromDir * i, fromSize2, to - toDir * j, toSize2);
					num = Math.Min(num, val);
				}
			}
			return num;
		}
	}

	private static Vector3 AdjustDirection(Vector3 dir)
	{
		if (dir.sqrMagnitude > 1.1f || dir.sqrMagnitude < 0.9f)
		{
			throw new ArgumentException("Need nonzero vector", "dir");
		}
		float num = Vector3.Dot(dir, Vector3.right);
		float num2 = Vector3.Dot(dir, Vector3.forward);
		if (num > 0.9f)
		{
			return Vector3.right;
		}
		if (num < -0.9f)
		{
			return Vector3.left;
		}
		if (num2 > 0.9f)
		{
			return Vector3.forward;
		}
		if (num2 < -0.9f)
		{
			return Vector3.back;
		}
		if (num > 0.1f && num2 > 0.1f)
		{
			return (Vector3.right + Vector3.forward).normalized;
		}
		if (num < -0.1f && num2 > 0.1f)
		{
			return (Vector3.left + Vector3.forward).normalized;
		}
		if (num < -0.1f && num2 < -0.1f)
		{
			return (Vector3.left + Vector3.back).normalized;
		}
		if (num > 0.1f && num2 < -0.1f)
		{
			return (Vector3.right + Vector3.back).normalized;
		}
		return Vector3.forward;
	}

	private static Vector3 ToCellSizedVector(this Vector3 v)
	{
		Vector3 result = AdjustDirection(v);
		result = new Vector3((Math.Abs(result.x) < 0.0001f) ? 0f : ((float)Math.Sign(result.x) * GraphParamsMechanicsCache.GridCellSize), 0f, (Math.Abs(result.z) < 0.0001f) ? 0f : ((float)Math.Sign(result.z) * GraphParamsMechanicsCache.GridCellSize));
		return result;
	}
}

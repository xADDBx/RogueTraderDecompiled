using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Utility;

public static class GridPatterns
{
	private readonly struct CanTransitionBetweenCellsImpl : Linecast.ICanTransitionBetweenCells
	{
		private readonly List<CustomGridNodeBase>[] m_Rays;

		private readonly int m_Left1Idx;

		private readonly int m_Left2Idx;

		private readonly int m_Right1Idx;

		private readonly int m_Right2Idx;

		private readonly CustomGridNodeBase m_FromNode;

		private readonly int m_Range;

		public CanTransitionBetweenCellsImpl(List<CustomGridNodeBase>[] rays, int left1Idx, int left2Idx, int right1Idx, int right2Idx, CustomGridNodeBase fromNode, int range)
		{
			m_Rays = rays;
			m_Left1Idx = left1Idx;
			m_Left2Idx = left2Idx;
			m_Right1Idx = right1Idx;
			m_Right2Idx = right2Idx;
			m_FromNode = fromNode;
			m_Range = range;
		}

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			CustomGridNodeBase customGridNodeBase;
			CustomGridNodeBase customGridNodeBase2;
			if (distanceFactor < 0.5f)
			{
				customGridNodeBase = nodeTo;
				customGridNodeBase2 = nodeTo;
			}
			else
			{
				customGridNodeBase = nodeTo.GetNeighbourAlongDirection(m_Left1Idx, checkConnectivity: false) ?? nodeTo.GetNeighbourAlongDirection(m_Left2Idx, checkConnectivity: false);
				customGridNodeBase2 = nodeTo.GetNeighbourAlongDirection(m_Right1Idx, checkConnectivity: false) ?? nodeTo.GetNeighbourAlongDirection(m_Right2Idx, checkConnectivity: false);
			}
			CustomGridNodeBase customGridNodeBase3 = customGridNodeBase?.GetNeighbourAlongDirection(m_Left1Idx, checkConnectivity: false) ?? customGridNodeBase?.GetNeighbourAlongDirection(m_Left2Idx, checkConnectivity: false);
			CustomGridNodeBase customGridNodeBase4 = customGridNodeBase2?.GetNeighbourAlongDirection(m_Right1Idx, checkConnectivity: false) ?? customGridNodeBase2?.GetNeighbourAlongDirection(m_Right2Idx, checkConnectivity: false);
			if (customGridNodeBase3 != null && m_FromNode.CellDistanceTo(customGridNodeBase3) <= m_Range)
			{
				m_Rays[0].Add(customGridNodeBase3);
			}
			if (customGridNodeBase != null && m_FromNode.CellDistanceTo(customGridNodeBase) <= m_Range)
			{
				m_Rays[1].Add(customGridNodeBase);
			}
			if (m_FromNode.CellDistanceTo(nodeTo) <= m_Range)
			{
				m_Rays[2].Add(nodeTo);
			}
			if (customGridNodeBase2 != null && m_FromNode.CellDistanceTo(customGridNodeBase2) <= m_Range)
			{
				m_Rays[3].Add(customGridNodeBase2);
			}
			if (customGridNodeBase4 != null && m_FromNode.CellDistanceTo(customGridNodeBase4) <= m_Range)
			{
				m_Rays[4].Add(customGridNodeBase4);
			}
			return true;
		}
	}

	public interface ILineWriter<T> where T : struct
	{
		void Write(ref T context, int xBegin, int xEnd, int y);
	}

	private static (int left1, int left2, int right1, int right2) GetNeighboursIdx(Int3 origin, Int3 end)
	{
		Int3 @int = end - origin;
		if (Mathf.Abs(@int.x) > Mathf.Abs(@int.z))
		{
			if (@int.x > 0)
			{
				if (@int.z <= 0)
				{
					return (left1: 2, left2: 1, right1: 0, right2: 3);
				}
				return (left1: 2, left2: 3, right1: 0, right2: 1);
			}
			if (@int.z <= 0)
			{
				return (left1: 0, left2: 1, right1: 2, right2: 3);
			}
			return (left1: 0, left2: 3, right1: 2, right2: 1);
		}
		if (@int.z > 0)
		{
			if (@int.x <= 0)
			{
				return (left1: 3, left2: 0, right1: 1, right2: 2);
			}
			return (left1: 3, left2: 2, right1: 1, right2: 0);
		}
		if (@int.x <= 0)
		{
			return (left1: 1, left2: 0, right1: 3, right2: 2);
		}
		return (left1: 1, left2: 2, right1: 3, right2: 0);
	}

	public static List<CustomGridNodeBase> CalcScatterShotMainLine(CustomGridNodeBase fromNode, CustomGridNodeBase toNode, int range)
	{
		return CalcScatterShot(fromNode, toNode, range)[2];
	}

	public static List<CustomGridNodeBase>[] CalcScatterShot(CustomGridNodeBase fromNode, CustomGridNodeBase toNode, int range)
	{
		List<CustomGridNodeBase>[] array = new List<CustomGridNodeBase>[5]
		{
			new List<CustomGridNodeBase>(),
			new List<CustomGridNodeBase>(),
			new List<CustomGridNodeBase>(),
			new List<CustomGridNodeBase>(),
			new List<CustomGridNodeBase>()
		};
		Vector3 vector3Position = fromNode.Vector3Position;
		Vector3 vector3Position2 = toNode.Vector3Position;
		vector3Position2 = vector3Position + (vector3Position2 - vector3Position).normalized * range.Cells().Meters;
		(int left1, int left2, int right1, int right2) neighboursIdx = GetNeighboursIdx((Int3)vector3Position, (Int3)vector3Position2);
		int item = neighboursIdx.left1;
		int item2 = neighboursIdx.left2;
		int item3 = neighboursIdx.right1;
		int item4 = neighboursIdx.right2;
		CanTransitionBetweenCellsImpl condition = new CanTransitionBetweenCellsImpl(array, item, item2, item3, item4, fromNode, range);
		Linecast.LinecastGrid2(fromNode.Graph, vector3Position, vector3Position2, fromNode, out var _, NNConstraint.None, ref condition);
		return array;
	}

	public static void AddCircleNodes(HashSet<Vector2Int> nodes, int radius, Size entitySizeRect = Size.Medium)
	{
		AccumulateNodesInRadius(radius, nodes);
		ExtendAreaByEntitySize(nodes, entitySizeRect);
	}

	public static PatternGridData ConstructPattern(PatternType pattern, int radius, int angle, Vector2 direction, Size entitySizeRect)
	{
		HashSet<Vector2Int> hashSet = TempHashSet.Get<Vector2Int>();
		GetOrientedNodes(hashSet, pattern, radius, angle, direction, entitySizeRect);
		return PatternGridData.Create(hashSet, disposable: true);
	}

	private static void GetOrientedNodes(HashSet<Vector2Int> result, PatternType pattern, int radius, int angle, Vector2 direction, Size entitySizeRect)
	{
		float sqrMagnitude = direction.sqrMagnitude;
		if (sqrMagnitude > 1.1f || sqrMagnitude < 0.9f)
		{
			throw new ArgumentException("Need nonzero vector", "direction");
		}
		switch (pattern)
		{
		case PatternType.Circle:
			AddCircleNodes(result, radius, entitySizeRect);
			break;
		case PatternType.Cone:
			AddConeNodes(result, radius, angle, direction);
			break;
		case PatternType.Sector:
			AddSectorNodes(result, radius, angle, direction);
			break;
		case PatternType.Ray:
			AddRayNodes(result, radius, direction);
			break;
		default:
			throw new ArgumentOutOfRangeException("pattern", pattern, null);
		}
	}

	private static void ExtendAreaByEntitySize(HashSet<Vector2Int> nodes, Size entitySizeRect)
	{
		if (entitySizeRect.Is1x1())
		{
			return;
		}
		IntRect rectForSize = SizePathfindingHelper.GetRectForSize(entitySizeRect);
		for (int i = rectForSize.ymin; i <= rectForSize.ymax; i++)
		{
			if (i == 0)
			{
				continue;
			}
			foreach (Vector2Int item in nodes.ToList())
			{
				if (!nodes.Contains(item + Vector2Int.up * i))
				{
					nodes.Add(item + Vector2Int.up * i);
				}
			}
		}
		for (int j = rectForSize.xmin; j <= rectForSize.xmax; j++)
		{
			if (j == 0)
			{
				continue;
			}
			foreach (Vector2Int item2 in nodes.ToList())
			{
				if (!nodes.Contains(item2 + Vector2Int.right * j))
				{
					nodes.Add(item2 + Vector2Int.right * j);
				}
			}
		}
	}

	private static void AccumulateNodesInRadius(int radius, HashSet<Vector2Int> nodes)
	{
		for (int i = -radius; i < radius + 1; i++)
		{
			for (int j = -radius; j < radius + 1; j++)
			{
				Vector2Int vector2Int = new Vector2Int(i, j);
				if (!nodes.Contains(vector2Int) && CustomGraphHelper.GetWarhammerLength(vector2Int) <= radius)
				{
					nodes.Add(vector2Int);
				}
			}
		}
	}

	public static void AddConeNodes(HashSet<Vector2Int> nodes, int radius, float degrees, Vector2 dir)
	{
		float sqrMagnitude = dir.sqrMagnitude;
		if (sqrMagnitude > 1.1f || sqrMagnitude < 0.9f)
		{
			throw new ArgumentException("Need nonzero vector", "dir");
		}
		if (degrees >= 180f)
		{
			throw new ArgumentException("Angle must be less then 180", "degrees");
		}
		AddRayNodes(nodes, radius, dir);
		float realRadius = nodes.MaxBy((Vector2Int v) => v.sqrMagnitude).magnitude;
		Vector2 vector = Quaternion.Euler(0f, 0f, degrees / 2f) * dir;
		Vector2 vector2 = Quaternion.Euler(0f, 0f, (0f - degrees) / 2f) * dir;
		AddRayNodes(nodes, (Vector2Int n) => Vector2.Dot(dir, n) <= realRadius, vector);
		Vector2Int leftSideVertex = nodes.MaxBy((Vector2Int v) => CustomGraphHelper.GetWarhammerLength(v));
		AddRayNodes(nodes, (Vector2Int n) => Vector2.Dot(dir, n) <= realRadius, vector2);
		nodes.Except((Vector2Int v) => v == leftSideVertex).MaxBy((Vector2Int v) => CustomGraphHelper.GetWarhammerLength(v));
		float num = Math.Min(Vector2.Dot(dir, vector), Vector3.Dot(dir, vector2));
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		foreach (Vector2Int node in nodes)
		{
			if (node.x < num2)
			{
				num2 = node.x;
			}
			if (node.x > num3)
			{
				num3 = node.x;
			}
			if (node.y < num5)
			{
				num5 = node.y;
			}
			if (node.y > num4)
			{
				num4 = node.y;
			}
		}
		for (int i = num2; i < num3 + 1; i++)
		{
			for (int j = num5; j < num4 + 1; j++)
			{
				Vector2Int vector2Int = new Vector2Int(i, j);
				float num6 = Vector2.Dot(dir, vector2Int);
				if (!nodes.Contains(vector2Int) && num6 / vector2Int.magnitude >= num && num6 <= realRadius)
				{
					nodes.Add(vector2Int);
				}
			}
		}
	}

	private static void AddRayNodes(HashSet<Vector2Int> nodes, Func<Vector2Int, bool> pred, Vector2 dir)
	{
		float sqrMagnitude = dir.sqrMagnitude;
		if (sqrMagnitude > 1.1f || sqrMagnitude < 0.9f)
		{
			throw new ArgumentException("Need nonzero vector", "dir");
		}
		foreach (Vector2Int item in new Linecast.Ray2NodeOffsets(Vector2Int.zero, dir))
		{
			if (!pred(item))
			{
				break;
			}
			nodes.Add(item);
		}
	}

	public static void AddRayNodes(HashSet<Vector2Int> nodes, int length, Vector2 dir)
	{
		float sqrMagnitude = dir.sqrMagnitude;
		if (sqrMagnitude > 1.1f || sqrMagnitude < 0.9f)
		{
			throw new ArgumentException("Need nonzero vector", "dir");
		}
		foreach (Vector2Int item in new Linecast.Ray2NodeOffsets(Vector2Int.zero, dir))
		{
			if (CustomGraphHelper.GetWarhammerLength(item) > length)
			{
				break;
			}
			nodes.Add(item);
		}
	}

	public static void AddSectorNodes(HashSet<Vector2Int> nodes, int radius, int degrees, Vector2 dir)
	{
		float sqrMagnitude = dir.sqrMagnitude;
		if (sqrMagnitude > 1.1f || sqrMagnitude < 0.9f)
		{
			throw new ArgumentException("Need nonzero vector", "dir");
		}
		Vector2 vector = Quaternion.Euler(0f, 0f, (float)degrees / 2f) * dir;
		Vector2 vector2 = Quaternion.Euler(0f, 0f, (float)(-degrees) / 2f) * dir;
		AddRayNodes(nodes, radius, vector);
		AddRayNodes(nodes, radius, vector2);
		float num = Math.Min(Vector2.Dot(dir, vector), Vector3.Dot(dir, vector2));
		for (int i = -radius; i < radius + 1; i++)
		{
			for (int j = -radius; j < radius + 1; j++)
			{
				Vector2Int vector2Int = new Vector2Int(i, j);
				if (!nodes.Contains(vector2Int) && Vector2.Dot(dir, new Vector2(vector2Int.x, vector2Int.y).normalized) >= num && CustomGraphHelper.GetWarhammerLength(vector2Int) <= radius)
				{
					nodes.Add(vector2Int);
				}
			}
		}
	}

	public static bool TryGetEnclosingRect(in NodeList nodes, out IntRect result)
	{
		result = new IntRect(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
		foreach (CustomGridNodeBase node in nodes)
		{
			int xCoordinateInGrid = node.XCoordinateInGrid;
			int zCoordinateInGrid = node.ZCoordinateInGrid;
			if (result.xmin > xCoordinateInGrid)
			{
				result.xmin = xCoordinateInGrid;
			}
			if (result.xmax < xCoordinateInGrid)
			{
				result.xmax = xCoordinateInGrid;
			}
			if (result.ymin > zCoordinateInGrid)
			{
				result.ymin = zCoordinateInGrid;
			}
			if (result.ymax < zCoordinateInGrid)
			{
				result.ymax = zCoordinateInGrid;
			}
		}
		if (result.xmin <= result.xmax)
		{
			return result.ymin <= result.ymax;
		}
		return false;
	}

	public static void GenerateCircle<TContext, TWriter>(int radius, int unitPosX, int unitPosY, ref TContext context, ref TWriter writer) where TContext : struct where TWriter : struct, ILineWriter<TContext>
	{
		GenerateRoundedRectangle(-1, radius, unitPosX, unitPosY, 1, 1, ref context, ref writer);
	}

	public static void GenerateCircle<TContext, TWriter>(int innerRadius, int outerRadius, int unitPosX, int unitPosY, ref TContext context, ref TWriter writer) where TContext : struct where TWriter : struct, ILineWriter<TContext>
	{
		GenerateRoundedRectangle(innerRadius, outerRadius, unitPosX, unitPosY, 1, 1, ref context, ref writer);
	}

	public static void GenerateRoundedRectangle<TContext, TWriter>(int radius, int unitPosX, int unitPosY, int unitWidth, int unitHeight, ref TContext context, ref TWriter writer) where TContext : struct where TWriter : struct, ILineWriter<TContext>
	{
		GenerateRoundedRectangle(-1, radius, unitPosX, unitPosY, unitWidth, unitHeight, ref context, ref writer);
	}

	public static void GenerateRoundedRectangle<TContext, TWriter>(int innerRadius, int outerRadius, int unitPosX, int unitPosY, int unitWidth, int unitHeight, ref TContext context, ref TWriter writer) where TContext : struct where TWriter : struct, ILineWriter<TContext>
	{
		if (outerRadius >= 0 && unitWidth >= 1 && unitHeight >= 1)
		{
			if (innerRadius >= outerRadius)
			{
				innerRadius = outerRadius - 1;
			}
			if (innerRadius >= 0)
			{
				GenerateRingRoundedRectangleInternal(innerRadius, outerRadius, unitPosX, unitPosY, unitWidth, unitHeight, ref context, ref writer);
			}
			else
			{
				GenerateSolidRoundedRectangleInternal(outerRadius, unitPosX, unitPosY, unitWidth, unitHeight, ref context, ref writer);
			}
		}
	}

	private static void GenerateSolidRoundedRectangleInternal<TContext, TWriter>(int radius, int unitPosX, int unitPosY, int unitWidth, int unitHeight, ref TContext context, ref TWriter writer) where TContext : struct where TWriter : struct, ILineWriter<TContext>
	{
		int num = Mathf.CeilToInt((float)(radius + 1) * 2f / 3f) - 1;
		int num2 = radius - num;
		int num3 = unitPosX + unitWidth;
		int num4 = -radius;
		int num5 = unitPosY - radius;
		int num6 = 0;
		while (num6 < num2)
		{
			int num7 = (radius + num4) * 2 + 1;
			writer.Write(ref context, unitPosX - num7, num3 + num7, num5++);
			num6++;
			num4++;
		}
		int num8 = 0;
		while (num8 < num)
		{
			int num9 = radius + num4 / 2;
			writer.Write(ref context, unitPosX - num9, num3 + num9, num5++);
			num8++;
			num4++;
		}
		int xBegin = unitPosX - radius;
		int xEnd = num3 + radius;
		for (int i = 0; i < unitHeight; i++)
		{
			writer.Write(ref context, xBegin, xEnd, num5++);
		}
		num4++;
		int num10 = 0;
		while (num10 < num)
		{
			int num11 = radius - num4 / 2;
			writer.Write(ref context, unitPosX - num11, num3 + num11, num5++);
			num10++;
			num4++;
		}
		int num12 = 0;
		while (num12 < num2)
		{
			int num13 = (radius - num4) * 2 + 1;
			writer.Write(ref context, unitPosX - num13, num3 + num13, num5++);
			num12++;
			num4++;
		}
	}

	private static void GenerateRingRoundedRectangleInternal<TContext, TWriter>(int innerRadius, int outerRadius, int unitPosX, int unitPosY, int unitWidth, int unitHeight, ref TContext context, ref TWriter writer) where TContext : struct where TWriter : struct, ILineWriter<TContext>
	{
		int num = unitPosX + unitWidth;
		int num2 = outerRadius - innerRadius;
		int num3 = -outerRadius;
		int y = unitPosY - outerRadius;
		int num4 = 0;
		while (num4 < num2)
		{
			int num5 = Mathf.Min(outerRadius + num3 / 2, (outerRadius + num3) * 2 + 1);
			writer.Write(ref context, unitPosX - num5, num + num5, y++);
			num4++;
			num3++;
		}
		int num6 = 0;
		while (num6 < innerRadius)
		{
			int num7 = num3 / 2;
			int num8 = Mathf.Min(innerRadius + num7, (innerRadius + num3) * 2 + 1);
			int num9 = Mathf.Min(outerRadius + num7, (outerRadius + num3) * 2 + 1);
			writer.Write(ref context, unitPosX - num9, unitPosX - num8, y);
			writer.Write(ref context, num + num8, num + num9, y++);
			num6++;
			num3++;
		}
		num3++;
		for (int i = 0; i < unitHeight; i++)
		{
			writer.Write(ref context, unitPosX - outerRadius, unitPosX - innerRadius, y);
			writer.Write(ref context, num + innerRadius, num + outerRadius, y++);
		}
		int num10 = 0;
		while (num10 < innerRadius)
		{
			int num11 = num3 / 2;
			int num12 = Mathf.Min(innerRadius - num11, (innerRadius - num3) * 2 + 1);
			int num13 = Mathf.Min(outerRadius - num11, (outerRadius - num3) * 2 + 1);
			writer.Write(ref context, unitPosX - num13, unitPosX - num12, y);
			writer.Write(ref context, num + num12, num + num13, y++);
			num10++;
			num3++;
		}
		int num14 = 0;
		while (num14 < num2)
		{
			int num15 = Mathf.Min(outerRadius - num3 / 2, (outerRadius - num3) * 2 + 1);
			writer.Write(ref context, unitPosX - num15, num + num15, y++);
			num14++;
			num3++;
		}
	}
}

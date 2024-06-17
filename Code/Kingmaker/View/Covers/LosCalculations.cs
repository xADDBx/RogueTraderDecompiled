using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.CodeTimer;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.Covers;

public static class LosCalculations
{
	public enum CoverType
	{
		None,
		Half,
		Full,
		Invisible
	}

	public readonly struct DistanceHeight
	{
		public readonly float Factor;

		public readonly float Height;

		public readonly CustomGridNodeBase Node;

		public DistanceHeight(float factor, float height, CustomGridNodeBase node)
		{
			Factor = factor;
			Height = height;
			Node = node;
		}
	}

	private struct HeightAccumulatorVisitor : Linecast.ICanTransitionBetweenCells
	{
		private float m_Height;

		private readonly List<DistanceHeight> m_Heights;

		public HeightAccumulatorVisitor(float height, List<DistanceHeight> heights)
		{
			m_Height = height;
			m_Heights = heights;
		}

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			float y = nodeFrom.Vector3Position.y;
			float y2 = nodeTo.Vector3Position.y;
			if (nodeFrom.HasFenceWithNode(nodeTo, out var fenceHeight))
			{
				float num = (float)fenceHeight * 0.001f - y;
				float num2 = (float)fenceHeight * 0.001f - y2;
				if (num > 1f)
				{
					m_Height += num;
				}
				m_Heights.Add(new DistanceHeight(distanceFactor, m_Height, nodeTo));
				if (num2 > 1f)
				{
					m_Height -= num2;
				}
				m_Heights.Add(new DistanceHeight(distanceFactor, m_Height, nodeTo));
			}
			else
			{
				float num3 = y2 - y;
				if (Mathf.Abs(num3) > 1f)
				{
					m_Height += num3;
				}
				m_Heights.Add(new DistanceHeight(distanceFactor, m_Height, nodeTo));
			}
			return true;
		}
	}

	public static readonly Vector3 EyeShift = Vector3.up * 1.5f;

	private const int HalfCoverMin = 500;

	private const int FullCoverMin = 1500;

	private const float HeightThreshold = 1f;

	private static readonly ThreadLocal<List<DistanceHeight>> DstHeights = new ThreadLocal<List<DistanceHeight>>(() => new List<DistanceHeight>(128));

	public static LosDescription GetCellCoverStatus(CustomGridNodeBase node, int direction)
	{
		CustomGridNodeBase neighbourAlongDirection = node.GetNeighbourAlongDirection(direction, checkConnectivity: false);
		if (neighbourAlongDirection == null)
		{
			return new LosDescription(CoverType.Full);
		}
		if (HasForcedCover(neighbourAlongDirection, out var cover))
		{
			return new LosDescription(cover, new Obstacle(neighbourAlongDirection));
		}
		if (node.GetNeighbourAlongDirection(direction) != null)
		{
			return new LosDescription(CoverType.None);
		}
		int fenceHeight;
		bool flag = node.HasFenceWithNode(neighbourAlongDirection, out fenceHeight);
		int num = (flag ? (fenceHeight - node.position.y) : (neighbourAlongDirection.position.y - node.position.y));
		if (num > 1500)
		{
			return new LosDescription(CoverType.Full, neighbourAlongDirection, flag ? CustomGraphHelper.OppositeDirections[direction] : (-1));
		}
		if (num > 500)
		{
			return new LosDescription(CoverType.Half, neighbourAlongDirection, flag ? CustomGraphHelper.OppositeDirections[direction] : (-1));
		}
		return new LosDescription(CoverType.None);
	}

	private static bool HasForcedCover(CustomGridNodeBase node, out CoverType cover)
	{
		cover = CoverType.None;
		return Game.Instance?.ForcedCoversController.TryGetCoverType(node, out cover) ?? false;
	}

	private static LosDescription GetEffectiveCover(Int2 originPos, CustomGridNodeBase end)
	{
		Int2 end2 = new Int2(end.XCoordinateInGrid, end.ZCoordinateInGrid);
		(int x, int y) tangentNeighboursIdx = GetTangentNeighboursIdx(originPos, end2);
		int item = tangentNeighboursIdx.x;
		int item2 = tangentNeighboursIdx.y;
		LosDescription result = new LosDescription(CoverType.None);
		if (item >= 0)
		{
			LosDescription losDescription = GetCellCoverStatus(end, item);
			if ((CoverType)losDescription == CoverType.Half)
			{
				int num = Math.Abs(end2.x - originPos.x);
				if (losDescription.Obstacle.IsFence ? (num < 2) : (num < 3))
				{
					losDescription = new LosDescription(CoverType.None);
				}
			}
			if (losDescription.CoverType > result.CoverType)
			{
				result = losDescription;
			}
		}
		if (item2 >= 0)
		{
			LosDescription losDescription2 = GetCellCoverStatus(end, item2);
			if ((CoverType)losDescription2 == CoverType.Half)
			{
				int num2 = Math.Abs(end2.y - originPos.y);
				if (losDescription2.Obstacle.IsFence ? (num2 < 2) : (num2 < 3))
				{
					losDescription2 = new LosDescription(CoverType.None);
				}
			}
			if (losDescription2.CoverType > result.CoverType)
			{
				result = losDescription2;
			}
		}
		return result;
	}

	public static Vector3 GetBestShootingPosition(MechanicEntity from, MechanicEntity to)
	{
		return GetBestShootingPosition(from.Position, from.SizeRect, to.Position, to.SizeRect);
	}

	public static Vector3 GetBestShootingPosition(Vector3 shooterPos, IntRect shooterSize, Vector3 targetPos, IntRect targetSize)
	{
		CustomGridNodeBase nearestNodeXZUnwalkable = shooterPos.GetNearestNodeXZUnwalkable();
		CustomGridNodeBase nearestNodeXZUnwalkable2 = targetPos.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null || nearestNodeXZUnwalkable2 == null)
		{
			return shooterPos;
		}
		if (!(nearestNodeXZUnwalkable.Graph is CustomGridGraph))
		{
			return shooterPos;
		}
		return GetBestShootingNode(nearestNodeXZUnwalkable, shooterSize, nearestNodeXZUnwalkable2, targetSize).Vector3Position;
	}

	public static CustomGridNodeBase GetBestShootingNode(CustomGridNodeBase origin, IntRect shooterSize, CustomGridNodeBase end, IntRect targetSize, MechanicEntity currentEntityHit = null)
	{
		Int2 cellIndex = (origin.Graph as CustomGridGraph).GetCellIndex(end.Vector3Position);
		if ((CoverType)GetEffectiveCover(cellIndex, origin) != CoverType.Full)
		{
			return origin;
		}
		(int left, int right) orthoNeighboursIdx = GetOrthoNeighboursIdx(origin.Vector3Position, end.Vector3Position);
		int item = orthoNeighboursIdx.left;
		int item2 = orthoNeighboursIdx.right;
		CustomGridNodeBase neighbourAlongDirection = origin.GetNeighbourAlongDirection(item);
		CustomGridNodeBase neighbourAlongDirection2 = origin.GetNeighbourAlongDirection(item2);
		CoverType coverType = ((neighbourAlongDirection != null && (CoverType)GetEffectiveCover(cellIndex, neighbourAlongDirection) != CoverType.Full) ? ((CoverType)GetWarhammerLos(neighbourAlongDirection, shooterSize, end, targetSize)) : CoverType.Invisible);
		CoverType coverType2 = ((neighbourAlongDirection2 != null && (CoverType)GetEffectiveCover(cellIndex, neighbourAlongDirection2) != CoverType.Full) ? ((CoverType)GetWarhammerLos(neighbourAlongDirection2, shooterSize, end, targetSize)) : CoverType.Invisible);
		if (coverType == coverType2 && coverType2 == CoverType.Invisible)
		{
			return origin;
		}
		CustomGridNodeBase customGridNodeBase = ((coverType <= coverType2) ? neighbourAlongDirection : neighbourAlongDirection2);
		BaseUnitEntity unit = customGridNodeBase.GetUnit();
		if (unit != currentEntityHit && unit != null && unit.BlockOccupiedNodes)
		{
			return origin;
		}
		return customGridNodeBase;
	}

	public static bool HasLos(CustomGridNodeBase origin, IntRect originSize, CustomGridNodeBase end, IntRect endSize)
	{
		NodeList borderNodes = GridAreaHelper.GetBorderNodes(origin, originSize, origin.CoordinatesInGrid.SimplifiedDirection(end.CoordinatesInGrid));
		NodeList borderNodes2 = GridAreaHelper.GetBorderNodes(end, endSize);
		Obstacle obstacle;
		return HasLos(borderNodes, borderNodes2, out obstacle);
	}

	public static bool HasLos(MechanicEntity origin, MechanicEntity target)
	{
		Obstacle obstacle;
		return HasLos(origin.GetOccupiedNodes(), target.GetOccupiedNodes(), out obstacle);
	}

	private static bool HasLos(NodeList from, NodeList to, out Obstacle obstacle)
	{
		return HasLosInternal(from, to, isMelee: false, out obstacle);
	}

	private static bool HasLosInternal(NodeList from, NodeList to, bool isMelee, out Obstacle obstacle)
	{
		obstacle = default(Obstacle);
		foreach (CustomGridNodeBase item in from)
		{
			foreach (CustomGridNodeBase item2 in to)
			{
				if (GetDirectLoSInternal(item, item2, isMelee, out obstacle))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool GetDirectLoSInternal([NotNull] CustomGridNodeBase from, [NotNull] CustomGridNodeBase to, bool isMelee, out Obstacle obstacle)
	{
		CustomGridGraph customGridGraph = (CustomGridGraph)from.Graph;
		CustomGridGraph customGridGraph2 = (CustomGridGraph)to.Graph;
		if (customGridGraph != customGridGraph2)
		{
			obstacle = default(Obstacle);
			return false;
		}
		if (isMelee && Math.Abs(from.Vector3Position.y - to.Vector3Position.y) > 1.Cells().Meters)
		{
			obstacle = default(Obstacle);
			return false;
		}
		float num = customGridGraph.nodeSize * 0.99f;
		Int2 orthoAxis = GetOrthoAxis(from.position, to.position);
		Vector3 vector = new Vector3((float)orthoAxis.x * num * 0.5f, 0f, (float)orthoAxis.y * num * 0.5f);
		if (GetOneLineDirectLoSInternal(from, to, vector, out obstacle))
		{
			return true;
		}
		return GetOneLineDirectLoSInternal(from, to, -vector, out obstacle);
	}

	private static bool GetOneLineDirectLoSInternal(CustomGridNodeBase from, CustomGridNodeBase to, Vector3 dstOffset, out Obstacle obstacle)
	{
		return GetOneLineDirectLoSInternalWithNodeHeights(from, to, dstOffset, out obstacle, null);
	}

	public static bool GetOneLineDirectLosWithNodeHeights(CustomGridNodeBase from, CustomGridNodeBase to, Vector3 dstOffset, [CanBeNull] List<DistanceHeight> nodeHeights)
	{
		Obstacle obstacle;
		return GetOneLineDirectLoSInternalWithNodeHeights(from, to, dstOffset, out obstacle, nodeHeights);
	}

	private static bool GetOneLineDirectLoSInternalWithNodeHeights(CustomGridNodeBase from, CustomGridNodeBase to, Vector3 dstOffset, out Obstacle obstacle, [CanBeNull] List<DistanceHeight> nodeHeights)
	{
		try
		{
			if (nodeHeights == null)
			{
				nodeHeights = DstHeights.Value;
			}
			int count = nodeHeights.Count;
			HeightAccumulatorVisitor condition = new HeightAccumulatorVisitor(((Vector3)from.position).y, nodeHeights);
			if (Linecast.LinecastGrid((CustomGridGraph)from.Graph, (Vector3)from.position, (Vector3)to.position + dstOffset, from, out var hit, null, ref condition))
			{
				obstacle = new Obstacle((CustomGridNodeBase)hit.node);
				return false;
			}
			if (nodeHeights.Count - count == 0)
			{
				obstacle = default(Obstacle);
				return true;
			}
			Vector3 vector = (Vector3)from.position + EyeShift;
			Vector3 vector2 = (Vector3)to.position + EyeShift + dstOffset;
			List<DistanceHeight> list = nodeHeights;
			vector2.y = list[list.Count - 1].Height + EyeShift.y;
			float y = vector.y;
			float y2 = vector2.y;
			float num = ((Vector3)from.position).y;
			CustomGridNodeBase node = from;
			for (int i = count; i < nodeHeights.Count; i++)
			{
				DistanceHeight distanceHeight = nodeHeights[i];
				float num2 = Mathf.Lerp(y, y2, distanceHeight.Factor);
				if (num > num2 || distanceHeight.Height > num2)
				{
					obstacle = new Obstacle(node, distanceHeight.Node);
					return false;
				}
				num = distanceHeight.Height;
				node = distanceHeight.Node;
			}
			obstacle = default(Obstacle);
			return true;
		}
		finally
		{
			DstHeights.Value.Clear();
		}
	}

	public static bool GetDirectLos(Vector3 origin, Vector3 end)
	{
		CustomGridNodeBase nearestNodeXZUnwalkable = origin.GetNearestNodeXZUnwalkable();
		CustomGridNodeBase nearestNodeXZUnwalkable2 = end.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null || nearestNodeXZUnwalkable2 == null)
		{
			return false;
		}
		Obstacle obstacle;
		return GetDirectLoSInternal(nearestNodeXZUnwalkable, nearestNodeXZUnwalkable2, isMelee: false, out obstacle);
	}

	public static (CustomGridNodeBase left, CustomGridNodeBase right) GetOrthoNeighbours(CustomGridNodeBase origin, Vector3 dir)
	{
		(int left, int right) orthoNeighboursIdx = GetOrthoNeighboursIdx(dir);
		int item = orthoNeighboursIdx.left;
		int item2 = orthoNeighboursIdx.right;
		CustomGridNodeBase neighbourAlongDirection = origin.GetNeighbourAlongDirection(item);
		CustomGridNodeBase neighbourAlongDirection2 = origin.GetNeighbourAlongDirection(item2);
		return (left: neighbourAlongDirection, right: neighbourAlongDirection2);
	}

	public static CoverType GetWarhammerLos(MechanicEntity from, Vector3 fromPosition, MechanicEntity to)
	{
		return GetWarhammerLos(fromPosition, from.SizeRect, to.Position, to.SizeRect);
	}

	private static (int left, int right) GetOrthoNeighboursIdx(Vector3 origin, Vector3 end)
	{
		return GetOrthoNeighboursIdx(end - origin);
	}

	private static (int left, int right) GetOrthoNeighboursIdx(Vector3 dir)
	{
		if (!(Mathf.Abs(dir.x) > Mathf.Abs(dir.z)))
		{
			return (left: 1, right: 3);
		}
		return (left: 2, right: 0);
	}

	private static Int2 GetOrthoAxis(Int3 origin, Int3 end)
	{
		Int3 @int = end - origin;
		if (Mathf.Abs(@int.x) <= Mathf.Abs(@int.z))
		{
			return new Int2(1, 0);
		}
		return new Int2(0, 1);
	}

	private static (int x, int y) GetTangentNeighboursIdx(Int2 origin, Int2 end)
	{
		Int2 @int = origin - end;
		int item = ((@int.x > 0) ? 1 : ((@int.x >= 0) ? (-1) : 3));
		int item2 = ((@int.y > 0) ? 2 : ((@int.y >= 0) ? (-1) : 0));
		return (x: item, y: item2);
	}

	public static CoverType GetCoverType(Vector3 position)
	{
		return GetCoverType((CustomGridNode)(GraphNode)ObstacleAnalyzer.GetNearestNode(position));
	}

	public static CoverType GetCoverType(CustomGridNode node)
	{
		CoverType result = CoverType.None;
		for (int i = 0; i < 4; i++)
		{
			LosDescription cellCoverStatus = GetCellCoverStatus(node, i);
			if ((CoverType)cellCoverStatus == CoverType.Half)
			{
				return cellCoverStatus;
			}
			if ((CoverType)cellCoverStatus == CoverType.Full)
			{
				result = cellCoverStatus;
			}
		}
		return result;
	}

	public static LosDescription GetWarhammerLos(CustomGridNodeBase origin, IntRect originSize, CustomGridNodeBase end, IntRect endSize)
	{
		NodeList borderNodes = GridAreaHelper.GetBorderNodes(origin, originSize, origin.CoordinatesInGrid.SimplifiedDirection(end.CoordinatesInGrid));
		NodeList borderNodes2 = GridAreaHelper.GetBorderNodes(end, endSize);
		return GetWarhammerLos(borderNodes, borderNodes2);
	}

	public static LosDescription GetWarhammerLos(NodeList from, NodeList to)
	{
		using (ProfileScope.New("LosCalculations.GetWarhammerLos"))
		{
			if (!HasLos(from, to, out var obstacle))
			{
				return new LosDescription(CoverType.Invisible, obstacle);
			}
			if (to.NonSingle())
			{
				return new LosDescription(CoverType.None);
			}
			CustomGridNodeBase customGridNodeBase = from.First();
			return GetEffectiveCover(new Int2(customGridNodeBase.XCoordinateInGrid, customGridNodeBase.ZCoordinateInGrid), to.First());
		}
	}

	public static LosDescription GetWarhammerLos(MechanicEntity entity, CustomGridNodeBase endNode, IntRect endSize)
	{
		NodeList nodes = GridAreaHelper.GetNodes(endNode, endSize);
		return GetWarhammerLos(entity.GetOccupiedNodes(), nodes);
	}

	public static LosDescription GetWarhammerLos(MechanicEntity from, MechanicEntity to)
	{
		return GetWarhammerLos(from.GetOccupiedNodes(), to.GetOccupiedNodes());
	}

	public static LosDescription GetWarhammerLos(Vector3 origin, IntRect originSize, Vector3 end, IntRect endSize)
	{
		CustomGridNodeBase nearestNodeXZUnwalkable = origin.GetNearestNodeXZUnwalkable();
		CustomGridNodeBase nearestNodeXZUnwalkable2 = end.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null || nearestNodeXZUnwalkable2 == null)
		{
			return new LosDescription(CoverType.Invisible);
		}
		NodeList nodes = GridAreaHelper.GetNodes(origin, originSize);
		NodeList nodes2 = GridAreaHelper.GetNodes(end, endSize);
		return GetWarhammerLos(nodes, nodes2);
	}

	public static LosDescription GetWarhammerLos(MechanicEntity entity, Vector3 target, IntRect targetSize)
	{
		NodeList occupiedNodes = entity.GetOccupiedNodes();
		NodeList nodes = GridAreaHelper.GetNodes(target.GetNearestNodeXZUnwalkable(), targetSize);
		return GetWarhammerLos(occupiedNodes, nodes);
	}

	public static LosDescription GetWarhammerLos(Vector3 origin, IntRect fromSize, MechanicEntity target)
	{
		return GetWarhammerLos(GridAreaHelper.GetNodes(origin.GetNearestNodeXZUnwalkable(), fromSize), target.GetOccupiedNodes());
	}

	public static bool HasMeleeLos(this MechanicEntity from, MechanicEntity to)
	{
		Obstacle obstacle;
		return HasLosInternal(from.GetOccupiedNodes(), to.GetOccupiedNodes(), isMelee: true, out obstacle);
	}

	public static bool HasMeleeLos(Vector3 origin, IntRect originSize, Vector3 end, IntRect endSize)
	{
		NodeList nodes = GridAreaHelper.GetNodes(origin.GetNearestNodeXZUnwalkable(), originSize);
		NodeList nodes2 = GridAreaHelper.GetNodes(end.GetNearestNodeXZUnwalkable(), endSize);
		Obstacle obstacle;
		return HasLosInternal(nodes, nodes2, isMelee: true, out obstacle);
	}

	public static bool HasMeleeLos([NotNull] this CustomGridNodeBase from, [NotNull] CustomGridNodeBase to)
	{
		Obstacle obstacle;
		return GetDirectLoSInternal(from, to, isMelee: true, out obstacle);
	}

	public static bool HasMeleeLos(CustomGridNodeBase origin, IntRect originSize, CustomGridNodeBase end, IntRect endSize)
	{
		NodeList nodes = GridAreaHelper.GetNodes(origin, originSize);
		NodeList nodes2 = GridAreaHelper.GetNodes(end, endSize);
		Obstacle obstacle;
		return HasLosInternal(nodes, nodes2, isMelee: true, out obstacle);
	}
}

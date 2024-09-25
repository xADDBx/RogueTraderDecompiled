using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerPathChargeMetricCostProvider : ITraversalCostProvider<WarhammerPathChargeMetric, WarhammerPathChargeCell>
{
	private const float InvalidLength = 1000f;

	[NotNull]
	private readonly BaseUnitEntity m_Unit;

	[NotNull]
	private readonly CustomGridNodeBase m_OriginNode;

	[NotNull]
	private readonly CustomGridNodeBase m_TargetNode;

	[CanBeNull]
	private readonly MechanicEntity m_TargetEntity;

	public WarhammerPathChargeMetricCostProvider([NotNull] BaseUnitEntity unit, CustomGridNodeBase origin, [NotNull] CustomGridNodeBase targetNode, [CanBeNull] MechanicEntity targetEntity)
	{
		m_Unit = unit;
		m_OriginNode = origin;
		m_TargetNode = targetNode;
		m_TargetEntity = targetEntity;
	}

	public WarhammerPathChargeMetric Calc(in WarhammerPathChargeMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		bool flag = PathExtras.IsDiagonal((CustomGridNodeBase)to, (CustomGridNodeBase)from);
		int diagonalsCount = distanceFrom.DiagonalsCount + (flag ? 1 : 0);
		return new WarhammerPathChargeMetric(distanceFrom.Length + Calc(from, to), diagonalsCount);
	}

	private float Calc(GraphNode from, GraphNode to)
	{
		Vector2 vector = (m_TargetNode.Vector3Position - from.Vector3Position).To2D();
		Vector2 vector2 = (m_TargetNode.Vector3Position - to.Vector3Position).To2D();
		if (Math.Abs(vector.x) < Math.Abs(vector2.x) || Math.Abs(vector.y) < Math.Abs(vector2.y))
		{
			return 1000f;
		}
		Vector3 vector3Position = m_OriginNode.Vector3Position;
		Vector3 vector3Position2 = m_TargetNode.Vector3Position;
		Vector3 vector3Position3 = to.Vector3Position;
		if ((double)Math.Abs((vector3Position2.x - vector3Position.x) * (vector3Position.z - vector3Position3.z) - (vector3Position.x - vector3Position3.x) * (vector3Position2.z - vector3Position.z)) / Math.Sqrt((vector3Position2.x - vector3Position.x) * (vector3Position2.x - vector3Position.x) + (vector3Position2.z - vector3Position.z) * (vector3Position2.z - vector3Position.z)) > (double)(Mathf.Sqrt(2f) * 1.Cells().Meters * 1.1f))
		{
			return 1000f;
		}
		return 1f;
	}

	public WarhammerPathChargeCell Convert(in WarhammerPathChargeMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider)
	{
		return new WarhammerPathChargeCell(node.Vector3Position, distance.DiagonalsCount, distance.Length, node, parentNode, traversalProvider.CanTraverseEndNode(node, 0));
	}

	public int Compare(in WarhammerPathChargeMetric lengthA, in GraphNode nodeA, in WarhammerPathChargeMetric lengthB, in GraphNode nodeB)
	{
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)nodeA;
		CustomGridNodeBase customGridNodeBase2 = (CustomGridNodeBase)nodeB;
		Vector2Int from = new Vector2Int(customGridNodeBase.XCoordinateInGrid, customGridNodeBase.ZCoordinateInGrid);
		Vector2Int from2 = new Vector2Int(customGridNodeBase2.XCoordinateInGrid, customGridNodeBase2.ZCoordinateInGrid);
		Vector2Int to = new Vector2Int(m_TargetNode.XCoordinateInGrid, m_TargetNode.ZCoordinateInGrid);
		float x = WarhammerGeometryUtils.DistanceToInCells(from, default(IntRect), to, default(IntRect));
		float y = WarhammerGeometryUtils.DistanceToInCells(from2, default(IntRect), to, default(IntRect));
		return Comparer<float>.Default.Compare(x, y);
	}

	public bool IsWithinRange(in WarhammerPathChargeMetric node)
	{
		return node.Length < 1000f;
	}

	public bool IsTargetNode(in WarhammerPathChargeMetric distance, in GraphNode node)
	{
		if (node == m_TargetNode || (m_TargetEntity != null && WarhammerGeometryUtils.DistanceToInCells(node.Vector3Position, m_Unit.SizeRect, m_TargetNode.Vector3Position, default(IntRect)) < 2 && ((CustomGridNodeBase)node).HasMeleeLos(m_TargetNode)))
		{
			return m_Unit.CanStandHere(node);
		}
		return false;
	}

	int ITraversalCostProvider<WarhammerPathChargeMetric, WarhammerPathChargeCell>.Compare(in WarhammerPathChargeMetric lengthA, in GraphNode nodeA, in WarhammerPathChargeMetric lengthB, in GraphNode nodeB)
	{
		return Compare(in lengthA, in nodeA, in lengthB, in nodeB);
	}

	WarhammerPathChargeMetric ITraversalCostProvider<WarhammerPathChargeMetric, WarhammerPathChargeCell>.Calc(in WarhammerPathChargeMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		return Calc(in distanceFrom, in from, in to);
	}

	WarhammerPathChargeCell ITraversalCostProvider<WarhammerPathChargeMetric, WarhammerPathChargeCell>.Convert(in WarhammerPathChargeMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider)
	{
		return Convert(in distance, in node, in parentNode, in traversalProvider);
	}

	bool ITraversalCostProvider<WarhammerPathChargeMetric, WarhammerPathChargeCell>.IsWithinRange(in WarhammerPathChargeMetric node)
	{
		return IsWithinRange(in node);
	}

	bool ITraversalCostProvider<WarhammerPathChargeMetric, WarhammerPathChargeCell>.IsTargetNode(in WarhammerPathChargeMetric distance, in GraphNode node)
	{
		return IsTargetNode(in distance, in node);
	}
}

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerPathPlayerMetricCostProvider : ITraversalCostProvider<WarhammerPathPlayerMetric, WarhammerPathPlayerCell>
{
	private enum CellType
	{
		Normal,
		ThreateningArea
	}

	private readonly AbstractUnitEntity m_Unit;

	private readonly ICollection<GraphNode> m_ThreateningAreaCells;

	private readonly Dictionary<GraphNode, float> m_OverrideCosts;

	private readonly float m_MaxLength;

	[CanBeNull]
	private readonly CustomGridNodeBase m_TargetNode;

	[CanBeNull]
	private readonly MechanicEntity m_TargetEntity;

	public WarhammerPathPlayerMetricCostProvider(AbstractUnitEntity unit, float maxLength, [CanBeNull] CustomGridNodeBase targetNode, [CanBeNull] MechanicEntity targetEntity, ICollection<GraphNode> threateningAreaCells, Dictionary<GraphNode, float> overrideCosts)
	{
		m_Unit = unit;
		m_ThreateningAreaCells = threateningAreaCells;
		m_MaxLength = maxLength;
		m_TargetNode = targetNode ?? targetEntity?.CurrentUnwalkableNode;
		m_TargetEntity = targetEntity;
		m_OverrideCosts = overrideCosts;
	}

	public WarhammerPathPlayerMetric Calc(in WarhammerPathPlayerMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		bool flag = PathExtras.IsDiagonal((CustomGridNodeBase)to, (CustomGridNodeBase)from);
		float num = ((!(distanceFrom.DiagonalsCount % 2 == 1 && flag)) ? 1 : 2);
		float num2 = Calc(from, to);
		num *= num2;
		float length = distanceFrom.Length + num;
		return new WarhammerPathPlayerMetric(distanceFrom.DiagonalsCount + ((flag && num > float.Epsilon) ? 1 : 0), length);
	}

	private float Calc(GraphNode from, GraphNode to)
	{
		if (!m_OverrideCosts.TryGetValue(to, out var value))
		{
			value = GetCellCost(GetCellType(from));
		}
		if (NodeLinksExtensions.AreConnected(from, to, out var currentLink))
		{
			return value * currentLink.CostFactor;
		}
		return value;
	}

	private CellType GetCellType(GraphNode cell)
	{
		if ((bool)m_Unit.Features.IgnoreThreateningAreaForMovementCostCalculation || !m_ThreateningAreaCells.Contains(cell))
		{
			return CellType.Normal;
		}
		return CellType.ThreateningArea;
	}

	private float GetCellCost(CellType cellType)
	{
		return cellType switch
		{
			CellType.Normal => m_Unit.Blueprint.WarhammerMovementApPerCell, 
			CellType.ThreateningArea => m_Unit.GetWarhammerMovementApPerCellThreateningArea(), 
			_ => throw new ArgumentOutOfRangeException("cellType", cellType, null), 
		};
	}

	public WarhammerPathPlayerCell Convert(in WarhammerPathPlayerMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider)
	{
		return new WarhammerPathPlayerCell(node.Vector3Position, distance.DiagonalsCount, distance.Length, node, parentNode, traversalProvider.CanTraverseEndNode(node, 0));
	}

	public int Compare(in WarhammerPathPlayerMetric lengthA, in GraphNode nodeA, in WarhammerPathPlayerMetric lengthB, in GraphNode nodeB)
	{
		if (m_TargetNode == null)
		{
			return Comparer<float>.Default.Compare(lengthA.Length, lengthB.Length);
		}
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)nodeA;
		CustomGridNodeBase customGridNodeBase2 = (CustomGridNodeBase)nodeB;
		Vector2Int from = new Vector2Int(customGridNodeBase.XCoordinateInGrid, customGridNodeBase.ZCoordinateInGrid);
		Vector2Int from2 = new Vector2Int(customGridNodeBase2.XCoordinateInGrid, customGridNodeBase2.ZCoordinateInGrid);
		Vector2Int to = new Vector2Int(m_TargetNode.XCoordinateInGrid, m_TargetNode.ZCoordinateInGrid);
		float num = lengthA.Length + (float)WarhammerGeometryUtils.DistanceToInCells(from, default(IntRect), to, default(IntRect)) * m_Unit.Blueprint.WarhammerMovementApPerCell;
		float num2 = lengthB.Length + (float)WarhammerGeometryUtils.DistanceToInCells(from2, default(IntRect), to, default(IntRect)) * m_Unit.Blueprint.WarhammerMovementApPerCell;
		if (!(Math.Abs(num - num2) < 0.001f))
		{
			return Comparer<float>.Default.Compare(num, num2);
		}
		return Comparer<int>.Default.Compare(lengthB.DiagonalsCount, lengthA.DiagonalsCount);
	}

	public bool IsWithinRange(in WarhammerPathPlayerMetric node)
	{
		if (!(m_MaxLength < 0f))
		{
			return node.Length <= m_MaxLength;
		}
		return true;
	}

	public bool IsTargetNode(in WarhammerPathPlayerMetric distance, in GraphNode node)
	{
		if (node == m_TargetNode || (m_TargetEntity != null && WarhammerGeometryUtils.DistanceToInCells(node.Vector3Position, m_Unit.SizeRect, m_TargetNode.Vector3Position, default(IntRect)) < 2 && ((CustomGridNodeBase)node).HasMeleeLos(m_TargetNode)))
		{
			return m_Unit.CanStandHere(node);
		}
		return false;
	}

	int ITraversalCostProvider<WarhammerPathPlayerMetric, WarhammerPathPlayerCell>.Compare(in WarhammerPathPlayerMetric lengthA, in GraphNode nodeA, in WarhammerPathPlayerMetric lengthB, in GraphNode nodeB)
	{
		return Compare(in lengthA, in nodeA, in lengthB, in nodeB);
	}

	WarhammerPathPlayerMetric ITraversalCostProvider<WarhammerPathPlayerMetric, WarhammerPathPlayerCell>.Calc(in WarhammerPathPlayerMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		return Calc(in distanceFrom, in from, in to);
	}

	WarhammerPathPlayerCell ITraversalCostProvider<WarhammerPathPlayerMetric, WarhammerPathPlayerCell>.Convert(in WarhammerPathPlayerMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider)
	{
		return Convert(in distance, in node, in parentNode, in traversalProvider);
	}

	bool ITraversalCostProvider<WarhammerPathPlayerMetric, WarhammerPathPlayerCell>.IsWithinRange(in WarhammerPathPlayerMetric node)
	{
		return IsWithinRange(in node);
	}

	bool ITraversalCostProvider<WarhammerPathPlayerMetric, WarhammerPathPlayerCell>.IsTargetNode(in WarhammerPathPlayerMetric distance, in GraphNode node)
	{
		return IsTargetNode(in distance, in node);
	}
}

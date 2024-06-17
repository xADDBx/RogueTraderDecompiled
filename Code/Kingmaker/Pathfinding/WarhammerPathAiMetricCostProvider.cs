using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerPathAiMetricCostProvider : ITraversalCostProvider<WarhammerPathAiMetric, WarhammerPathAiCell>
{
	private enum CellType
	{
		Normal,
		ThreateningArea
	}

	private readonly AbstractUnitEntity m_Unit;

	private readonly IReadOnlyDictionary<GraphNode, AiBrainHelper.ThreatsInfo> m_ThreateningAreaCells;

	private readonly int m_MaxLength;

	private static readonly AiBrainHelper.ThreatsInfo DefaultThreatsInfo = new AiBrainHelper.ThreatsInfo();

	public WarhammerPathAiMetricCostProvider(AbstractUnitEntity unit, int maxLength, IReadOnlyDictionary<GraphNode, AiBrainHelper.ThreatsInfo> threateningAreaCells)
	{
		m_Unit = unit;
		m_MaxLength = maxLength;
		m_ThreateningAreaCells = threateningAreaCells;
	}

	public WarhammerPathAiMetric Calc(in WarhammerPathAiMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		if (!m_ThreateningAreaCells.TryGetValue(from, out var value))
		{
			value = DefaultThreatsInfo;
		}
		if (!m_ThreateningAreaCells.TryGetValue(to, out var nextTd))
		{
			nextTd = DefaultThreatsInfo;
		}
		float num = 100f;
		int num2 = Mathf.Clamp(value.dmgOnMoveAes.Count, 0, 3);
		num += (float)num2 * 100f;
		int num3 = nextTd.aes.Count - value.aes.Count;
		int num4 = Mathf.Clamp(num3, -3, 3);
		num += ((num4 >= 0) ? (300f * (float)num4) : (33f * (float)num4));
		int num5 = value.aooUnits.Count((BaseUnitEntity un) => !nextTd.aooUnits.Contains(un)) + nextTd.overwatchUnits.Count();
		num += (float)num5 * 1000f;
		bool flag = PathExtras.IsDiagonal((CustomGridNodeBase)to, (CustomGridNodeBase)from);
		float num6 = ((!(distanceFrom.DiagonalsCount % 2 == 1 && flag)) ? 1 : 2);
		float num7 = Calc(from, to);
		num6 *= num7;
		float num8 = distanceFrom.Length + num6;
		float num9 = num8 - distanceFrom.Length;
		int enteredAoE = distanceFrom.EnteredAoE + Math.Max(0, num3);
		int stepsInsideDamagingAoE = distanceFrom.StepsInsideDamagingAoE + num2;
		int provokedAttacks = distanceFrom.ProvokedAttacks + num5;
		float length = num8;
		float delay = distanceFrom.Delay + num9 + (flag ? (0.1f * num7) : 0f) + num;
		return new WarhammerPathAiMetric(distanceFrom.DiagonalsCount + (flag ? 1 : 0), length, delay, enteredAoE, stepsInsideDamagingAoE, provokedAttacks);
	}

	private float Calc(GraphNode from, GraphNode to)
	{
		float cellCost = GetCellCost(GetCellType(from));
		if (NodeLinksExtensions.AreConnected(from, to, out var currentLink))
		{
			return cellCost * currentLink.CostFactor;
		}
		return cellCost;
	}

	private CellType GetCellType(GraphNode cell)
	{
		if ((bool)m_Unit.Features.IgnoreThreateningAreaForMovementCostCalculation || !m_ThreateningAreaCells.ContainsKey(cell))
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

	public WarhammerPathAiCell Convert(in WarhammerPathAiMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider)
	{
		return new WarhammerPathAiCell(node.Vector3Position, distance.DiagonalsCount, distance.Length, node, parentNode, traversalProvider.CanTraverseEndNode(node, 0), distance.EnteredAoE, distance.StepsInsideDamagingAoE, distance.ProvokedAttacks);
	}

	public int Compare(in WarhammerPathAiMetric lengthA, in GraphNode nodeA, in WarhammerPathAiMetric lengthB, in GraphNode nodeB)
	{
		return Comparer<float>.Default.Compare(lengthA.Delay, lengthB.Delay);
	}

	public bool IsWithinRange(in WarhammerPathAiMetric node)
	{
		return node.Length <= (float)m_MaxLength;
	}

	public bool IsTargetNode(in WarhammerPathAiMetric distance, in GraphNode node)
	{
		return false;
	}

	int ITraversalCostProvider<WarhammerPathAiMetric, WarhammerPathAiCell>.Compare(in WarhammerPathAiMetric lengthA, in GraphNode nodeA, in WarhammerPathAiMetric lengthB, in GraphNode nodeB)
	{
		return Compare(in lengthA, in nodeA, in lengthB, in nodeB);
	}

	WarhammerPathAiMetric ITraversalCostProvider<WarhammerPathAiMetric, WarhammerPathAiCell>.Calc(in WarhammerPathAiMetric distanceFrom, in GraphNode from, in GraphNode to)
	{
		return Calc(in distanceFrom, in from, in to);
	}

	WarhammerPathAiCell ITraversalCostProvider<WarhammerPathAiMetric, WarhammerPathAiCell>.Convert(in WarhammerPathAiMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider)
	{
		return Convert(in distance, in node, in parentNode, in traversalProvider);
	}

	bool ITraversalCostProvider<WarhammerPathAiMetric, WarhammerPathAiCell>.IsWithinRange(in WarhammerPathAiMetric node)
	{
		return IsWithinRange(in node);
	}

	bool ITraversalCostProvider<WarhammerPathAiMetric, WarhammerPathAiCell>.IsTargetNode(in WarhammerPathAiMetric distance, in GraphNode node)
	{
		return IsTargetNode(in distance, in node);
	}
}

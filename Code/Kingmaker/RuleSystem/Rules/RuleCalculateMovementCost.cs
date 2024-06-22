using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateMovementCost : RulebookEvent<BaseUnitEntity>
{
	private readonly Path m_PathFound;

	private readonly bool m_CalcFullPathAPCost;

	public int ResultPointCount;

	public float[] ResultAPCostPerPoint;

	public float ResultFullPathAPCost;

	public RuleCalculateMovementCost([NotNull] BaseUnitEntity initiator, Path path, bool calcFullPathApCost = false)
		: base(initiator)
	{
		m_PathFound = path;
		m_CalcFullPathAPCost = calcFullPathApCost;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (m_PathFound.path.Count < 2)
		{
			ResultPointCount = 0;
			ResultAPCostPerPoint = Array.Empty<float>();
			return;
		}
		int totalApInt = Mathf.RoundToInt(base.Initiator.GetCombatStateOptional()?.ActionPointsBlue ?? 0f);
		if (m_PathFound is WarhammerPathPlayer)
		{
			CalculateDirectly(totalApInt);
		}
		else
		{
			Calculate(totalApInt);
		}
	}

	private void CalculateDirectly(int totalApInt)
	{
		WarhammerPathPlayer warhammerPathPlayer = (WarhammerPathPlayer)m_PathFound;
		List<float> list = TempList.Get<float>();
		ref WarhammerPathPlayerCell reference = ref warhammerPathPlayer.CalculatedPath[0];
		list.Add(reference.Length);
		int i;
		for (i = 1; i < m_PathFound.path.Count; i++)
		{
			ref WarhammerPathPlayerCell reference2 = ref warhammerPathPlayer.CalculatedPath[i];
			if (reference2.Length > (float)totalApInt && !m_CalcFullPathAPCost)
			{
				break;
			}
			float item = reference2.Length - reference.Length;
			list.Add(item);
			reference = reference2;
		}
		ResultFullPathAPCost = reference.Length;
		ResultPointCount = i;
		ResultAPCostPerPoint = list.ToArray();
	}

	private void Calculate(int totalApInt)
	{
		HashSet<GraphNode> threateningAreaCells = UnitMovementAgentBase.CacheThreateningAreaCells(base.Initiator);
		Dictionary<GraphNode, float> overrideCosts = NodeTraverseCostHelper.GetOverrideCosts(base.Initiator);
		WarhammerPathPlayerMetricCostProvider warhammerPathPlayerMetricCostProvider = new WarhammerPathPlayerMetricCostProvider(base.Initiator, 0, null, null, threateningAreaCells, overrideCosts);
		List<float> list = TempList.Get<float>();
		WarhammerPathPlayerMetric distanceFrom = new WarhammerPathPlayerMetric(base.Initiator.CombatState.LastDiagonalCount, 0f);
		list.Add(distanceFrom.Length);
		int i;
		for (i = 1; i < m_PathFound.path.Count; i++)
		{
			GraphNode from = m_PathFound.path[i - 1];
			GraphNode to = m_PathFound.path[i];
			WarhammerPathPlayerMetric warhammerPathPlayerMetric = warhammerPathPlayerMetricCostProvider.Calc(in distanceFrom, in from, in to);
			if (warhammerPathPlayerMetric.Length > (float)totalApInt && !m_CalcFullPathAPCost)
			{
				break;
			}
			float item = warhammerPathPlayerMetric.Length - distanceFrom.Length;
			list.Add(item);
			distanceFrom = warhammerPathPlayerMetric;
		}
		ResultFullPathAPCost = distanceFrom.Length;
		ResultPointCount = i;
		ResultAPCostPerPoint = list.ToArray();
	}
}

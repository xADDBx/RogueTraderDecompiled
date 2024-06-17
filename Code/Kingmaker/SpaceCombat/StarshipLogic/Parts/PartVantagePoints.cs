using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Random;
using Newtonsoft.Json;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public class PartVantagePoints : StarshipPart, IHashable
{
	[JsonProperty]
	private List<Vector3> m_VantagePoints = new List<Vector3>();

	[JsonProperty]
	private bool m_IsInVantagePoint;

	public IEnumerable<Vector3> VantagePoints => m_VantagePoints;

	public bool IsInVantagePoint
	{
		get
		{
			return m_IsInVantagePoint;
		}
		set
		{
			if (m_IsInVantagePoint == value)
			{
				return;
			}
			m_IsInVantagePoint = value;
			if (m_IsInVantagePoint)
			{
				EventBus.RaiseEvent((IStarshipEntity)base.Owner, (Action<IStarshipVantagePointsHandler>)delegate(IStarshipVantagePointsHandler h)
				{
					h.HandleEnteredVantagePoint();
				}, isCheckRuntime: true);
			}
			else
			{
				EventBus.RaiseEvent((IStarshipEntity)base.Owner, (Action<IStarshipVantagePointsHandler>)delegate(IStarshipVantagePointsHandler h)
				{
					h.HandleLeavedVantagePoint();
				}, isCheckRuntime: true);
			}
		}
	}

	public void UpdateIsInVantagePoint()
	{
		foreach (CustomGridNodeBase node in base.Owner.GetOccupiedNodes())
		{
			if (VantagePoints.Any((Vector3 p) => (node.Vector3Position - p).sqrMagnitude < 0.01f))
			{
				IsInVantagePoint = true;
				return;
			}
		}
		IsInVantagePoint = false;
	}

	public void DetectVantagePoints(int percentAmongReachableNodes)
	{
		ClearVantagePoints();
		GraphNode node = AstarPath.active.GetNearest(base.Owner.Position).node;
		Dictionary<GraphNode, ShipPath.PathCell> endNodes = base.Owner.Navigation.GetEndNodes();
		int num = endNodes.Count - 1;
		int num2 = num * percentAmongReachableNodes / 100;
		foreach (var (graphNode2, pathCell2) in endNodes)
		{
			if (num2 == 0)
			{
				break;
			}
			if (graphNode2 == node)
			{
				continue;
			}
			if (PFStatefulRandom.SpaceCombat.Range(0, num) < num2)
			{
				if (pathCell2.CanStand)
				{
					m_VantagePoints.Add(graphNode2.Vector3Position);
				}
				num2--;
			}
			num--;
		}
	}

	public void ClearVantagePoints()
	{
		m_VantagePoints.Clear();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<Vector3> vantagePoints = m_VantagePoints;
		if (vantagePoints != null)
		{
			for (int i = 0; i < vantagePoints.Count; i++)
			{
				Vector3 obj = vantagePoints[i];
				Hash128 val2 = UnmanagedHasher<Vector3>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		result.Append(ref m_IsInVantagePoint);
		return result;
	}
}

using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public sealed class WarhammerPathPlayerCache
{
	private readonly struct WarhammerPathPlayerEntry
	{
		public readonly WarhammerPathPlayer Path;

		public readonly int Tick;

		public readonly EntityRef AgentEntity;

		public readonly Vector3 Origin;

		public readonly Vector3 Destination;

		public readonly float MaxLength;

		public readonly bool IgnoreThreateningAreaCost;

		public readonly bool IgnoreBlockers;

		public WarhammerPathPlayerEntry(WarhammerPathPlayer path, int tick, EntityRef agentEntity, Vector3 origin, Vector3 destination, float maxLength, bool ignoreThreateningAreaCost, bool ignoreBlockers)
		{
			Path = path;
			Tick = tick;
			AgentEntity = agentEntity;
			Origin = origin;
			Destination = destination;
			MaxLength = maxLength;
			IgnoreThreateningAreaCost = ignoreThreateningAreaCost;
			IgnoreBlockers = ignoreBlockers;
		}

		public bool Equals(EntityRef agentEntity, Vector3 origin, Vector3 destination, float maxLength, bool ignoreThreateningAreaCost, bool ignoreBlockers)
		{
			if (AgentEntity == agentEntity && Origin == origin && Destination == destination && Mathf.Approximately(MaxLength, maxLength) && IgnoreThreateningAreaCost == ignoreThreateningAreaCost)
			{
				return IgnoreBlockers == ignoreBlockers;
			}
			return false;
		}
	}

	private const int RemoveDelayInTicks = 5;

	private readonly Queue<WarhammerPathPlayerEntry> m_Cache = new Queue<WarhammerPathPlayerEntry>();

	public WarhammerPathPlayer FindPathTB_Blocking(UnitMovementAgentBase agent, Vector3 origin, Vector3 destination, float maxLength, bool ignoreThreateningAreaCost = false, bool ignoreBlockers = false)
	{
		EntityRef agentEntity = (agent.Unit?.Data.Ref).Value;
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		while (0 < m_Cache.Count && m_Cache.Peek().Tick + 5 <= currentNetworkTick)
		{
			PathPool.Pool(m_Cache.Dequeue().Path);
		}
		foreach (WarhammerPathPlayerEntry item in m_Cache)
		{
			if (item.Equals(agentEntity, origin, destination, maxLength, ignoreThreateningAreaCost, ignoreBlockers))
			{
				return item.Path;
			}
		}
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(agent, origin, destination, maxLength, ignoreThreateningAreaCost, ignoreBlockers);
		if (5 <= m_Cache.Count)
		{
			PathPool.Pool(m_Cache.Dequeue().Path);
		}
		m_Cache.Enqueue(new WarhammerPathPlayerEntry(warhammerPathPlayer, currentNetworkTick, agentEntity, origin, destination, maxLength, ignoreThreateningAreaCost, ignoreBlockers));
		return warhammerPathPlayer;
	}
}

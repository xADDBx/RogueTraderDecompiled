using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerBlockManager : MonoBehaviour
{
	private readonly struct BlockedNodeInfo
	{
		public readonly HashSet<WarhammerSingleNodeBlocker> Enemies;

		public readonly HashSet<WarhammerSingleNodeBlocker> Friends;

		public readonly HashSet<WarhammerSingleNodeBlocker> Invisible;

		public bool HasVisible
		{
			get
			{
				if (Enemies.Count <= 0)
				{
					return Friends.Count > 0;
				}
				return true;
			}
		}

		public bool Empty
		{
			get
			{
				if (Enemies.Count == 0 && Friends.Count == 0)
				{
					return Invisible.Count == 0;
				}
				return false;
			}
		}

		public static BlockedNodeInfo Create()
		{
			return new BlockedNodeInfo(new HashSet<WarhammerSingleNodeBlocker>(), new HashSet<WarhammerSingleNodeBlocker>(), new HashSet<WarhammerSingleNodeBlocker>());
		}

		private BlockedNodeInfo(HashSet<WarhammerSingleNodeBlocker> enemies, HashSet<WarhammerSingleNodeBlocker> friends, HashSet<WarhammerSingleNodeBlocker> invisible)
		{
			Enemies = enemies;
			Friends = friends;
			Invisible = invisible;
		}
	}

	private int m_ChangeVersion = 1;

	private readonly Dictionary<GraphNode, BlockedNodeInfo> m_Blocked = new Dictionary<GraphNode, BlockedNodeInfo>();

	public static WarhammerBlockManager Instance { get; private set; }

	public int ChangeVersion => m_ChangeVersion;

	public void OnEnable()
	{
		if (Instance != null)
		{
			PFLog.System.Error("Trying to activate a WarhammerBlockManager when another one already exists. The old one will be disabled.");
			Instance.OnDisable();
		}
		Instance = this;
		AstarPath.OnGraphPreScan = (OnGraphDelegate)Delegate.Combine(AstarPath.OnGraphPreScan, new OnGraphDelegate(OnGraphPreScan));
	}

	private void OnGraphPreScan(NavGraph graph)
	{
		lock (m_Blocked)
		{
			m_ChangeVersion++;
			m_Blocked.Clear();
		}
	}

	public void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		AstarPath.OnGraphPreScan = (OnGraphDelegate)Delegate.Remove(AstarPath.OnGraphPreScan, new OnGraphDelegate(OnGraphPreScan));
	}

	public bool NodeContains(GraphNode node, WarhammerSingleNodeBlocker blocker, bool enemies)
	{
		lock (m_Blocked)
		{
			if (!m_Blocked.TryGetValue(node, out var value))
			{
				return false;
			}
			return (enemies ? value.Enemies : value.Friends).Contains(blocker);
		}
	}

	public bool NodeContainsInvisibleAnyExcept(GraphNode node, WarhammerSingleNodeBlocker exceptedBlocker)
	{
		lock (m_Blocked)
		{
			if (!m_Blocked.TryGetValue(node, out var value))
			{
				return false;
			}
			HashSet<WarhammerSingleNodeBlocker> invisible = value.Invisible;
			return invisible.Contains(exceptedBlocker) ? (invisible.Count >= 2) : (invisible.Count >= 1);
		}
	}

	public bool NodeContainsAnyExcept(GraphNode node, WarhammerSingleNodeBlocker blocker, bool enemies)
	{
		lock (m_Blocked)
		{
			if (!m_Blocked.TryGetValue(node, out var value))
			{
				return false;
			}
			HashSet<WarhammerSingleNodeBlocker> hashSet = (enemies ? value.Enemies : value.Friends);
			if (hashSet.Contains(blocker) ? (hashSet.Count >= 2) : (hashSet.Count >= 1))
			{
				foreach (WarhammerSingleNodeBlocker item in hashSet)
				{
					if (item != blocker && !item.IsBlockerSoftUnit)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool NodeContainsAnyExcept(GraphNode node, WarhammerSingleNodeBlocker blocker)
	{
		lock (m_Blocked)
		{
			if (!m_Blocked.TryGetValue(node, out var value))
			{
				return false;
			}
			if (value.Enemies.Contains(blocker))
			{
				if (value.Enemies.Count >= 2)
				{
					return true;
				}
			}
			else
			{
				if (value.Enemies.Count > 1)
				{
					return true;
				}
				if (value.Enemies.Count == 1 && !value.Enemies.First().IsSameUnit(blocker))
				{
					return true;
				}
			}
			if (value.Friends.Contains(blocker))
			{
				if (value.Friends.Count >= 2)
				{
					return true;
				}
			}
			else
			{
				if (value.Friends.Count > 1)
				{
					return true;
				}
				if (value.Friends.Count == 1 && !value.Friends.First().IsSameUnit(blocker))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool NodeContainsAnyExcept(IEnumerable<GraphNode> nodes, WarhammerSingleNodeBlocker blocker)
	{
		lock (m_Blocked)
		{
			foreach (GraphNode node in nodes)
			{
				if (NodeContainsAnyExcept(node, blocker))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool NodeContainsAny(GraphNode node)
	{
		lock (m_Blocked)
		{
			return m_Blocked.ContainsKey(node);
		}
	}

	public bool CanUnitStandOnNode(MechanicEntity unit, GraphNode targetNode)
	{
		NodeList occupiedNodes = unit.GetOccupiedNodes();
		foreach (CustomGridNodeBase node in GridAreaHelper.GetNodes(targetNode, unit.SizeRect, unit.Forward))
		{
			if (NodeContainsAny(node) && !occupiedNodes.Contains(node))
			{
				return false;
			}
		}
		if (unit.SizeRect.Height == unit.SizeRect.Width && !GridAreaHelper.AllNodesConnectedToNeighbours(unit.SizeRect, targetNode))
		{
			return false;
		}
		return true;
	}

	public bool CanUnitStandOnNode(IntRect sizeRect, GraphNode targetNode, WarhammerSingleNodeBlocker exceptBlocker = null)
	{
		foreach (CustomGridNodeBase node in GridAreaHelper.GetNodes(targetNode, sizeRect))
		{
			if (exceptBlocker == null)
			{
				if (NodeContainsAny(node))
				{
					return false;
				}
			}
			else if (NodeContainsAnyExcept(node, exceptBlocker))
			{
				return false;
			}
		}
		if (sizeRect.Height == sizeRect.Width && !GridAreaHelper.AllNodesConnectedToNeighbours(sizeRect, targetNode))
		{
			return false;
		}
		return true;
	}

	public NodeList GetBlockedNodes(GraphNode node, IntRect rect, Vector3 direction)
	{
		return GridAreaHelper.GetNodes(node, rect, direction);
	}

	public void InternalBlock(GraphNode node, WarhammerSingleNodeBlocker blocker, IntRect rect, Vector3 direction, BlockType blockType)
	{
		lock (m_Blocked)
		{
			m_ChangeVersion++;
			foreach (CustomGridNodeBase node2 in GridAreaHelper.GetNodes(node, rect, direction))
			{
				if (!m_Blocked.TryGetValue(node2, out var value))
				{
					BlockedNodeInfo blockedNodeInfo2 = (m_Blocked[node2] = BlockedNodeInfo.Create());
					value = blockedNodeInfo2;
				}
				(blockType switch
				{
					BlockType.Enemy => value.Enemies, 
					BlockType.Invisible => value.Invisible, 
					BlockType.Friend => value.Friends, 
					_ => throw new ArgumentOutOfRangeException("blockType", blockType, null), 
				}).Add(blocker);
			}
		}
	}

	public void InternalUnblock(GraphNode node, WarhammerSingleNodeBlocker blocker, IntRect rect, Vector3 direction)
	{
		lock (m_Blocked)
		{
			m_ChangeVersion++;
			foreach (CustomGridNodeBase node2 in GridAreaHelper.GetNodes(node, rect, direction))
			{
				if (!m_Blocked.TryGetValue(node2, out var value))
				{
					break;
				}
				if (!value.Enemies.Remove(blocker))
				{
					value.Enemies.RemoveWhere((WarhammerSingleNodeBlocker x) => blocker.OwnerUnit == x.OwnerUnit);
				}
				if (!value.Friends.Remove(blocker))
				{
					value.Friends.RemoveWhere((WarhammerSingleNodeBlocker x) => blocker.OwnerUnit == x.OwnerUnit);
				}
				if (!value.Invisible.Remove(blocker))
				{
					value.Invisible.RemoveWhere((WarhammerSingleNodeBlocker x) => blocker.OwnerUnit == x.OwnerUnit);
				}
				if (value.Empty)
				{
					m_Blocked.Remove(node2);
				}
			}
		}
	}
}

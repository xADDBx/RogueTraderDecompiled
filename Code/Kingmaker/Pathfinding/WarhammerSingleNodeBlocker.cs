using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_single_node_blocker.php")]
public class WarhammerSingleNodeBlocker
{
	private readonly MechanicEntity m_Owner;

	private GraphNode m_LastBlocked;

	private Vector3 m_LastDirection;

	public BaseUnitEntity OwnerUnit => m_Owner as BaseUnitEntity;

	public NodeList BlockedNodes => WarhammerBlockManager.Instance.GetBlockedNodes(m_LastBlocked, m_Owner.SizeRect, m_LastDirection);

	public bool IsBlocking => m_LastBlocked != null;

	public bool IsBlockerSoftUnit => (m_Owner as StarshipEntity)?.GetStarshipNavigationOptional()?.IsSoftUnit ?? false;

	public WarhammerSingleNodeBlocker(BaseUnitEntity owner)
	{
		m_Owner = owner;
	}

	public WarhammerSingleNodeBlocker(MechanicEntity owner)
	{
		m_Owner = owner;
	}

	public void BlockAtCurrentPosition()
	{
		BlockAt(m_Owner.Position);
	}

	public void BlockAt(Vector3 position)
	{
		Unblock();
		if ((bool)AstarPath.active)
		{
			GraphNode node = AstarPath.active.GetNearest(position, NNConstraint.None).node;
			if (node != null)
			{
				Block(node);
			}
		}
	}

	public void Block(GraphNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		IntRect sizeRect = m_Owner.SizeRect;
		Vector3 forward = m_Owner.Forward;
		BlockType blockType = BlockType.Enemy;
		if (OwnerUnit != null)
		{
			blockType = (OwnerUnit.IsInvisible ? BlockType.Invisible : (OwnerUnit.Faction.IsPlayerEnemy ? BlockType.Enemy : BlockType.Friend));
		}
		if (WarhammerBlockManager.Instance != null)
		{
			WarhammerBlockManager.Instance.InternalBlock(node, this, sizeRect, forward, blockType);
			m_LastBlocked = node;
			m_LastDirection = forward;
		}
		else
		{
			PFLog.System.Warning("Can't block node, BlockManager == null!");
		}
	}

	public void Unblock()
	{
		if (m_LastBlocked != null && (bool)WarhammerBlockManager.Instance)
		{
			IntRect sizeRect = m_Owner.SizeRect;
			WarhammerBlockManager.Instance.InternalUnblock(m_LastBlocked, this, sizeRect, m_LastDirection);
			m_LastBlocked = null;
		}
	}

	public bool IsSameUnit(WarhammerSingleNodeBlocker blocker)
	{
		return blocker.OwnerUnit == OwnerUnit;
	}
}

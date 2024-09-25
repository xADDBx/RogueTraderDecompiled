using System;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public class WarhammerTraversalProvider : IWarhammerTraversalProvider, ITraversalProvider
{
	private bool m_IsPlayersEnemy;

	private readonly IntRect m_SizeRect;

	private bool m_IsSoftUnit;

	private readonly WarhammerSingleNodeBlocker m_Executor;

	public WarhammerTraversalProvider(WarhammerSingleNodeBlocker executor, IntRect sizeRect, bool isPlayersEnemy, bool isSoftUnit = false)
	{
		m_Executor = executor ?? throw new ArgumentNullException("executor");
		m_IsPlayersEnemy = isPlayersEnemy;
		m_SizeRect = sizeRect;
		m_IsSoftUnit = isSoftUnit;
	}

	public bool CanTraverse(Path path, GraphNode node)
	{
		if (m_IsSoftUnit)
		{
			return true;
		}
		foreach (CustomGridNodeBase node2 in GridAreaHelper.GetNodes(node, m_SizeRect))
		{
			if (!CanTraverseSingleCell(path, node2, m_Executor, m_IsPlayersEnemy))
			{
				return false;
			}
		}
		return true;
	}

	private bool CanTraverse(Path path, GraphNode node, int direction)
	{
		if (m_IsSoftUnit)
		{
			return true;
		}
		foreach (CustomGridNodeBase node2 in GridAreaHelper.GetNodes(node, m_SizeRect, direction))
		{
			if (!CanTraverseSingleCell(path, node2, m_Executor, m_IsPlayersEnemy))
			{
				return false;
			}
		}
		if (m_SizeRect.Height == m_SizeRect.Width && !GridAreaHelper.AllNodesConnectedToNeighbours(m_SizeRect.ShiftRectByDirection(direction), node))
		{
			return false;
		}
		return true;
	}

	private static bool CanTraverseSingleCell(Path path, GraphNode node, WarhammerSingleNodeBlocker executor, bool enemy)
	{
		if (node == null || !node.Walkable || ((path.enabledTags >> (int)node.Tag) & 1) == 0)
		{
			return false;
		}
		if (path is ABPath aBPath && node == aBPath.endNode)
		{
			return !WarhammerBlockManager.Instance.NodeContainsAny(node);
		}
		if (!(path is IPathBlockModeOwner pathBlockModeOwner))
		{
			return true;
		}
		return pathBlockModeOwner.PathBlockMode switch
		{
			BlockMode.AllExceptSelector => !WarhammerBlockManager.Instance.NodeContainsAnyExcept(node, executor, !enemy), 
			BlockMode.OnlySelector => !WarhammerBlockManager.Instance.NodeContains(node, executor, enemy), 
			BlockMode.Ignore => true, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public uint GetTraversalCost(Path path, GraphNode node)
	{
		return path.GetTagPenalty((int)node.Tag) + node.Penalty;
	}

	public bool CanTraverseEndNode(GraphNode node, int direction)
	{
		if (m_IsSoftUnit)
		{
			return true;
		}
		foreach (CustomGridNodeBase node2 in GridAreaHelper.GetNodes(node, m_SizeRect, direction))
		{
			if (WarhammerBlockManager.Instance.NodeContainsAnyExcept(node2, m_Executor))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanTraverseAlongDirection(Path path, GraphNode node, int direction)
	{
		CustomGridNodeBase neighbourAlongDirection = ((CustomGridNodeBase)node).GetNeighbourAlongDirection(direction);
		if (!CanTraverse(path, neighbourAlongDirection, direction))
		{
			return false;
		}
		foreach (CustomGridNodeBase node2 in GridAreaHelper.GetNodes(node, m_SizeRect, direction))
		{
			if (node2.GetNeighbourAlongDirection(direction) == null)
			{
				return false;
			}
		}
		return true;
	}

	public void SetIsPlayerEnemy(bool enemy)
	{
		m_IsPlayersEnemy = enemy;
	}
}

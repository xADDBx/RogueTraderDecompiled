using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public sealed class CombatHubCollectionAreaSource : IAreaSource
{
	private readonly List<int> m_NodeIndices = new List<int>();

	public void Add(int nodeIndex)
	{
		m_NodeIndices.Add(nodeIndex);
	}

	public void Add([NotNull] CustomGridNodeBase node)
	{
		m_NodeIndices.Add(node.NodeInGridIndex);
	}

	public void AddRange([NotNull][ItemNotNull] IEnumerable<CustomGridNodeBase> nodes)
	{
		foreach (CustomGridNodeBase node in nodes)
		{
			m_NodeIndices.Add(node.NodeInGridIndex);
		}
	}

	public void AddRange([NotNull][ItemNotNull] HashSet<GraphNode> nodes)
	{
		foreach (GraphNode node in nodes)
		{
			if (node is CustomGridNodeBase customGridNodeBase)
			{
				m_NodeIndices.Add(customGridNodeBase.NodeInGridIndex);
			}
		}
	}

	public void AddRange([NotNull][ItemNotNull] List<GraphNode> nodes)
	{
		foreach (GraphNode node in nodes)
		{
			if (node is CustomGridNodeBase customGridNodeBase)
			{
				m_NodeIndices.Add(customGridNodeBase.NodeInGridIndex);
			}
		}
	}

	public void Clear()
	{
		m_NodeIndices.Clear();
	}

	public int EstimateCount()
	{
		return m_NodeIndices.Count;
	}

	public void GetCellIdentifiers<T>(Vector2Int gridDimensions, ref T container) where T : struct, IIdentifierContainer
	{
		int num = gridDimensions.x * gridDimensions.y;
		foreach (int nodeIndex in m_NodeIndices)
		{
			if (nodeIndex < num)
			{
				container.Push(nodeIndex);
			}
		}
	}
}

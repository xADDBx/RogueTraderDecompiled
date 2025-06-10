using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

public readonly struct OrientedPatternData
{
	public readonly struct NodesWithExtraDataEnumerable
	{
		private readonly NodeList m_NodeList;

		private readonly Dictionary<CustomGridNodeBase, PatternCellData> m_NodesExtraData;

		public NodesWithExtraDataEnumerable(NodeList nodeList, Dictionary<CustomGridNodeBase, PatternCellData> nodesExtraData)
		{
			m_NodeList = nodeList;
			m_NodesExtraData = nodesExtraData;
		}

		public NodesWithExtraDataEnumerator GetEnumerator()
		{
			return new NodesWithExtraDataEnumerator(m_NodeList.GetEnumerator(), m_NodesExtraData);
		}
	}

	public struct NodesWithExtraDataEnumerator
	{
		private NodeList.Enumerator m_NodesEnumerator;

		private readonly Dictionary<CustomGridNodeBase, PatternCellData> m_NodesExtraData;

		private (CustomGridNodeBase node, PatternCellData patternCellData) m_Current;

		public (CustomGridNodeBase node, PatternCellData patternCellData) Current => m_Current;

		public NodesWithExtraDataEnumerator(NodeList.Enumerator nodesEnumerator, Dictionary<CustomGridNodeBase, PatternCellData> nodesExtraData)
		{
			m_NodesEnumerator = nodesEnumerator;
			m_NodesExtraData = nodesExtraData;
			m_Current = default((CustomGridNodeBase, PatternCellData));
		}

		public bool MoveNext()
		{
			if (m_NodesEnumerator.MoveNext())
			{
				if (m_NodesExtraData != null && m_NodesExtraData.TryGetValue(m_NodesEnumerator.Current, out var value))
				{
					m_Current = (node: m_NodesEnumerator.Current, patternCellData: value);
				}
				else
				{
					m_Current = (node: m_NodesEnumerator.Current, patternCellData: PatternCellData.Empty);
				}
				return true;
			}
			return false;
		}
	}

	public static readonly OrientedPatternData Empty = new OrientedPatternData(new List<CustomGridNodeBase>(), null);

	[CanBeNull]
	private readonly Dictionary<CustomGridNodeBase, PatternCellData> m_NodesExtraData;

	[CanBeNull]
	public CustomGridNodeBase ApplicationNode { get; }

	public NodeList Nodes { get; }

	public NodesWithExtraDataEnumerable NodesWithExtraData => new NodesWithExtraDataEnumerable(Nodes, m_NodesExtraData);

	public bool IsEmpty => Nodes.IsEmpty;

	public OrientedPatternData([NotNull] Dictionary<CustomGridNodeBase, PatternCellDataAccumulator> nodes, [NotNull] CustomGridNodeBase applicationNode)
	{
		ApplicationNode = applicationNode;
		PatternGridData pattern = PatternGridData.Create(nodes.Keys.Select((CustomGridNodeBase v) => v.CoordinatesInGrid).ToTempHashSet(), disposable: true);
		Nodes = new NodeList((CustomGridGraph)applicationNode.Graph, in pattern);
		m_NodesExtraData = new Dictionary<CustomGridNodeBase, PatternCellData>(nodes.Count);
		foreach (KeyValuePair<CustomGridNodeBase, PatternCellDataAccumulator> node in nodes)
		{
			m_NodesExtraData.Add(node.Key, node.Value.Result);
		}
	}

	public OrientedPatternData(ReadonlyList<CustomGridNodeBase> nodes, CustomGridNodeBase applicationNode)
	{
		ApplicationNode = applicationNode;
		m_NodesExtraData = null;
		if (nodes.Count == 0)
		{
			Nodes = NodeList.Empty;
			return;
		}
		CustomGridGraph graph = (CustomGridGraph)nodes[0].Graph;
		PatternGridData pattern = PatternGridData.Create(nodes.Select((CustomGridNodeBase v) => v.CoordinatesInGrid).ToTempHashSet(), disposable: true);
		Nodes = new NodeList(graph, in pattern);
	}

	public OrientedPatternData([NotNull] HashSet<CustomGridNodeBase> nodes, CustomGridNodeBase applicationNode)
	{
		ApplicationNode = applicationNode;
		m_NodesExtraData = null;
		if (nodes.Count == 0)
		{
			Nodes = NodeList.Empty;
			return;
		}
		CustomGridGraph graph = (CustomGridGraph)nodes.First().Graph;
		PatternGridData pattern = PatternGridData.Create(nodes.Select((CustomGridNodeBase v) => v.CoordinatesInGrid).ToTempHashSet(), disposable: true);
		Nodes = new NodeList(graph, in pattern);
	}

	public OrientedPatternData(in NodeList nodes, CustomGridNodeBase applicationNode)
	{
		ApplicationNode = applicationNode;
		m_NodesExtraData = null;
		Nodes = nodes;
	}

	public bool Contains(CustomGridNodeBase node)
	{
		return Nodes.Contains(node);
	}

	public bool TryGet(CustomGridNodeBase node, out PatternCellData data)
	{
		if (!Nodes.Contains(node))
		{
			data = PatternCellData.Empty;
			return false;
		}
		if (m_NodesExtraData == null || !m_NodesExtraData.TryGetValue(node, out data))
		{
			data = PatternCellData.Empty;
		}
		return true;
	}
}

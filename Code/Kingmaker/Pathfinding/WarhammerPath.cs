using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using MemoryPack;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public abstract class WarhammerPath<TIntermediateMetric, TFinalMetric> : Path, ILinkTraversePath, IPathBlockModeOwner
{
	private readonly struct PathNode
	{
		public readonly CustomGridNodeBase Node;

		public readonly CustomGridNodeBase Parent;

		public readonly TIntermediateMetric Length;

		public PathNode(CustomGridNodeBase node, CustomGridNodeBase parent, TIntermediateMetric length)
		{
			Node = node;
			Parent = parent;
			Length = length;
		}

		public override string ToString()
		{
			if (Parent == null)
			{
				return $"(Start) ---{Length}---> ({Node.XCoordinateInGrid},{Node.ZCoordinateInGrid})";
			}
			return $"({Parent.XCoordinateInGrid},{Parent.ZCoordinateInGrid}) ---{Length}---> ({Node.XCoordinateInGrid},{Node.ZCoordinateInGrid})";
		}
	}

	private Dictionary<GraphNode, TFinalMetric> m_AllNodes;

	private Vector3 m_StartPoint;

	private ITraversalCostProvider<TIntermediateMetric, TFinalMetric> m_TraversalCostProvider;

	private TFinalMetric[] m_Path;

	private BlockMode m_BlockMode;

	private int m_SearchedNodes;

	private PathNode m_Initial;

	private PathNode m_Current;

	private PriorityQueue<PathNode> m_OpenNodes;

	private readonly Dictionary<int, PathNode> m_ClosedNodes = new Dictionary<int, PathNode>();

	[MemoryPackIgnore]
	public ILinkTraversalProvider LinkTraversalProvider { get; set; }

	public BlockMode PathBlockMode => m_BlockMode;

	[MemoryPackIgnore]
	public TFinalMetric[] CalculatedPath => m_Path;

	public Dictionary<GraphNode, TFinalMetric> GetAllNodesAndReset()
	{
		Dictionary<GraphNode, TFinalMetric> allNodes = m_AllNodes;
		m_AllNodes = null;
		return allNodes;
	}

	public void OverrideBlockMode(BlockMode unitBlockMode)
	{
		m_BlockMode = unitBlockMode;
	}

	private void FailWithError(string msg)
	{
		Error();
		PFLog.Default.Error("WarhammerPath: " + msg);
	}

	protected void Setup(Vector3 start, BlockMode blockMode, TIntermediateMetric initialLength, ITraversalCostProvider<TIntermediateMetric, TFinalMetric> traversalCostProvider, OnPathDelegate pathComplete)
	{
		callback = pathComplete;
		m_StartPoint = start;
		m_BlockMode = blockMode;
		m_Initial = new PathNode(null, null, initialLength);
		m_TraversalCostProvider = traversalCostProvider;
		Comparer<PathNode> comparer = Comparer<PathNode>.Create(delegate(PathNode a, PathNode b)
		{
			ITraversalCostProvider<TIntermediateMetric, TFinalMetric> traversalCostProvider2 = m_TraversalCostProvider;
			ref readonly TIntermediateMetric length = ref a.Length;
			GraphNode nodeA = a.Node;
			ref readonly TIntermediateMetric length2 = ref b.Length;
			GraphNode nodeB = b.Node;
			return traversalCostProvider2.Compare(in length, in nodeA, in length2, in nodeB);
		});
		m_OpenNodes = new PriorityQueue<PathNode>(comparer, EqualityComparer<PathNode>.Default);
	}

	protected override void Reset()
	{
		base.Reset();
		m_AllNodes?.Clear();
		m_StartPoint = Vector3.zero;
		heuristic = Heuristic.None;
		m_Initial = default(PathNode);
		m_OpenNodes = null;
		m_TraversalCostProvider = null;
		m_SearchedNodes = 0;
	}

	protected override void Prepare()
	{
		if (traversalProvider != null && !(traversalProvider is IWarhammerTraversalProvider))
		{
			FailWithError("Traversal Provider does not match the type WarhammerTraversalProvider");
			return;
		}
		nnConstraint.tags = enabledTags;
		CustomGridNode customGridNode = (CustomGridNode)AstarPath.active.GetNearest(m_StartPoint).node;
		if (customGridNode == null)
		{
			FailWithError("Could not find close node to the start point");
		}
		m_Initial = new PathNode(customGridNode, null, m_Initial.Length);
	}

	protected override void Initialize()
	{
		m_OpenNodes.Enqueue(m_Initial);
	}

	private TFinalMetric Convert(PathNode main)
	{
		ITraversalCostProvider<TIntermediateMetric, TFinalMetric> traversalCostProvider = m_TraversalCostProvider;
		ref readonly TIntermediateMetric length = ref main.Length;
		GraphNode node = main.Node;
		GraphNode parentNode = main.Parent;
		IWarhammerTraversalProvider warhammerTraversalProvider = traversalProvider as IWarhammerTraversalProvider;
		return traversalCostProvider.Convert(in length, in node, in parentNode, in warhammerTraversalProvider);
	}

	protected override void Cleanup()
	{
		m_AllNodes = m_ClosedNodes.Values.Select((PathNode v) => ((GraphNode Node, TFinalMetric Metric))(Node: v.Node, Metric: Convert(v))).ToDictionary(((GraphNode Node, TFinalMetric Metric) v) => v.Node, ((GraphNode Node, TFinalMetric Metric) v) => v.Metric);
		m_OpenNodes.Clear();
		m_ClosedNodes.Clear();
	}

	protected override void CalculateStep(long targetTick)
	{
		using (ProfileScope.NewScope("CalculateStep"))
		{
			int num = 0;
			do
			{
				if (m_OpenNodes.Count == 0)
				{
					TraceBack(in m_Current);
					base.CompleteState = PathCompleteState.Partial;
					return;
				}
				m_Current = m_OpenNodes.Dequeue();
				if (PlaceNode(m_Current))
				{
					ITraversalCostProvider<TIntermediateMetric, TFinalMetric> traversalCostProvider = m_TraversalCostProvider;
					ref readonly TIntermediateMetric length = ref m_Current.Length;
					GraphNode node = m_Current.Node;
					if (traversalCostProvider.IsTargetNode(in length, in node))
					{
						TraceBack(in m_Current);
						base.CompleteState = PathCompleteState.Complete;
						return;
					}
					Open(in m_Current);
				}
				if (num++ > 500)
				{
					if (DateTime.UtcNow.Ticks >= targetTick)
					{
						return;
					}
					num = 0;
				}
			}
			while (m_SearchedNodes++ <= 1000000);
			throw new Exception("Probable infinite loop. Over 1,000,000 nodes searched");
		}
	}

	private void TraceBack(in PathNode node)
	{
		List<PathNode> list = new List<PathNode>();
		PathNode item = node;
		while (item.Node != null)
		{
			list.Add(item);
			item = ((item.Parent != null) ? m_ClosedNodes[item.Parent.NodeIndex] : default(PathNode));
		}
		list.Reverse();
		path = ((IEnumerable<PathNode>)list).Select((Func<PathNode, GraphNode>)((PathNode v) => v.Node)).ToList();
		vectorPath = list.Select((PathNode v) => v.Node.Vector3Position).ToList();
		m_Path = list.Select(delegate(PathNode v)
		{
			ITraversalCostProvider<TIntermediateMetric, TFinalMetric> traversalCostProvider = m_TraversalCostProvider;
			ref readonly TIntermediateMetric length = ref v.Length;
			GraphNode node2 = v.Node;
			GraphNode parentNode = v.Parent;
			IWarhammerTraversalProvider warhammerTraversalProvider = (IWarhammerTraversalProvider)traversalProvider;
			return traversalCostProvider.Convert(in length, in node2, in parentNode, in warhammerTraversalProvider);
		}).ToArray();
	}

	private bool PlaceNode(PathNode newNode)
	{
		if (!m_ClosedNodes.TryGetValue(newNode.Node.NodeIndex, out var value))
		{
			m_ClosedNodes[newNode.Node.NodeIndex] = newNode;
			return true;
		}
		ITraversalCostProvider<TIntermediateMetric, TFinalMetric> traversalCostProvider = m_TraversalCostProvider;
		ref readonly TIntermediateMetric length = ref newNode.Length;
		GraphNode nodeA = newNode.Node;
		ref readonly TIntermediateMetric length2 = ref value.Length;
		GraphNode nodeB = value.Node;
		if (traversalCostProvider.Compare(in length, in nodeA, in length2, in nodeB) >= 0)
		{
			return false;
		}
		if (newNode.Parent == value.Parent)
		{
			throw new Exception("infinite loop in pathfinding");
		}
		using (ProfileScope.New("InvalidateRecursive"))
		{
			InvalidateRecursive(newNode.Node);
		}
		m_ClosedNodes[newNode.Node.NodeIndex] = newNode;
		return true;
	}

	private void Open(in PathNode pathNode)
	{
		using (ProfileScope.NewScope("Open"))
		{
			CustomGridNodeBase node = pathNode.Node;
			for (int i = 0; i < 8; i++)
			{
				CustomGridNodeBase neighbourAlongDirection = node.GetNeighbourAlongDirection(i);
				if (neighbourAlongDirection != null && ((IWarhammerTraversalProvider)traversalProvider).CanTraverseAlongDirection(this, node, i))
				{
					ProcessOtherNode(neighbourAlongDirection, in pathNode);
				}
			}
			if (!NodeLinksCache.AnyLinkActive || LinkTraversalProvider == null)
			{
				return;
			}
			CustomConnection[] connections = node.connections;
			if (connections == null || connections.Length <= 0)
			{
				return;
			}
			connections = node.connections;
			for (int j = 0; j < connections.Length; j++)
			{
				CustomConnection customConnection = connections[j];
				if (customConnection.Node is CustomGridNodeBase customGridNodeBase && LinkTraversalProvider.CanBuildPathThroughLink(node, customGridNodeBase, customConnection.Link))
				{
					ProcessOtherNode(customGridNodeBase, in pathNode);
				}
			}
		}
	}

	private void ProcessOtherNode(CustomGridNodeBase newNode, in PathNode parentNode)
	{
		ITraversalCostProvider<TIntermediateMetric, TFinalMetric> traversalCostProvider = m_TraversalCostProvider;
		ref readonly TIntermediateMetric length = ref parentNode.Length;
		GraphNode from = parentNode.Node;
		GraphNode to = newNode;
		TIntermediateMetric node = traversalCostProvider.Calc(in length, in from, in to);
		if (m_TraversalCostProvider.IsWithinRange(in node))
		{
			m_OpenNodes.Enqueue(new PathNode(newNode, parentNode.Node, node));
		}
	}

	private void InvalidateRecursive(CustomGridNodeBase node)
	{
		m_ClosedNodes.Remove(node.NodeIndex);
		while (true)
		{
			int num = m_OpenNodes.FindIndex((PathNode v) => v.Parent == node);
			if (num == -1)
			{
				break;
			}
			m_OpenNodes.RemoveAt(num);
		}
		for (int i = 0; i < 8; i++)
		{
			CustomGridNodeBase neighbourAlongDirection = node.GetNeighbourAlongDirection(i);
			if (neighbourAlongDirection != null && m_ClosedNodes.TryGetValue(neighbourAlongDirection.NodeIndex, out var value) && value.Parent == node)
			{
				InvalidateRecursive(neighbourAlongDirection);
			}
		}
		if (!NodeLinksCache.AnyLinkActive || LinkTraversalProvider == null)
		{
			return;
		}
		CustomConnection[] connections = node.connections;
		if (connections == null || connections.Length <= 0)
		{
			return;
		}
		connections = node.connections;
		for (int j = 0; j < connections.Length; j++)
		{
			if (connections[j].Node is CustomGridNodeBase customGridNodeBase && m_ClosedNodes.TryGetValue(customGridNodeBase.NodeIndex, out var value2) && value2.Parent == node)
			{
				InvalidateRecursive(customGridNodeBase);
			}
		}
	}
}

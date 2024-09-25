using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;
using Newtonsoft.Json;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AI.Scenarios;

public class HoldPositionScenario : AiScenario, IHashable
{
	private class NodeData
	{
		public readonly GraphNode node;

		public readonly int pathLenght;

		public readonly int diagonalCount;

		public static IComparer<NodeData> Comparer => Comparer<NodeData>.Create((NodeData a, NodeData b) => (a.pathLenght == b.pathLenght) ? (b.diagonalCount - a.diagonalCount) : (a.pathLenght - b.pathLenght));

		public NodeData(GraphNode node, int pathLenght, int diagonalCount)
		{
			this.node = node;
			this.pathLenght = pathLenght;
			this.diagonalCount = diagonalCount;
		}
	}

	[JsonProperty]
	public readonly TargetWrapper Target;

	[JsonProperty]
	public readonly int Range;

	[JsonProperty]
	public readonly bool CompleteOnTargetReached;

	public HoldPositionScenario(BaseUnitEntity owner, TargetWrapper target, int range, int idleRoundsCountLimit, bool completeOnTargetReached)
		: base(owner, idleRoundsCountLimit)
	{
		Target = target;
		Range = range;
		CompleteOnTargetReached = completeOnTargetReached;
	}

	public override bool ShouldComplete()
	{
		if (CompleteOnTargetReached && IsTargetReached())
		{
			return true;
		}
		return base.ShouldComplete();
	}

	private bool IsTargetReached()
	{
		GraphNode node = AstarPath.active.GetNearest(Owner.Position).node;
		return GetHoldPositionNodes().Contains(node);
	}

	public IEnumerable<GraphNode> GetHoldPositionNodes()
	{
		HashSet<GraphNode> hashSet = new HashSet<GraphNode>();
		PriorityQueue<NodeData> priorityQueue = new PriorityQueue<NodeData>(NodeData.Comparer, EqualityComparer<NodeData>.Default);
		priorityQueue.Enqueue(new NodeData(Target.NearestNode, 0, 0));
		bool flag = false;
		while (priorityQueue.Count > 0)
		{
			NodeData nodeData = priorityQueue.Dequeue();
			if (hashSet.Contains(nodeData.node) || nodeData.pathLenght > Range)
			{
				continue;
			}
			hashSet.Add(nodeData.node);
			if (nodeData.node.Walkable)
			{
				flag = true;
			}
			if (flag && !nodeData.node.Walkable)
			{
				continue;
			}
			CustomGridNodeBase customGridNodeBase = nodeData.node as CustomGridNodeBase;
			for (int i = 0; i < 8; i++)
			{
				CustomGridNodeBase neighbourAlongDirection = customGridNodeBase.GetNeighbourAlongDirection(i, flag);
				if (neighbourAlongDirection != null)
				{
					bool flag2 = i > 3;
					int num = nodeData.diagonalCount + (flag2 ? 1 : 0);
					int pathLenght = nodeData.pathLenght + 1 + ((flag2 && num % 2 == 0) ? 1 : 0);
					priorityQueue.Enqueue(new NodeData(neighbourAlongDirection, pathLenght, num));
				}
			}
		}
		if (hashSet.Count == 0)
		{
			yield break;
		}
		foreach (GraphNode item in hashSet)
		{
			if (item.Walkable)
			{
				yield return item;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<TargetWrapper>.GetHash128(Target);
		result.Append(ref val2);
		int val3 = Range;
		result.Append(ref val3);
		bool val4 = CompleteOnTargetReached;
		result.Append(ref val4);
		return result;
	}
}

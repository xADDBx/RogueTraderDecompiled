using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeFindPositionForRetreat : TaskNode
{
	protected override Status TickInternal(Blackboard blackboard)
	{
		AILogger.Instance.Log(AILogNode.Start(this));
		DecisionContext decisionContext = blackboard.DecisionContext;
		BaseUnitEntity unit = decisionContext.Unit;
		List<MechanicEntity> list = TempList.Get<MechanicEntity>();
		list.AddRange(decisionContext.GetEngagingEnemies());
		HashSet<GraphNode> engagedNodes = decisionContext.GetEngagedNodes();
		if (list.Count == 0)
		{
			AILogger.Instance.Log(new AILogReason(AILogReasonType.PositionForRetreatNotFound));
			return Status.Failure;
		}
		GraphNode graphNode = null;
		float num = GetAvgDistanceForNode(unit, unit.CurrentNode.node, engagedNodes, list);
		if (!decisionContext.UnitMoveVariants.IsZero)
		{
			foreach (GraphNode key in decisionContext.UnitMoveVariants.cells.Keys)
			{
				if (key != unit.CurrentNode.node)
				{
					float avgDistanceForNode = GetAvgDistanceForNode(unit, key, engagedNodes, list);
					if (num < avgDistanceForNode)
					{
						graphNode = key;
						num = avgDistanceForNode;
					}
				}
			}
			if (graphNode == null)
			{
				num = GetAvgDistanceForNode(unit, unit.CurrentNode.node, list);
				if (!decisionContext.UnitMoveVariants.IsZero)
				{
					foreach (GraphNode key2 in decisionContext.UnitMoveVariants.cells.Keys)
					{
						if (key2 != unit.CurrentNode.node)
						{
							float avgDistanceForNode2 = GetAvgDistanceForNode(unit, key2, list);
							if (num < avgDistanceForNode2)
							{
								graphNode = key2;
								num = avgDistanceForNode2;
							}
						}
					}
				}
			}
		}
		if (graphNode != null)
		{
			AILogger.Instance.Log(AILogPositionSearch.Found(AILogPositionSearch.PositionType.Retreat, graphNode));
			decisionContext.FoundBetterPlace = new DecisionContext.BetterPlace
			{
				PathData = decisionContext.UnitMoveVariants,
				BestCell = decisionContext.UnitMoveVariants.cells[graphNode]
			};
			return Status.Success;
		}
		AILogger.Instance.Log(new AILogReason(AILogReasonType.PositionForRetreatNotFound));
		return Status.Failure;
	}

	private float GetAvgDistanceForNode(BaseUnitEntity unit, GraphNode node, HashSet<GraphNode> engagedNodes, List<MechanicEntity> engagingEnemies)
	{
		if (unit.GetOccupiedNodes(node.Vector3Position).Any((CustomGridNodeBase n) => engagedNodes.Contains(n)))
		{
			return -1f;
		}
		return GetAvgDistanceForNode(unit, node, engagingEnemies);
	}

	private float GetAvgDistanceForNode(BaseUnitEntity unit, GraphNode node, List<MechanicEntity> engagingEnemies)
	{
		float distanceSum = 0f;
		engagingEnemies.ForEach(delegate(MechanicEntity enemy)
		{
			distanceSum += enemy.DistanceToInCells(node.Vector3Position, unit.SizeRect);
		});
		return distanceSum / (float)engagingEnemies.Count;
	}
}

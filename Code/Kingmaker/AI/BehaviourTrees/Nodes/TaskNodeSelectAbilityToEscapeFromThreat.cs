using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI.Blueprints.Components;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeSelectAbilityToEscapeFromThreat : TaskNode
{
	private static readonly Vector2Int[] DirectMovementOffsets = new Vector2Int[8]
	{
		new Vector2Int(-1, 0),
		new Vector2Int(0, 1),
		new Vector2Int(1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 1),
		new Vector2Int(1, 1),
		new Vector2Int(1, -1),
		new Vector2Int(-1, -1)
	};

	protected override Status TickInternal(Blackboard blackboard)
	{
		AILogger.Instance.Log(AILogAbility.SelectAbilityToEscapeThreat());
		DecisionContext decisionContext = blackboard.DecisionContext;
		BaseUnitEntity unit = decisionContext.Unit;
		foreach (AbilityData sortedEscapeAbility in GetSortedEscapeAbilities(unit))
		{
			if (HasAppropriateTarget(decisionContext, sortedEscapeAbility, out var target))
			{
				decisionContext.Ability = sortedEscapeAbility;
				decisionContext.AbilityTarget = target;
				AILogger.Instance.Log(AILogAbility.AbilitySelected(sortedEscapeAbility));
				return Status.Success;
			}
		}
		AILogger.Instance.Log(new AILogReason(AILogReasonType.AbilityToEscapeFromTreatNotFound));
		return Status.Failure;
	}

	private List<AbilityData> GetSortedEscapeAbilities(BaseUnitEntity unit)
	{
		List<AbilityData> list = TempList.Get<AbilityData>();
		unit.Abilities.RawFacts.ForEach(delegate(Ability ab)
		{
			if (ab.Data.HasEnoughActionPoint && ab.Data.IsAvailable && ab.Blueprint.GetComponent<AiEscapeFromThreat>() != null)
			{
				list.Add(ab.Data);
			}
		});
		list.Sort((AbilityData a, AbilityData b) => a.CalculateActionPointCost() - b.CalculateActionPointCost());
		return list;
	}

	private bool HasAppropriateTarget(DecisionContext context, AbilityData ability, out TargetWrapper target)
	{
		target = null;
		switch (ability.GetEscapeType())
		{
		case EscapeType.Absolute:
			target = new TargetWrapper(context.Unit);
			break;
		case EscapeType.Retreat:
			target = FindAppropriateRetreatPosition(context, ability);
			break;
		case EscapeType.PushAway:
			target = FindAppropriateTargetForPush(context, ability);
			break;
		}
		return target != null;
	}

	private TargetWrapper FindAppropriateRetreatPosition(DecisionContext context, AbilityData ability)
	{
		if (!ability.Blueprint.CanTargetPointAfterRestrictions(ability))
		{
			return null;
		}
		AbilityCustomDirectMovement component = ability.Blueprint.GetComponent<AbilityCustomDirectMovement>();
		if (component != null)
		{
			return FindAppropriateRetreatPositionForDirectMovement(context, ability, component);
		}
		return FindAppropriateRetreatPositionForFreeMovement(context, ability);
	}

	private TargetWrapper FindAppropriateRetreatPositionForDirectMovement(DecisionContext context, AbilityData ability, AbilityCustomDirectMovement directMovement)
	{
		int rangeCells = ability.RangeCells;
		CustomGridNodeBase casterNode = context.UnitNode;
		int xCoordinateInGrid = casterNode.XCoordinateInGrid;
		int zCoordinateInGrid = casterNode.ZCoordinateInGrid;
		CustomGridGraph customGridGraph = casterNode.Graph as CustomGridGraph;
		HashSet<GraphNode> engagedNodes = context.GetEngagedNodes();
		Vector2Int[] directMovementOffsets = DirectMovementOffsets;
		for (int i = 0; i < directMovementOffsets.Length; i++)
		{
			Vector2Int vector2Int = directMovementOffsets[i];
			CustomGridNodeBase node2 = customGridGraph.GetNode(xCoordinateInGrid + vector2Int.x * rangeCells, zCoordinateInGrid + vector2Int.y * rangeCells);
			if (node2 != null)
			{
				int distance;
				LosCalculations.CoverType los;
				CustomGridNodeBase customGridNodeBase = directMovement.GetOrientedPattern(ability, casterNode, node2).Nodes.LastOrDefault((CustomGridNodeBase node) => ability.CanTargetFromNode(casterNode, node, node.Vector3Position, out distance, out los));
				if (customGridNodeBase != null && !engagedNodes.Contains(customGridNodeBase))
				{
					return new TargetWrapper(customGridNodeBase.Vector3Position);
				}
			}
		}
		return null;
	}

	private TargetWrapper FindAppropriateRetreatPositionForFreeMovement(DecisionContext context, AbilityData ability)
	{
		CustomGridNodeBase unitNode = context.UnitNode;
		CustomGridGraph customGridGraph = unitNode.Graph as CustomGridGraph;
		List<MechanicEntity> list = TempList.Get<MechanicEntity>();
		list.AddRange(context.GetEngagingEnemies());
		HashSet<GraphNode> engagedNodes = context.GetEngagedNodes();
		GraphNode graphNode = null;
		float num = 0f;
		for (int i = unitNode.XCoordinateInGrid - ability.RangeCells; i <= unitNode.XCoordinateInGrid + ability.RangeCells; i++)
		{
			for (int j = unitNode.ZCoordinateInGrid - ability.RangeCells; j <= unitNode.ZCoordinateInGrid + ability.RangeCells; j++)
			{
				CustomGridNodeBase node = customGridGraph.GetNode(i, j);
				if (node != null && !engagedNodes.Contains(node) && WarhammerGeometryUtils.DistanceToInCells(unitNode.Vector3Position, default(IntRect), node.Vector3Position, default(IntRect)) <= ability.RangeCells && WarhammerGeometryUtils.DistanceToInCells(unitNode.Vector3Position, default(IntRect), node.Vector3Position, default(IntRect)) >= ability.MinRangeCells)
				{
					float distanceSum = 0f;
					list.ForEach(delegate(MechanicEntity enemy)
					{
						distanceSum += enemy.DistanceToInCells(node.Vector3Position, context.Unit.SizeRect);
					});
					if (num < distanceSum / (float)list.Count)
					{
						graphNode = node;
						num = distanceSum / (float)list.Count;
					}
				}
			}
		}
		if (graphNode == null)
		{
			return null;
		}
		return new TargetWrapper(graphNode.Vector3Position);
	}

	private TargetWrapper FindAppropriateTargetForPush(DecisionContext context, AbilityData ability)
	{
		if (!ability.Blueprint.CanTargetEnemies)
		{
			return null;
		}
		BaseUnitEntity unit = context.Unit;
		if (unit.GetEngagedByUnits().Count() > 1)
		{
			return null;
		}
		BaseUnitEntity baseUnitEntity = unit.GetEngagedByUnits().First();
		if (baseUnitEntity == null)
		{
			return null;
		}
		return new TargetWrapper(baseUnitEntity);
	}
}

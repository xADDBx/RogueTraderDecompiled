using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kingmaker;
using Kingmaker.AI;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace Warhammer.SpaceCombat.AI.BehaviourTrees;

public class TaskNodeFindBestTrajectory : TaskNode
{
	private Status LastStatus;

	private IEnumerator<Status> WorkCoroutine;

	private const int AllowedIterations = 100;

	protected override void InitInternal()
	{
		base.InitInternal();
		LastStatus = Status.Unknown;
		WorkCoroutine = null;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		if (WorkCoroutine == null)
		{
			WorkCoroutine = CreateCoroutine(blackboard);
		}
		for (int i = 0; i < 100; i++)
		{
			if (!WorkCoroutine.MoveNext())
			{
				break;
			}
			LastStatus = WorkCoroutine.Current;
		}
		return LastStatus;
	}

	private IEnumerator<Status> CreateCoroutine(Blackboard blackboard)
	{
		SpaceCombatDecisionContext context = (SpaceCombatDecisionContext)blackboard.DecisionContext;
		PartStarshipNavigation navigation = context.Unit.GetOptional<PartStarshipNavigation>();
		_ = context.Unit.CombatState.ActionPointsBlue;
		AILogger.Instance.Log(AILogNode.Start(this));
		context.AbilityValueCache = new AbilityValueCache(new AbilityValueCalculator(context));
		context.IsLastActionBrokePlan = false;
		context.BestTrajectoryScore = 0f;
		Stopwatch sw = new Stopwatch();
		sw.Start();
		navigation.UpdateReachableTiles_Blocking();
		sw.Stop();
		sw.Start();
		float maxScore = 0f;
		ShipPath.DirectionalPathNode bestPathNode = null;
		if (navigation.GetEndNodes().Count == 0)
		{
			context.IsBlockedByShip = true;
			context.BestPath = new List<ShipPath.DirectionalPathNode>
			{
				new ShipPath.DirectionalPathNode
				{
					node = context.UnitNode,
					direction = CustomGraphHelper.GuessDirection(context.Unit.Forward),
					canStand = true
				}
			};
			yield return Status.Success;
			yield break;
		}
		foreach (ShipPath.DirectionalPathNode rawReachableTile in navigation.RawReachableTiles)
		{
			if (CanEndTurnAtNode(context, rawReachableTile) && rawReachableTile.canStand)
			{
				float num = CalculateTrajectoryScore(context, navigation.RawReachableTiles, rawReachableTile);
				if (num > maxScore)
				{
					bestPathNode = rawReachableTile;
					maxScore = num;
				}
				yield return Status.Running;
			}
		}
		sw.Stop();
		AILogger.Instance.Log(new AILogElapsed(sw.ElapsedMilliseconds));
		if (bestPathNode != null)
		{
			AILogger.Instance.Log(new AILogTrajectorySearch(bestPathNode, maxScore));
			context.BestPathNode = bestPathNode;
			context.BestPath.Clear();
			for (ShipPath.DirectionalPathNode directionalPathNode = context.BestPathNode; directionalPathNode != null; directionalPathNode = directionalPathNode.parent)
			{
				context.BestPath.Add(directionalPathNode);
			}
			context.BestPath.Reverse();
			context.BestTrajectoryScore = maxScore;
			yield return Status.Success;
		}
		else
		{
			AILogger.Instance.Log(new AILogReason(AILogReasonType.BetterPositionNotFound));
			yield return Status.Failure;
		}
	}

	private bool CanEndTurnAtNode(DecisionContext context, ShipPath.DirectionalPathNode pathNode)
	{
		PartStarshipNavigation required = context.Unit.GetRequired<PartStarshipNavigation>();
		float actionPointsBlue = context.Unit.CombatState.ActionPointsBlue;
		if (required.IsSuicideAttacker && context.HatedTargets.Any((TargetInfo t) => t.Entity.GetOccupiedNodes().Contains(pathNode.node)))
		{
			return true;
		}
		return actionPointsBlue - (float)pathNode.lengthFromStart < (float)required.FinishingTilesCount;
	}

	private float CalculateTrajectoryScore(SpaceCombatDecisionContext context, HashSet<ShipPath.DirectionalPathNode> reachableTiles, ShipPath.DirectionalPathNode targetNode)
	{
		float num = CalculateDestinationScore(context, targetNode.node.Vector3Position);
		if (!(context.Unit.Brain.Blueprint is BlueprintStarshipBrain blueprintStarshipBrain))
		{
			return 0f;
		}
		List<BlueprintAbility> extraMeasures = blueprintStarshipBrain.ExtraMeasures;
		Dictionary<BlueprintAbility, int> dictionary = new Dictionary<BlueprintAbility, int>();
		for (ShipPath.DirectionalPathNode directionalPathNode = targetNode; directionalPathNode != null; directionalPathNode = directionalPathNode.parent)
		{
			foreach (Ability item in context.Unit.Abilities.RawFacts.Where((Ability a) => !extraMeasures.Contains(a.Blueprint)))
			{
				int value = context.AbilityValueCache.GetValue(directionalPathNode, item);
				if (!dictionary.TryGetValue(item.Blueprint, out var value2) || value2 < value)
				{
					dictionary[item.Blueprint] = value;
				}
			}
		}
		int num2 = dictionary.Sum((KeyValuePair<BlueprintAbility, int> x) => x.Value);
		float num3 = CalculateDestinationThreat(targetNode.node, blueprintStarshipBrain);
		return num + (float)num2 - num3;
	}

	private float CalculateDestinationScore(DecisionContext context, Vector3 destination)
	{
		if (!(context.Unit.Brain.Blueprint is BlueprintStarshipBrain blueprintStarshipBrain))
		{
			return 0f;
		}
		IEnumerable<StarshipEntity> enumerable = from enemy in context.Enemies
			select enemy.Entity as StarshipEntity into enemy
			where !enemy.Blueprint.IsSoftUnit
			select enemy;
		int num = enumerable.Select((StarshipEntity enemy) => enemy.DistanceToInCells(destination)).DefaultIfEmpty(0).Min();
		if (blueprintStarshipBrain.IsStrikecraftReturningBrain)
		{
			StarshipEntity overrideTarget = blueprintStarshipBrain.GetOverrideTarget(context.Unit);
			if (overrideTarget == null)
			{
				return num;
			}
			num = overrideTarget.DistanceToInCells(destination);
		}
		float num2 = 1f;
		if (blueprintStarshipBrain.TryToStayBehind)
		{
			foreach (StarshipEntity item in enumerable)
			{
				float num3 = Mathf.Abs(Vector3.SignedAngle(destination - item.Position, item.Forward, Vector3.up));
				if (num3 < 90f)
				{
					num2 += 10f;
				}
				else if (num3 < 135f)
				{
					num2 += 2f;
				}
			}
		}
		float num4 = ((float?)blueprintStarshipBrain?.AiDesiredDistanceToEnemies) ?? 3f;
		return 0.95f / (1f + Math.Abs((float)num - num4)) / num2;
	}

	private float CalculateDestinationThreat(CustomGridNodeBase node, BlueprintStarshipBrain brain)
	{
		if (!Game.Instance.MeteorStreamController.GetDangerousNodes().ContainsKey(node))
		{
			return 0f;
		}
		return brain.FearOfMeteors;
	}
}

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

	private readonly Dictionary<BlueprintAbility, int> m_abilityValues = new Dictionary<BlueprintAbility, int>();

	private readonly List<StarshipEntity> m_hardEnemies = new List<StarshipEntity>();

	public TaskNodeFindBestTrajectory()
	{
	}

	public TaskNodeFindBestTrajectory(string debugDescription)
		: base(debugDescription)
	{
	}

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
		float maxPathLength = context.Unit.CombatState.ActionPointsBlue;
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
			if (rawReachableTile.canStand && CanEndTurnAtNode(context, rawReachableTile, navigation, maxPathLength))
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
			base.FailReason = "No better position was found";
			yield return Status.Failure;
		}
	}

	private bool CanEndTurnAtNode(DecisionContext context, ShipPath.DirectionalPathNode pathNode, PartStarshipNavigation navigation, float maxPathLength)
	{
		if (navigation.IsSuicideAttacker && context.HatedTargets.Any((TargetInfo t) => t.Entity.GetOccupiedNodes().Contains(pathNode.node)))
		{
			return true;
		}
		return maxPathLength - (float)pathNode.lengthFromStart < (float)navigation.FinishingTilesCount;
	}

	private float CalculateTrajectoryScore(SpaceCombatDecisionContext context, HashSet<ShipPath.DirectionalPathNode> reachableTiles, ShipPath.DirectionalPathNode targetNode)
	{
		float num = CalculateDestinationScore(context, targetNode.node.Vector3Position);
		if (!(context.Unit.Brain.Blueprint is BlueprintStarshipBrain blueprintStarshipBrain))
		{
			return 0f;
		}
		m_abilityValues.Clear();
		for (ShipPath.DirectionalPathNode directionalPathNode = targetNode; directionalPathNode != null; directionalPathNode = directionalPathNode.parent)
		{
			if (!directionalPathNode.canStand)
			{
				return 0f;
			}
			foreach (Ability rawFact in context.Unit.Abilities.RawFacts)
			{
				if (!blueprintStarshipBrain.ExtraMeasures.Contains(rawFact.Blueprint))
				{
					int value = context.AbilityValueCache.GetValue(directionalPathNode, rawFact);
					if (!m_abilityValues.TryGetValue(rawFact.Blueprint, out var value2) || value2 < value)
					{
						m_abilityValues[rawFact.Blueprint] = value;
					}
				}
			}
		}
		int num2 = 0;
		foreach (KeyValuePair<BlueprintAbility, int> abilityValue in m_abilityValues)
		{
			num2 += abilityValue.Value;
		}
		float num3 = CalculateDestinationThreat(targetNode.node, blueprintStarshipBrain);
		return num + (float)num2 - num3;
	}

	private float CalculateDestinationScore(DecisionContext context, Vector3 destination)
	{
		if (!(context.Unit.Brain.Blueprint is BlueprintStarshipBrain blueprintStarshipBrain))
		{
			return 0f;
		}
		m_hardEnemies.Clear();
		foreach (TargetInfo enemy in context.Enemies)
		{
			if (enemy.Entity is StarshipEntity starshipEntity && !starshipEntity.Blueprint.IsSoftUnit)
			{
				m_hardEnemies.Add(starshipEntity);
			}
		}
		int num = 0;
		if (m_hardEnemies.Count > 0)
		{
			num = int.MaxValue;
			foreach (StarshipEntity hardEnemy in m_hardEnemies)
			{
				int num2 = hardEnemy.DistanceToInCells(destination);
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		if (blueprintStarshipBrain.IsStrikecraftReturningBrain)
		{
			StarshipEntity overrideTarget = blueprintStarshipBrain.GetOverrideTarget(context.Unit);
			if (overrideTarget == null)
			{
				return num;
			}
			num = overrideTarget.DistanceToInCells(destination);
		}
		float num3 = 1f;
		if (blueprintStarshipBrain.TryToStayBehind)
		{
			foreach (StarshipEntity hardEnemy2 in m_hardEnemies)
			{
				float num4 = Mathf.Abs(Vector3.SignedAngle(destination - hardEnemy2.Position, hardEnemy2.Forward, Vector3.up));
				if (num4 < 90f)
				{
					num3 += 10f;
				}
				else if (num4 < 135f)
				{
					num3 += 2f;
				}
			}
		}
		float num5 = ((float?)blueprintStarshipBrain?.AiDesiredDistanceToEnemies) ?? 3f;
		return 0.95f / (1f + Math.Abs((float)num - num5)) / num3;
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

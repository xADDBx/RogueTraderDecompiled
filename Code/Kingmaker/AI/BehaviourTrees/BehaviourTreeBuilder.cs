using Kingmaker.AI.AreaScanning.TileScorers;
using Kingmaker.AI.BehaviourTrees.Nodes;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.AI.Strategies;
using Kingmaker.AI.TargetSelectors;
using Kingmaker.AreaLogic.TimeSurvival;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Squads;
using Warhammer.SpaceCombat.AI;
using Warhammer.SpaceCombat.AI.BehaviourTrees;

namespace Kingmaker.AI.BehaviourTrees;

public static class BehaviourTreeBuilder
{
	public static BehaviourTreeNode MovementDecisionSubtree;

	static BehaviourTreeBuilder()
	{
		MovementDecisionSubtree = new Condition((Blackboard b) => b.DecisionContext.Unit.Brain.IsHoldingPosition, new Sequence(new AsyncTaskNodeCreateMoveVariants(50), TaskNodeSetupMoveCommand.ToHoldPosition()), new Selector(new Condition((Blackboard b) => !b.DecisionContext.Unit.Brain.IsUsualMeleeUnit, new Sequence(new TaskNodeExecute(delegate(Blackboard b)
		{
			b.DecisionContext.ConsideringAbility = null;
		}), new Selector(new Condition((Blackboard b) => b.DecisionContext.IsMovementInfluentAbility, new Sequence(new AsyncTaskNodeCreateMoveVariants(), new TaskNodeFindBetterPlace(new AttackEffectivenessTileScorer()), new TaskNodeExecuteWithResult(delegate(Blackboard b)
		{
			DecisionContext decisionContext2 = b.DecisionContext;
			return (new AbilityInfo(decisionContext2.Ability).GetAbilityTargetSelector().SelectTarget(decisionContext2, (CustomGridNodeBase)decisionContext2.FoundBetterPlace.BestCell.Node) != null) ? Status.Success : Status.Failure;
		}), TaskNodeSetupMoveCommand.ToBetterPosition())), new LoopOverAbilities(new Sequence(new AsyncTaskNodeCreateMoveVariants(), new TaskNodeFindBetterPlace(new AttackEffectivenessTileScorer()), new TaskNodeExecuteWithResult(delegate(Blackboard b)
		{
			DecisionContext decisionContext = b.DecisionContext;
			AbilityTargetSelector abilityTargetSelector = new AbilityInfo(decisionContext.ConsideringAbility).GetAbilityTargetSelector();
			decisionContext.AbilityTarget = abilityTargetSelector.SelectTarget(decisionContext, (CustomGridNodeBase)decisionContext.FoundBetterPlace.BestCell.Node);
			return (decisionContext.AbilityTarget != null) ? Status.Success : Status.Failure;
		}), new TaskNodeExecute(delegate(Blackboard b)
		{
			b.DecisionContext.Ability = b.DecisionContext.ConsideringAbility;
		}), TaskNodeSetupMoveCommand.ToBetterPosition()), Loop.ExitCondition.ExitOnSuccess)))), new Sequence(new TaskNodeExecute(delegate(Blackboard b)
		{
			b.DecisionContext.ConsideringAbility = null;
		}), new AsyncTaskNodeCreateMoveVariants(50), TaskNodeSetupMoveCommand.ToClosestEnemy())));
	}

	public static BehaviourTree Create(MechanicEntity owner)
	{
		if (owner is UnitEntity unit)
		{
			return CreateForUnit(unit);
		}
		if (owner is UnitSquad squad)
		{
			return CreateForSquad(squad);
		}
		if (owner is StarshipEntity starship)
		{
			return CreateForStarship(starship);
		}
		return null;
	}

	public static BehaviourTree CreateForUnit(UnitEntity unit)
	{
		LuredStrategy luredStrategy = new LuredStrategy();
		HideAwayStrategy hideAwayStrategy = new HideAwayStrategy();
		MoveAndCastStrategy moveAndCastStrategy = new MoveAndCastStrategy();
		ResponseToAoOThreatStrategy responseToAoOThreatStrategy = new ResponseToAoOThreatStrategy();
		Selector rootNode = new Selector(new Sequence(new TaskNodeWaitCommandsDone(), new Condition((Blackboard b) => b.Unit.Commands.Empty && b.Unit.State.CanActInTurnBased, new Sequence(new AsyncTaskNodeInitializeDecisionContext(), new TaskNodeTryCompleteScenario(), new TaskNodeSelectReferenceAbility(), new Selector(new Condition((Blackboard b) => b.DecisionContext.IsLured, new Sequence(luredStrategy.CreateBehaviourTree(), new Condition((Blackboard b) => b.DecisionContext.Ability != null, moveAndCastStrategy.CreateBehaviourTree()), new Condition((Blackboard b) => b.DecisionContext.ShouldResponseToAoOThreatAfterAbility, responseToAoOThreatStrategy.CreateBehaviourTree()), new Sequence(new TaskNodeSelectAbilityTarget(CastTimepointType.AfterMove), new TaskNodeCastAbility()))), new Condition((Blackboard b) => b.DecisionContext.Unit.IsInSquad, new Sequence(new TaskNodeExecute(delegate(Blackboard b)
		{
			DecisionContext decisionContext2 = b.DecisionContext;
			decisionContext2.AbilityTarget = decisionContext2.Unit.GetSquadOptional().Squad.CommonTarget;
		}), new Condition(delegate(Blackboard b)
		{
			DecisionContext decisionContext = b.DecisionContext;
			if (decisionContext.Ability == null)
			{
				AILogger.Instance.Log(AILogAbility.AbilityNotSelected(CastTimepointType.None));
				return false;
			}
			if (decisionContext.AbilityTarget == null)
			{
				AILogger.Instance.Log(AILogAbility.TargetNotFound(CastTimepointType.None, decisionContext.Ability));
				return false;
			}
			if (!decisionContext.Ability.CanTarget(decisionContext.AbilityTarget, out var unavailableReason))
			{
				AILogger.Instance.Log(AILogAbility.CantTargetWithAbility(decisionContext.Ability, decisionContext.AbilityTarget, unavailableReason));
				return false;
			}
			if (new AbilityInfo(decisionContext.Ability).GetAbilityTargetSelector() is ScatterShotTargetSelector scatterShotTargetSelector && scatterShotTargetSelector.IsScatterShotRisky(decisionContext, decisionContext.UnitNode, decisionContext.AbilityTarget.NearestNode))
			{
				AILogger.Instance.Log(AILogAbility.CantTargetWithAbility(decisionContext.Ability, decisionContext.AbilityTarget, AbilityData.UnavailabilityReasonType.FriendlyFire));
				return false;
			}
			return true;
		}, new Succeeder(new TaskNodeCastAbility()), new Succeeder(new Sequence(new TaskNodeExecute(delegate(Blackboard b)
		{
			AILogger.Instance.Log(new AILogMessage($"{b.DecisionContext.Unit} from squad chooses tries to choose new target"));
		}), new TaskNodeSelectAbilityTarget(CastTimepointType.None), new TaskNodeCastAbility()))), new TaskNodeWaitCommandsDone(), new TaskNodeTryFinishTurn())), new Sequence(new TaskNodeSelectAbilityTarget(CastTimepointType.BeforeMove), new TaskNodeCastAbility()), new Condition((Blackboard b) => b.DecisionContext.ShouldResponseToAoOThreat, responseToAoOThreatStrategy.CreateBehaviourTree()), new Condition((Blackboard b) => b.DecisionContext.Ability == null, hideAwayStrategy.CreateBehaviourTree()), new Condition((Blackboard b) => b.DecisionContext.Ability != null, moveAndCastStrategy.CreateBehaviourTree()), new Condition((Blackboard b) => b.DecisionContext.ShouldResponseToAoOThreatAfterAbility, responseToAoOThreatStrategy.CreateBehaviourTree()), new Sequence(new TaskNodeSelectAbilityTarget(CastTimepointType.AfterMove), new TaskNodeCastAbility()))))), new TaskNodeTryFinishTurn());
		return new BehaviourTree(unit, rootNode, new DecisionContext());
	}

	public static BehaviourTree CreateForSquad(UnitSquad squad)
	{
		Sequence rootNode = new Sequence(new AsyncTaskNodeInitializeDecisionContext(), new TaskNodeTryCompleteScenario(), new TaskNodeSelectReferenceAbility(), new Loop(delegate(Blackboard b)
		{
			b.DecisionContext.InitSquadUnitsEnumerator();
		}, delegate(Blackboard b)
		{
			DecisionContext decisionContext3 = b.DecisionContext;
			decisionContext3.ConsiderNextSquadUnit();
			if (decisionContext3.CurrentSquadUnit != null)
			{
				AILogger.Instance.Log(new AILogSquad(decisionContext3.CurrentSquadUnit));
			}
			return decisionContext3.CurrentSquadUnit != null;
		}, new Sequence(new Selector(new Condition((Blackboard b) => b.DecisionContext.Unit == b.DecisionContext.SquadLeader, new Sequence(new Succeeder(MovementDecisionSubtree), new TaskNodeSetupSquadTarget())), new Sequence(new AsyncTaskNodeCreateMoveVariants(), new TaskNodeFindBetterPlace(new AttackEffectivenessTileScorer()), TaskNodeSetupMoveCommand.ToBetterPosition()), new Sequence(new AsyncTaskNodeCreateMoveVariants(50), new Selector(new Condition((Blackboard b) => b.DecisionContext.SquadLeaderTarget != null, TaskNodeSetupMoveCommand.ToSquadLeaderTarget()), TaskNodeSetupMoveCommand.ToSquadLeader()))), new TaskNodeExecute(delegate(Blackboard b)
		{
			DecisionContext decisionContext2 = b.DecisionContext;
			if (decisionContext2.MoveCommand != null)
			{
				decisionContext2.SquadUnitsMoveCommands.Add((decisionContext2.CurrentSquadUnit, decisionContext2.MoveCommand));
				decisionContext2.MoveCommand = null;
			}
		}))), new Loop(delegate(Blackboard b)
		{
			b.DecisionContext.InitSquadUnitsEnumerator();
		}, delegate(Blackboard b)
		{
			DecisionContext decisionContext = b.DecisionContext;
			decisionContext.ConsiderNextSquadUnit();
			if (decisionContext.CurrentSquadUnit != null)
			{
				AILogger.Instance.Log(new AILogSquad(decisionContext.CurrentSquadUnit));
			}
			return decisionContext.CurrentSquadUnit != null;
		}, new Sequence(new TaskNodeExecuteMoveCommand(), new TaskNodeExecute(delegate(Blackboard b)
		{
			b.DecisionContext.CurrentSquadUnit.CombatState.SpendActionPointsAll(yellow: false, blue: true);
		}))), new TaskNodeWaitCommandsDone(), new TaskNodeTryFinishTurn());
		return new BehaviourTree(squad, rootNode, new DecisionContext());
	}

	public static BehaviourTree CreateForStarship(StarshipEntity starship)
	{
		Sequence rootNode = new Sequence(new TaskNodeWaitCommandsDone(), new Succeeder(new Condition((Blackboard b) => Game.Instance.CurrentlyLoadedArea.GetComponent<TimeSurvival>()?.IsShouldDoNothing(b.Unit) ?? false, new Sequence(new TaskNodeWaitSpawnTimeSurvival(), new TaskNodeWaitCommandsDone()), new Condition((Blackboard b) => b.Unit.Commands.Empty && b.Unit.State.CanActInTurnBased, new Sequence(new AsyncTaskNodeInitializeDecisionContext(), new TaskNodeFindBestTrajectory(), new Selector(new Condition(delegate(Blackboard b)
		{
			SpaceCombatDecisionContext obj = (SpaceCombatDecisionContext)b.DecisionContext;
			float num = (obj.Unit.Brain.Blueprint as BlueprintStarshipBrain)?.TrajectoryScoreMinThreshold ?? 0f;
			return obj.BestTrajectoryScore < num;
		}, new TaskNodeTryStarshipExtraMeasures()), new Sequence(new TaskNodeFindWhenToCastAbility(), new Loop(delegate
		{
		}, (Blackboard b) => ((SpaceCombatDecisionContext)b.DecisionContext).BestPath.Count > 0, new Sequence(new TaskNodeDoNextAction(), new TaskNodeWaitCommandsDone(), new TaskNodeExecuteWithResult(delegate(Blackboard b)
		{
			if (((SpaceCombatDecisionContext)b.DecisionContext).IsLastActionBrokePlan || b.DecisionContext.Unit.Brain.EnemyConditionsDirty)
			{
				AILogger.Instance.Log(new AILogReason(AILogReasonType.StarshipPlanWasBroken));
				return Status.Failure;
			}
			return Status.Success;
		})), Loop.ExitCondition.ExitOnFailure))))))), new Condition((Blackboard b) => !((SpaceCombatDecisionContext)b.DecisionContext).IsLastActionBrokePlan, new TaskNodeTryFinishTurn()));
		return new BehaviourTree(starship, rootNode, new SpaceCombatDecisionContext());
	}
}

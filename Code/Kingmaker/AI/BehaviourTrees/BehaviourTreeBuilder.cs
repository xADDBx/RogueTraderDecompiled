using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI.AreaScanning.Scoring;
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
	private static Dictionary<CustomBehaviourType, ICustomBehaviourTreeBuilder> CustomBehaviourTreeBuilders;

	public static BehaviourTreeNode MovementDecisionSubtree;

	static BehaviourTreeBuilder()
	{
		CustomBehaviourTreeBuilders = new Dictionary<CustomBehaviourType, ICustomBehaviourTreeBuilder> { 
		{
			CustomBehaviourType.DLC2_FeudalWorld_GovernorAndGolemsSquad,
			new DLC2_FeudalWorld_GovernorAndGolemsSquad()
		} };
		MovementDecisionSubtree = new Condition("Субдерево принятия решения о перемещении\n\nЮнит должен удерживать позицию?\n", (Blackboard b) => b.DecisionContext.Unit.Brain.IsHoldingPosition, "Unit.Brain.IsHoldingPosition", new Sequence(new AsyncTaskNodeCreateMoveVariants("Рассчет возможных путей на бюджет в 50 МП\n", 50), TaskNodeSetupMoveCommand.ToHoldPosition("Построение пути к удерживаемой позиции\n")), new Selector(new Condition("Юнит - не обычный боец ближнего боя?\n", (Blackboard b) => !b.DecisionContext.Unit.Brain.IsUsualMeleeUnit, "!Unit.Brain.IsUsualMeleeUnit", new Sequence(new TaskNodeExecute("Очистка рассматриваемой абилки\n", delegate(Blackboard b)
		{
			b.DecisionContext.ConsideringAbility = null;
		}, "ConsideringAbility = null"), new Selector(new Condition("У юнита есть запомненная абилка, влияющая на выбор позиции?\n", (Blackboard b) => b.DecisionContext.IsMovementInfluentAbility, "IsMovementInfluentAbility", new Sequence(new AsyncTaskNodeCreateMoveVariants("Рассчет возможных путей на бюджет имеющихся МП, но для swarm-ов лимит в 3МП, для common-ов в 4МП\n", 50), new TaskNodeFindBetterPlace("Поиск лучшей позиции относительно эффективности каста запомненной абилки и запоминание её\n", new AttackEffectivenessTileScorer()), new TaskNodeExecuteWithResult("Выбрать цель для каста абилки с запомненной на предыдущем шаге позиции\n", delegate(Blackboard b)
		{
			DecisionContext decisionContext2 = b.DecisionContext;
			int num2;
			if (!(new AbilityInfo(decisionContext2.Ability).GetAbilityTargetSelector().SelectTarget(decisionContext2, (CustomGridNodeBase)decisionContext2.FoundBetterPlace.BestCell.Node) != null))
			{
				ScoreOrder scoreOrder2 = decisionContext2.ScoreOrder;
				if (scoreOrder2 == null || scoreOrder2.Order?.First() != ScoreType.BodyGuardScore)
				{
					num2 = 2;
					goto IL_0082;
				}
			}
			num2 = 1;
			goto IL_0082;
			IL_0082:
			string item2 = ((num2 == 1) ? "" : "Target was not found");
			return ((Status)num2, item2);
		}, "Select target for ability or bodyguard"), TaskNodeSetupMoveCommand.ToBetterPosition("Построение пути к лучшей позиции и запоминание его\n"))), new LoopOverAbilities("Перебор абилок, пока ветвь не будет выполнена успешно\n", new Sequence(new AsyncTaskNodeCreateMoveVariants("Рассчет возможных путей на бюджет имеющихся МП, но для swarm-ов лимит в 3МП, для common-ов в 4МП\n"), new TaskNodeFindBetterPlace("Поиск лучшей позиции относительно эффективности каста запомненной абилки и запоминание её\n", new AttackEffectivenessTileScorer()), new TaskNodeExecuteWithResult("Выбрать цель для каста абилки с запомненной на предыдущем шаге позиции\n", delegate(Blackboard b)
		{
			DecisionContext decisionContext = b.DecisionContext;
			AbilityTargetSelector abilityTargetSelector = new AbilityInfo(decisionContext.ConsideringAbility).GetAbilityTargetSelector();
			decisionContext.AbilityTarget = abilityTargetSelector.SelectTarget(decisionContext, (CustomGridNodeBase)decisionContext.FoundBetterPlace.BestCell.Node);
			int num;
			if (!(decisionContext.AbilityTarget != null))
			{
				ScoreOrder scoreOrder = decisionContext.ScoreOrder;
				if (scoreOrder == null || scoreOrder.Order?.First() != ScoreType.BodyGuardScore)
				{
					num = 2;
					goto IL_0092;
				}
			}
			num = 1;
			goto IL_0092;
			IL_0092:
			string item = ((num == 1) ? "" : "Target was not found");
			return ((Status)num, item);
		}, "Select target for ability or bodyguard"), new TaskNodeExecute("Запомнить выбранную абилку\n", delegate(Blackboard b)
		{
			b.DecisionContext.Ability = b.DecisionContext.ConsideringAbility;
		}, "Ability = ConsideringAbility"), TaskNodeSetupMoveCommand.ToBetterPosition("Построение пути к лучшей позиции и запоминание его\n")), Loop.ExitCondition.ExitOnSuccess)))), new Sequence(new TaskNodeExecute("Очистка рассматриваемой абилки\n", delegate(Blackboard b)
		{
			b.DecisionContext.ConsideringAbility = null;
		}, "ConsideringAbility = null"), new AsyncTaskNodeCreateMoveVariants("Рассчет возможных путей на бюджет имеющихся МП\n", 50), TaskNodeSetupMoveCommand.ToClosestEnemy("Построение и запоминание пути к ближайшему врагу\n"))));
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

	public static bool TryCreateCustom(MechanicEntity entity, CustomBehaviourType type, out BehaviourTree behaviourTree)
	{
		behaviourTree = null;
		if (type == CustomBehaviourType.None)
		{
			return false;
		}
		if (CustomBehaviourTreeBuilders.TryGetValue(type, out var value))
		{
			behaviourTree = value.Create(entity);
		}
		return behaviourTree != null;
	}

	private static BehaviourTree CreateForUnit(UnitEntity unit)
	{
		LuredStrategy luredStrategy = new LuredStrategy();
		HideAwayStrategy hideAwayStrategy = new HideAwayStrategy();
		MoveAndCastStrategy moveAndCastStrategy = new MoveAndCastStrategy();
		BodyGuardStrategy bodyGuardStrategy = new BodyGuardStrategy();
		ResponseToAoOThreatStrategy responseToAoOThreatStrategy = new ResponseToAoOThreatStrategy();
		Selector rootNode = new Selector(new Sequence("Основная ветка принятия решения юнита", new TaskNodeWaitCommandsDone("Подождать завершения всех команд\n"), new Condition("У юнита нет команд и он может действовать в пошаговом режиме?\n", (Blackboard b) => b.Unit.Commands.Empty && b.Unit.State.CanActInTurnBased, "Unit.Commands.Empty && Unit.State.CanActInTurnBased", new Sequence("Принятие решения о действии юнита", new AsyncTaskNodeInitializeDecisionContext("Инициализация контекста принятия решения"), new TaskNodeTryCompleteScenario("Попытаться завершить сценарий, если условие завершения сценария выполнено:\n\n- удержание позиции\n- прорыв\n- приоритетная цель"), new TaskNodeSelectReferenceAbility("Выбрать абилку для каста из списка Movement Influent Abilities, если он не пустой, или лучшую по приоритету или другим параметрам"), new Selector("Выбор стратегии в зависимости от ситуации", new Condition("Юнит приманивается?\n", (Blackboard b) => b.DecisionContext.IsLured, "Unit.GetOptional<UnitPartLure>()?.UnitLuredTo != null", new Sequence(luredStrategy.CreateBehaviourTree(), new Condition("Юнит в режиме телохранителя?\n", delegate(Blackboard b)
		{
			ScoreOrder scoreOrder2 = b.DecisionContext.ScoreOrder;
			return scoreOrder2 != null && scoreOrder2.Order?.First() == ScoreType.BodyGuardScore;
		}, "ScoreOrder?.Order?.First() == ScoreType.BodyGuardScore", bodyGuardStrategy.CreateBehaviourTree(), new Condition("У юнита есть запомненная абилка?\n", (Blackboard b) => b.DecisionContext.Ability != null, "Ability != null", moveAndCastStrategy.CreateBehaviourTree())), new Condition("Юнит должен отреагировать на угрозу АоО после использования абилки?\n", (Blackboard b) => b.DecisionContext.ShouldResponseToAoOThreatAfterAbility, "Unit.Brain.ResponseToAoOThreatAfterAbility && Unit.CombatState.IsEngaged", responseToAoOThreatStrategy.CreateBehaviourTree()), new Sequence("Выбор цели и каст абилки после движения", new TaskNodeSelectAbilityTarget("Выбор цели для абилки после движения", CastTimepointType.AfterMove), new TaskNodeCastAbility("Каст абилки после движения")))), new Condition("Юнит в отряде?\n", (Blackboard b) => b.DecisionContext.Unit.IsInSquad, "Unit.IsInSquad", new Sequence(new TaskNodeExecute("Взять общую цель отряда и запомнить её как цель для каста абилки", delegate(Blackboard b)
		{
			DecisionContext decisionContext2 = b.DecisionContext;
			decisionContext2.AbilityTarget = decisionContext2.Unit.GetSquadOptional().Squad.CommonTarget;
		}, "AbilityTarget = Unit.GetSquadOptional().Squad.CommonTarget"), new Condition("У юнита есть запомненная цель, есть запомненная абилка, абилку можно применить в цель и это не рискованный скаттершот по союзникам?\n", delegate(Blackboard b)
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
		}, "Ability != null && AbilityTarget != null && Ability.CanTarget && !Ability.TargetSelector.IsScatterShotRisky", new Succeeder(new TaskNodeCastAbility("Каст абилки по цели отряда")), new Succeeder(new Sequence("Выбор другой цели для абилки отряда и каст абилки по новой цели", new TaskNodeExecute(delegate(Blackboard b)
		{
			AILogger.Instance.Log(new AILogMessage($"{b.DecisionContext.Unit} from squad chooses tries to choose new target"));
		}, "Log unit from squad chooses tries to choose new target"), new TaskNodeSelectAbilityTarget("Выбор цели для абилки", CastTimepointType.None), new TaskNodeCastAbility("Каст абилки")))), new TaskNodeWaitCommandsDone("Подождать завершения всех команд отряда"), new TaskNodeTryFinishTurn("Попытаться завершить ход, если нет других действий для выполнения"))), new Sequence("Выбор цели и каст абилки перед движением", new TaskNodeSelectAbilityTarget("Выбор цели для абилки перед движением", CastTimepointType.BeforeMove), new TaskNodeCastAbility("Каст абилки перед движением")), new Condition("Юнит в режиме телохранителя?\n", delegate(Blackboard b)
		{
			ScoreOrder scoreOrder = b.DecisionContext.ScoreOrder;
			return scoreOrder != null && scoreOrder.Order?.First() == ScoreType.BodyGuardScore;
		}, "ScoreOrder?.Order?.First() == ScoreType.BodyGuardScore", bodyGuardStrategy.CreateBehaviourTree()), new Condition("Юнит должен отреагировать на угрозу АоО?\n", (Blackboard b) => b.DecisionContext.ShouldResponseToAoOThreat, "Unit.Brain.ResponseToAoOThreat && Unit.CombatState.IsEngaged", responseToAoOThreatStrategy.CreateBehaviourTree()), new Condition("У юнита нет запомненной абилки?\n", (Blackboard b) => b.DecisionContext.Ability == null, "Ability == null", hideAwayStrategy.CreateBehaviourTree()), new Condition("У юнита есть запомненная абилка?\n", (Blackboard b) => b.DecisionContext.Ability != null, "Ability != null", moveAndCastStrategy.CreateBehaviourTree()), new Condition("Юнит должен отреагировать на угрозу АоО после использования абилки?\n", (Blackboard b) => b.DecisionContext.ShouldResponseToAoOThreatAfterAbility, "Unit.Brain.ResponseToAoOThreatAfterAbility && Unit.CombatState.IsEngaged", responseToAoOThreatStrategy.CreateBehaviourTree()), new Sequence("Выбор цели и каст абилки после движения", new TaskNodeSelectAbilityTarget("Выбор цели для абилки после движения\n", CastTimepointType.AfterMove), new TaskNodeCastAbility("Каст абилки после движения\n")))))), new Condition("Должен ли юнит выбирать не-hated цели?\n", (Blackboard b) => (b.Unit.Brain?.Blueprint?.TargetOthersIfCantReachHated).GetValueOrDefault(), "Unit.Brain?.Blueprint?.TargetOthersIfCantReachHated ?? false", new Sequence(new TaskNodeSelectAbilityTarget("Выбор цели для абилки\n", CastTimepointType.None, tryTargetAllEnemies: true), new TaskNodeCastAbility("Каст абилки\n"))), new Condition("Должен ли юнит выбирать не-hated цели после движения?\n", (Blackboard b) => (b.Unit.Brain?.Blueprint?.TargetOthersIfCantReachHated).GetValueOrDefault(), "Unit.Brain?.Blueprint?.TargetOthersIfCantReachHated ?? false", new Sequence(new TaskNodeSelectAbilityTarget("Выбор цели для абилки после движения\n", CastTimepointType.AfterMove, tryTargetAllEnemies: true), new TaskNodeCastAbility("Каст абилки\n"))), new TaskNodeTryFinishTurn("Попытаться завершить ход, если нет других действий для выполнения\n"));
		return new BehaviourTree(unit, rootNode, new DecisionContext());
	}

	private static BehaviourTree CreateForSquad(UnitSquad squad)
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
		}, "Iterate all squad units", new Sequence(new Selector(new Condition((Blackboard b) => b.DecisionContext.Unit == b.DecisionContext.SquadLeader, "Unit == SquadLeader", new Sequence(new Succeeder(MovementDecisionSubtree), new TaskNodeSetupSquadTarget())), new Sequence(new AsyncTaskNodeCreateMoveVariants(), new TaskNodeFindBetterPlace(new AttackEffectivenessTileScorer()), TaskNodeSetupMoveCommand.ToBetterPosition()), new Sequence(new AsyncTaskNodeCreateMoveVariants(50), new Selector(new Condition((Blackboard b) => b.DecisionContext.SquadLeaderTarget != null, "SquadLeaderTarget != null", TaskNodeSetupMoveCommand.ToSquadLeaderTarget()), TaskNodeSetupMoveCommand.ToSquadLeader()))), new TaskNodeExecute(delegate(Blackboard b)
		{
			DecisionContext decisionContext2 = b.DecisionContext;
			if (decisionContext2.MoveCommand != null)
			{
				decisionContext2.SquadUnitsMoveCommands.Add((decisionContext2.CurrentSquadUnit, decisionContext2.MoveCommand));
				decisionContext2.MoveCommand = null;
			}
		}, "Store move command for current squad unit"))), new Loop(delegate(Blackboard b)
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
		}, "Iterate all squad units", new Sequence(new TaskNodeExecuteMoveCommand(), new TaskNodeExecute(delegate(Blackboard b)
		{
			b.DecisionContext.CurrentSquadUnit.CombatState.SpendActionPointsAll(yellow: false, blue: true);
		}, "Spend all move points of current squad unit"))), new TaskNodeWaitCommandsDone(), new TaskNodeTryFinishTurn());
		return new BehaviourTree(squad, rootNode, new DecisionContext());
	}

	private static BehaviourTree CreateForStarship(StarshipEntity starship)
	{
		Sequence rootNode = new Sequence(new TaskNodeWaitCommandsDone(), new Succeeder(new Condition((Blackboard b) => Game.Instance.CurrentlyLoadedArea.GetComponent<TimeSurvival>()?.IsShouldDoNothing(b.Unit) ?? false, "CurrentlyLoadedArea.TimeSurvival != null && CurrentlyLoadedArea.TimeSurvival.IsShouldDoNothing(Unit)", new Sequence(new TaskNodeWaitSpawnTimeSurvival(), new TaskNodeWaitCommandsDone()), new Condition((Blackboard b) => b.Unit.Commands.Empty && b.Unit.State.CanActInTurnBased, "Unit.Commands.Empty && Unit.State.CanActInTurnBased", new Sequence(new AsyncTaskNodeInitializeDecisionContext(), new TaskNodeFindBestTrajectory(), new Selector(new Condition(delegate(Blackboard b)
		{
			SpaceCombatDecisionContext obj = (SpaceCombatDecisionContext)b.DecisionContext;
			float num = (obj.Unit.Brain.Blueprint as BlueprintStarshipBrain)?.TrajectoryScoreMinThreshold ?? 0f;
			return obj.BestTrajectoryScore < num;
		}, "BestTrajectoryScore < Unit.Brain.TrajectoryScoreMinThreshold", new TaskNodeTryStarshipExtraMeasures()), new Sequence(new TaskNodeFindWhenToCastAbility(), new Loop(delegate
		{
		}, (Blackboard b) => ((SpaceCombatDecisionContext)b.DecisionContext).BestPath.Count > 0, "While BestPath.Count > 0 && No Failure", new Sequence(new TaskNodeDoNextAction(), new TaskNodeWaitCommandsDone(), new TaskNodeExecuteWithResult(delegate(Blackboard b)
		{
			if (((SpaceCombatDecisionContext)b.DecisionContext).IsLastActionBrokePlan || b.DecisionContext.Unit.Brain.EnemyConditionsDirty)
			{
				AILogger.Instance.Log(new AILogReason(AILogReasonType.StarshipPlanWasBroken));
				return (Status.Failure, "Starship plan was broken");
			}
			return (Status.Success, "");
		}, "Fail if IsLastActionBrokePlan or Unit.Brain.EnemyConditionsDirty")), Loop.ExitCondition.ExitOnFailure))))))), new Condition((Blackboard b) => !((SpaceCombatDecisionContext)b.DecisionContext).IsLastActionBrokePlan, "!IsLastActionBrokePlan", new TaskNodeTryFinishTurn()));
		return new BehaviourTree(starship, rootNode, new SpaceCombatDecisionContext());
	}
}

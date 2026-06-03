using Kingmaker.AI.AreaScanning.TileScorers;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;

namespace Kingmaker.AI.Strategies;

public class BodyGuardStrategy : AiStrategy
{
	public override BehaviourTreeNode CreateBehaviourTree()
	{
		return new Sequence(new Sequence(new Condition("Удерживает ли юнит позицию?\n", (Blackboard b) => b.DecisionContext.Unit.Brain.IsHoldingPosition, "Unit.Brain.IsHoldingPosition", new Sequence(new AsyncTaskNodeCreateMoveVariants("Рассчет возможных путей на бюджет в 50 МП\n", 50), TaskNodeSetupMoveCommand.ToHoldPosition("Построение пути к удерживаемой позиции\n")), new Sequence(new TaskNodeExecute("Очистка рассматриваемой абилки\n", delegate(Blackboard b)
		{
			b.DecisionContext.ConsideringAbility = null;
		}, "ConsideringAbility = null"), new Sequence(new AsyncTaskNodeCreateMoveVariants("Рассчет возможных путей на бюджет в 50 МП\n", 50), new TaskNodeFindBetterPlace("Построение пути к лучшей позиции\n", new AttackEffectivenessTileScorer()), TaskNodeSetupMoveCommand.ToBetterPosition("Построение пути к лучшей позиции\n")))), new Sequence("Движение и трата всех МП\n", new TaskNodeExecuteMoveCommand("Выполнение команды движения\n"), new TaskNodeExecute("Трата всех МП юнита\n", delegate(Blackboard b)
		{
			b.DecisionContext.Unit.CombatState.SpendActionPointsAll(yellow: false, blue: true);
		}, "Spend all move points of current unit"), new TaskNodeWaitCommandsDone("Ожидание завершения команд\n")), new Selector("Выбор цели и каст абилки без перемещения или после него\n", new Sequence("Выбор цели и каст абилки\n", new TaskNodeSelectAbilityTarget("Выбор цели для абилки\n", CastTimepointType.None), new TaskNodeCastAbility("Каст абилки\n")), new Sequence("Выбор цели и каст абилки после движения\n", new TaskNodeSelectAbilityTarget("Выбор цели для абилки после движения\n", CastTimepointType.AfterMove), new TaskNodeCastAbility("Каст абилки после перемещения\n")), new TaskNodeTryFinishTurn("Попытка завершить ход\n"))))
		{
			DebugName = "Субдерево - стратегия режима телохранителя"
		};
	}
}

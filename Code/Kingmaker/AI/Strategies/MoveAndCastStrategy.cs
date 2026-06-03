using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;

namespace Kingmaker.AI.Strategies;

public class MoveAndCastStrategy : AiStrategy
{
	public override BehaviourTreeNode CreateBehaviourTree()
	{
		return new Selector(new Sequence("Перемещение с последующим использованием запомненной абилки\n", BehaviourTreeBuilder.MovementDecisionSubtree, new TaskNodeExecuteMoveCommand("Выполнение команды движения\n"), new TaskNodeWaitCommandsDone("Ожидание завершения движения\n"), new Succeeder(new Condition("У юнита есть запомненная абилка, влияющая на выбор позиции?\n", (Blackboard b) => b.DecisionContext.IsMovementInfluentAbility, "IsMovementInfluentAbility", new LoopOverAbilities("Перебор абилок, пока ветвь не будет выполнена успешно\n", new Sequence(new TaskNodeExecuteWithResult("Выбор цели для каста абилки с текущей позиции\n", delegate(Blackboard b)
		{
			DecisionContext decisionContext = b.DecisionContext;
			AbilityTargetSelector abilityTargetSelector = new AbilityInfo(decisionContext.ConsideringAbility).GetAbilityTargetSelector();
			decisionContext.AbilityTarget = abilityTargetSelector.SelectTarget(decisionContext, decisionContext.UnitNode);
			int num = ((decisionContext.AbilityTarget != null) ? 1 : 2);
			string item = ((num == 1) ? "" : "Target was not found");
			return ((Status)num, item);
		}, "Select target for ability"), new TaskNodeExecute("Запоминание выбранной абилки\n", delegate(Blackboard b)
		{
			b.DecisionContext.Ability = b.DecisionContext.ConsideringAbility;
		}, "Ability = ConsideringAbility")), Loop.ExitCondition.ExitOnSuccess))), new Succeeder(new Condition("У юнита есть цель и нет изменений среди противников?\n", (Blackboard b) => b.DecisionContext.AbilityTarget != null && !b.DecisionContext.Unit.Brain.EnemyConditionsDirty, "AbilityTarget != null && !Unit.Brain.EnemyConditionsDirty", new TaskNodeCastAbility("Каст запомненной абилки в выбранную цель\n")))), new Sequence("Подбор и применение способности без учета выбранной абилки\n", new TaskNodeSelectAbilityTarget("Выбор цели для абилки\n", CastTimepointType.None), new TaskNodeCastAbility("Каст абилки\n")))
		{
			DebugName = "Субдерево - стратегия движения и использования абилки"
		};
	}
}

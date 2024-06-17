using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;

namespace Kingmaker.AI.Strategies;

public class MoveAndCastStrategy : AiStrategy
{
	public override BehaviourTreeNode CreateBehaviourTree()
	{
		return new Selector(new Sequence(BehaviourTreeBuilder.MovementDecisionSubtree, new TaskNodeExecuteMoveCommand(), new TaskNodeWaitCommandsDone(), new Succeeder(new Condition((Blackboard b) => b.DecisionContext.IsMovementInfluentAbility, new LoopOverAbilities(new Sequence(new TaskNodeExecuteWithResult(delegate(Blackboard b)
		{
			DecisionContext decisionContext = b.DecisionContext;
			AbilityTargetSelector abilityTargetSelector = new AbilityInfo(decisionContext.ConsideringAbility).GetAbilityTargetSelector();
			decisionContext.AbilityTarget = abilityTargetSelector.SelectTarget(decisionContext, decisionContext.UnitNode);
			return (decisionContext.AbilityTarget != null) ? Status.Success : Status.Failure;
		}), new TaskNodeExecute(delegate(Blackboard b)
		{
			b.DecisionContext.Ability = b.DecisionContext.ConsideringAbility;
		})), Loop.ExitCondition.ExitOnSuccess))), new Succeeder(new Condition((Blackboard b) => b.DecisionContext.AbilityTarget != null && !b.DecisionContext.Unit.Brain.EnemyConditionsDirty, new TaskNodeCastAbility()))), new Sequence(new TaskNodeSelectAbilityTarget(CastTimepointType.None), new TaskNodeCastAbility()))
		{
			DebugName = "MoveAndCastStrategy"
		};
	}
}

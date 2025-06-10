using Kingmaker.AI.AreaScanning.TileScorers;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;

namespace Kingmaker.AI.Strategies;

public class BodyGuardStrategy : AiStrategy
{
	public override BehaviourTreeNode CreateBehaviourTree()
	{
		return new Sequence(new Sequence(new Condition((Blackboard b) => b.DecisionContext.Unit.Brain.IsHoldingPosition, new Sequence(new AsyncTaskNodeCreateMoveVariants(50), TaskNodeSetupMoveCommand.ToHoldPosition()), new Sequence(new TaskNodeExecute(delegate(Blackboard b)
		{
			b.DecisionContext.ConsideringAbility = null;
		}), new Sequence(new AsyncTaskNodeCreateMoveVariants(50), new TaskNodeFindBetterPlace(new AttackEffectivenessTileScorer()), TaskNodeSetupMoveCommand.ToBetterPosition()))), new Sequence(new TaskNodeExecuteMoveCommand(), new TaskNodeExecute(delegate(Blackboard b)
		{
			b.DecisionContext.Unit.CombatState.SpendActionPointsAll(yellow: false, blue: true);
		}), new TaskNodeWaitCommandsDone()), new Selector(new Sequence(new TaskNodeSelectAbilityTarget(CastTimepointType.None), new TaskNodeCastAbility()), new Sequence(new TaskNodeSelectAbilityTarget(CastTimepointType.AfterMove), new TaskNodeCastAbility()), new TaskNodeTryFinishTurn())))
		{
			DebugName = "BodyGuardTree"
		};
	}
}

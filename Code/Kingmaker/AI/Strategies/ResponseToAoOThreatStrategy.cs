using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;

namespace Kingmaker.AI.Strategies;

public class ResponseToAoOThreatStrategy : AiStrategy
{
	public override BehaviourTreeNode CreateBehaviourTree()
	{
		return new Selector(new Sequence(new TaskNodeSelectAbilityToEscapeFromThreat(), new TaskNodeCastAbility()), new Sequence(new AsyncTaskNodeCreateMoveVariants(), new TaskNodeFindPositionForRetreat(), TaskNodeSetupMoveCommand.ToBetterPosition(), new TaskNodeExecuteMoveCommand()))
		{
			DebugName = "ResponseToAoOThreatStrategy"
		};
	}
}

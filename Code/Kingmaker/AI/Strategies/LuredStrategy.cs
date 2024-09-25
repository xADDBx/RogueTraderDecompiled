using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;

namespace Kingmaker.AI.Strategies;

public class LuredStrategy : AiStrategy
{
	public override BehaviourTreeNode CreateBehaviourTree()
	{
		return new Sequence(new AsyncTaskNodeCreateMoveVariants(50), TaskNodeSetupMoveCommand.ToLureCaster(), new TaskNodeExecuteMoveCommand(), new TaskNodeWaitCommandsDone())
		{
			DebugName = "LuredStrategy"
		};
	}
}

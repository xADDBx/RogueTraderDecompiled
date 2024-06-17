using Kingmaker.AI.DebugUtilities;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeTryFinishTurn : TaskNode
{
	protected override Status TickInternal(Blackboard blackboard)
	{
		if (blackboard.Unit == null || blackboard.Unit.Commands.Empty)
		{
			AILogger.Instance.Log(new AILogReason(AILogReasonType.NothingToDo));
			blackboard.IsFinishedTurn = true;
		}
		return Status.Success;
	}
}

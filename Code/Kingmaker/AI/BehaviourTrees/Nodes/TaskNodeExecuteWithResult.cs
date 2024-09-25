using System;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeExecuteWithResult : TaskNode
{
	private Func<Blackboard, Status> action;

	public TaskNodeExecuteWithResult(Func<Blackboard, Status> action)
	{
		this.action = action;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		return action(blackboard);
	}
}

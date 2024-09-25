using System;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeExecute : TaskNode
{
	private Action<Blackboard> action;

	public TaskNodeExecute(Action<Blackboard> action)
	{
		this.action = action;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		action(blackboard);
		return Status.Success;
	}
}

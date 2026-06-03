using System;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeExecute : TaskNode
{
	private Action<Blackboard> action;

	public string Description { get; }

	public TaskNodeExecute(Action<Blackboard> action, string description)
	{
		this.action = action;
		Description = description;
	}

	public TaskNodeExecute(string debugDescription, Action<Blackboard> action, string description)
		: base(debugDescription)
	{
		this.action = action;
		Description = description;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		action(blackboard);
		return Status.Success;
	}
}

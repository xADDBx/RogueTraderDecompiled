using System;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeExecuteWithResult : TaskNode
{
	private Func<Blackboard, (Status, string)> action;

	public string Description { get; }

	public TaskNodeExecuteWithResult(Func<Blackboard, (Status, string)> action, string description)
	{
		this.action = action;
		Description = description;
	}

	public TaskNodeExecuteWithResult(string debugDescription, Func<Blackboard, (Status, string)> action, string description)
		: base(debugDescription)
	{
		this.action = action;
		Description = description;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		(Status, string) tuple = action(blackboard);
		Status item = tuple.Item1;
		string item2 = tuple.Item2;
		base.FailReason = item2;
		return item;
	}
}

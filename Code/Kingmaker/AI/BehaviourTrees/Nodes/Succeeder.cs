namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Succeeder : Decorator
{
	public Succeeder(BehaviourTreeNode node)
		: base(node)
	{
	}

	public Succeeder(string debugDescription, BehaviourTreeNode node)
		: base(debugDescription, node)
	{
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		if (base.Child.Tick(blackboard) == Status.Running)
		{
			return Status.Running;
		}
		return Status.Success;
	}
}

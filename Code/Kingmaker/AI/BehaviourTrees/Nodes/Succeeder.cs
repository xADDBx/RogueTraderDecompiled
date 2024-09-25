namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Succeeder : Decorator
{
	public Succeeder(BehaviourTreeNode node)
		: base(node)
	{
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		if (child.Tick(blackboard) == Status.Running)
		{
			return Status.Running;
		}
		return Status.Success;
	}
}

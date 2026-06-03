namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Selector : Composite
{
	public Selector(params BehaviourTreeNode[] nodes)
		: base(nodes)
	{
	}

	public Selector(string debugDescription, params BehaviourTreeNode[] nodes)
		: base(debugDescription, nodes)
	{
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		while (currentChildIndex < children.Length)
		{
			Status status = children[currentChildIndex].Tick(blackboard);
			if (status != Status.Failure)
			{
				return status;
			}
			currentChildIndex++;
		}
		base.FailReason = "All children failed";
		return Status.Failure;
	}
}

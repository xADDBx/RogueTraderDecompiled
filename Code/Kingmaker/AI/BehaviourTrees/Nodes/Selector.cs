namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Selector : Composite
{
	public Selector(params BehaviourTreeNode[] nodes)
		: base(nodes)
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
		return Status.Failure;
	}
}

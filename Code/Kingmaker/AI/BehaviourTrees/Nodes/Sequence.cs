namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Sequence : Composite
{
	public Sequence(params BehaviourTreeNode[] nodes)
		: base(nodes)
	{
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		while (currentChildIndex < children.Length)
		{
			Status status = children[currentChildIndex].Tick(blackboard);
			if (status != Status.Success)
			{
				return status;
			}
			currentChildIndex++;
		}
		return Status.Success;
	}
}

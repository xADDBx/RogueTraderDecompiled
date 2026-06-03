namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Sequence : Composite
{
	public Sequence(params BehaviourTreeNode[] nodes)
		: base(nodes)
	{
	}

	public Sequence(string debugDescription, params BehaviourTreeNode[] nodes)
		: base(debugDescription, nodes)
	{
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		while (currentChildIndex < children.Length)
		{
			Status status = children[currentChildIndex].Tick(blackboard);
			if (status != Status.Success)
			{
				if (status == Status.Failure)
				{
					base.FailReason = "Failed at node: " + children[currentChildIndex].DebugName;
				}
				return status;
			}
			currentChildIndex++;
		}
		return Status.Success;
	}
}

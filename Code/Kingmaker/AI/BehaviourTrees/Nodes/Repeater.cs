namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Repeater : Decorator
{
	private readonly int countLimit;

	private int count;

	private bool shouldInitChild;

	public Repeater(BehaviourTreeNode node, int limit)
		: base(node)
	{
		countLimit = limit;
		count = 0;
		shouldInitChild = true;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		while (count < countLimit)
		{
			if (shouldInitChild)
			{
				shouldInitChild = false;
				child.Init();
			}
			if (child.Tick(blackboard) != Status.Running)
			{
				count++;
				shouldInitChild = true;
				continue;
			}
			return Status.Running;
		}
		return Status.Success;
	}
}

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Repeater : Decorator
{
	public int CountLimit { get; }

	public int Count { get; private set; }

	public bool ShouldInitChild { get; private set; }

	public Repeater(BehaviourTreeNode node, int limit)
		: base(node)
	{
		CountLimit = limit;
		Count = 0;
		ShouldInitChild = true;
	}

	public Repeater(string debugDescription, BehaviourTreeNode node, int limit)
		: base(debugDescription, node)
	{
		CountLimit = limit;
		Count = 0;
		ShouldInitChild = true;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		while (Count < CountLimit)
		{
			if (ShouldInitChild)
			{
				ShouldInitChild = false;
				base.Child.Init();
			}
			if (base.Child.Tick(blackboard) != Status.Running)
			{
				Count++;
				ShouldInitChild = true;
				continue;
			}
			return Status.Running;
		}
		return Status.Success;
	}
}

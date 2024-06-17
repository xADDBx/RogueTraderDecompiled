namespace Kingmaker.AI.BehaviourTrees.Nodes;

public abstract class TaskNode : BehaviourTreeNode
{
	public TaskNode()
	{
	}

	public TaskNode(string name)
		: base(name)
	{
	}

	protected override void InitInternal()
	{
	}
}

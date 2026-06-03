namespace Kingmaker.AI.BehaviourTrees.Nodes;

public abstract class TaskNode : BehaviourTreeNode
{
	protected TaskNode()
	{
	}

	protected TaskNode(string debugDescription)
		: base(debugDescription)
	{
	}

	protected override void InitInternal()
	{
	}
}

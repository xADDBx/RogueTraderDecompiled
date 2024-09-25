namespace Kingmaker.AI.BehaviourTrees.Nodes;

public abstract class Decorator : BehaviourTreeNode
{
	protected BehaviourTreeNode child;

	public Decorator(BehaviourTreeNode node)
	{
		child = node;
	}

	protected override void InitInternal()
	{
		child.Init();
	}
}

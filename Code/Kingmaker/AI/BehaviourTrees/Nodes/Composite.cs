namespace Kingmaker.AI.BehaviourTrees.Nodes;

public abstract class Composite : BehaviourTreeNode
{
	protected BehaviourTreeNode[] children;

	protected int currentChildIndex;

	public Composite(params BehaviourTreeNode[] nodes)
	{
		children = nodes;
		currentChildIndex = 0;
	}

	protected override void InitInternal()
	{
		currentChildIndex = 0;
		for (int i = 0; i < children.Length; i++)
		{
			children[i].Init();
		}
	}
}

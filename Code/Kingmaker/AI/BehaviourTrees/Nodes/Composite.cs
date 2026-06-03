namespace Kingmaker.AI.BehaviourTrees.Nodes;

public abstract class Composite : BehaviourTreeNode
{
	protected BehaviourTreeNode[] children;

	protected int currentChildIndex;

	public BehaviourTreeNode[] Children => children;

	public Composite(params BehaviourTreeNode[] nodes)
	{
		InitializeNode(nodes);
	}

	public Composite(string debugDescription, params BehaviourTreeNode[] nodes)
		: base(debugDescription)
	{
		InitializeNode(nodes);
	}

	protected override void InitInternal()
	{
		currentChildIndex = 0;
		for (int i = 0; i < children.Length; i++)
		{
			children[i].Init();
		}
	}

	private void InitializeNode(params BehaviourTreeNode[] nodes)
	{
		children = nodes;
		currentChildIndex = 0;
	}
}

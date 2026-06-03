namespace Kingmaker.AI.BehaviourTrees.Nodes;

public abstract class Decorator : BehaviourTreeNode
{
	public BehaviourTreeNode Child { get; }

	public Decorator(BehaviourTreeNode node)
	{
		Child = node;
	}

	public Decorator(string debugDescription, BehaviourTreeNode node)
		: base(debugDescription)
	{
		Child = node;
	}

	protected override void InitInternal()
	{
		Child.Init();
	}
}

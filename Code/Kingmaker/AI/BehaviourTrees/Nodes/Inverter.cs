namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Inverter : Decorator
{
	public Inverter(BehaviourTreeNode node)
		: base(node)
	{
	}

	public Inverter(string debugDescription, BehaviourTreeNode node)
		: base(debugDescription, node)
	{
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		switch (base.Child.Tick(blackboard))
		{
		case Status.Success:
			base.FailReason = "Inverted success child";
			return Status.Failure;
		case Status.Failure:
			return Status.Success;
		default:
			return Status.Running;
		}
	}
}

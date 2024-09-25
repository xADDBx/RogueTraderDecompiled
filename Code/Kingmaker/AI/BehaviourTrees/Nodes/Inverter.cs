namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Inverter : Decorator
{
	public Inverter(BehaviourTreeNode node)
		: base(node)
	{
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		return child.Tick(blackboard) switch
		{
			Status.Success => Status.Failure, 
			Status.Failure => Status.Success, 
			_ => Status.Running, 
		};
	}
}

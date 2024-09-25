using System;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Condition : Decorator
{
	private readonly Func<Blackboard, bool> condition;

	private bool isChecked;

	private bool checkResult;

	private BehaviourTreeNode elseNode;

	public Condition(Func<Blackboard, bool> condition, BehaviourTreeNode node, BehaviourTreeNode elseNode = null)
		: base(node)
	{
		this.condition = condition;
		this.elseNode = elseNode;
	}

	protected override void InitInternal()
	{
		base.InitInternal();
		isChecked = false;
		elseNode?.Init();
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		if (!isChecked)
		{
			isChecked = true;
			checkResult = condition(blackboard);
		}
		if (!checkResult)
		{
			return elseNode?.Tick(blackboard) ?? Status.Failure;
		}
		return child.Tick(blackboard);
	}
}

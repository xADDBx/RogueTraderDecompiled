using System;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Condition : Decorator
{
	private readonly Func<Blackboard, bool> condition;

	public BehaviourTreeNode ElseNode { get; }

	public string ConditionDescription { get; }

	public bool IsChecked { get; private set; }

	public bool CheckResult { get; private set; }

	public Condition(Func<Blackboard, bool> condition, string conditionDescription, BehaviourTreeNode node, BehaviourTreeNode elseNode = null)
		: base(node)
	{
		this.condition = condition;
		ElseNode = elseNode;
		ConditionDescription = conditionDescription;
	}

	public Condition(string debugDescription, Func<Blackboard, bool> condition, string conditionDescription, BehaviourTreeNode node, BehaviourTreeNode elseNode = null)
		: base(debugDescription, node)
	{
		this.condition = condition;
		ElseNode = elseNode;
		ConditionDescription = conditionDescription;
	}

	protected override void InitInternal()
	{
		base.InitInternal();
		IsChecked = false;
		ElseNode?.Init();
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		if (!IsChecked)
		{
			IsChecked = true;
			CheckResult = condition(blackboard);
		}
		Status num = (CheckResult ? base.Child.Tick(blackboard) : (ElseNode?.Tick(blackboard) ?? Status.Failure));
		if (num == Status.Failure)
		{
			if (!CheckResult && ElseNode == null)
			{
				base.FailReason = "Condition is false and no else node is set";
				return num;
			}
			base.FailReason = "Condition child returned Failure";
		}
		return num;
	}
}

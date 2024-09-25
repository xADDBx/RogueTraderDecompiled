using System;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class Loop : Composite
{
	public enum ExitCondition
	{
		NoCondition,
		ExitOnSuccess,
		ExitOnFailure
	}

	private Action<Blackboard> InitializeLoop;

	private Func<Blackboard, bool> NextIteration;

	private BehaviourTreeNode Node;

	private ExitCondition ExitCond;

	private bool IsLoopStarted;

	private bool IterationDone;

	public Loop(Action<Blackboard> initializer, Func<Blackboard, bool> moveNextDelegate, BehaviourTreeNode node, ExitCondition exitCond = ExitCondition.NoCondition)
		: base(node)
	{
		InitializeLoop = initializer;
		NextIteration = moveNextDelegate;
		Node = node;
		ExitCond = exitCond;
	}

	protected override void InitInternal()
	{
		base.InitInternal();
		IterationDone = true;
		IsLoopStarted = false;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		if (!IsLoopStarted)
		{
			InitializeLoop(blackboard);
			IterationDone = true;
			IsLoopStarted = true;
		}
		if (!IterationDone)
		{
			Status status = Node.Tick(blackboard);
			if (status == Status.Running)
			{
				return Status.Running;
			}
			IterationDone = true;
			if ((ExitCond == ExitCondition.ExitOnSuccess && status == Status.Success) || (ExitCond == ExitCondition.ExitOnFailure && status == Status.Failure))
			{
				return Status.Success;
			}
		}
		while (NextIteration(blackboard))
		{
			IterationDone = false;
			Node.Init();
			Status status2 = Node.Tick(blackboard);
			if (status2 == Status.Running)
			{
				return Status.Running;
			}
			IterationDone = true;
			if ((ExitCond == ExitCondition.ExitOnSuccess && status2 == Status.Success) || (ExitCond == ExitCondition.ExitOnFailure && status2 == Status.Failure))
			{
				return Status.Success;
			}
		}
		if (ExitCond != 0)
		{
			return Status.Failure;
		}
		return Status.Success;
	}
}

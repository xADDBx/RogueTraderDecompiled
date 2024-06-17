using System;
using System.Threading.Tasks;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public abstract class AsyncTaskNode : TaskNode
{
	private Status ProcessStatus;

	private Exception CaughtException;

	protected override void InitInternal()
	{
		base.InitInternal();
		ProcessStatus = Status.Unknown;
		CaughtException = null;
	}

	protected sealed override Status TickInternal(Blackboard blackboard)
	{
		if (ProcessStatus == Status.Unknown)
		{
			TickInternalAsync(blackboard);
		}
		if (CaughtException != null)
		{
			throw CaughtException;
		}
		return ProcessStatus;
	}

	protected async void TickInternalAsync(Blackboard blackboard)
	{
		ProcessStatus = Status.Running;
		try
		{
			ProcessStatus = await Process(blackboard);
		}
		catch (Exception caughtException)
		{
			CaughtException = caughtException;
			ProcessStatus = Status.Failure;
		}
	}

	protected abstract Task<Status> Process(Blackboard blackboard);
}

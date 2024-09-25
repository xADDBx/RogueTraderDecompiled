using System;
using System.Threading.Tasks;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class AsyncTaskNodeExecute : AsyncTaskNode
{
	private Func<Blackboard, Task<Status>> asyncTask;

	public AsyncTaskNodeExecute(Func<Blackboard, Task<Status>> asyncTask)
	{
		this.asyncTask = asyncTask;
	}

	protected override Task<Status> Process(Blackboard blackboard)
	{
		return asyncTask(blackboard);
	}
}

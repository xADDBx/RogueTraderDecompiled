using System;
using System.Threading.Tasks;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class AsyncTaskNodeExecute : AsyncTaskNode
{
	private Func<Blackboard, Task<Status>> asyncTask;

	public string Description { get; }

	public AsyncTaskNodeExecute(Func<Blackboard, Task<Status>> asyncTask, string description)
	{
		this.asyncTask = asyncTask;
		Description = description;
	}

	public AsyncTaskNodeExecute(string debugDescription, Func<Blackboard, Task<Status>> asyncTask, string description)
		: base(debugDescription)
	{
		this.asyncTask = asyncTask;
		Description = description;
	}

	protected override Task<Status> Process(Blackboard blackboard)
	{
		return asyncTask(blackboard);
	}
}

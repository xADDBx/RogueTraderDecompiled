using Kingmaker.AI.Profiling;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.Random;

namespace Kingmaker.AI.BehaviourTrees;

public abstract class BehaviourTreeNode
{
	public string id;

	private bool isStarted;

	public Status Status { get; protected set; }

	public string DebugName { get; set; }

	public BehaviourTreeNode()
	{
		DebugName = GetType().Name;
	}

	public BehaviourTreeNode(string name)
	{
		DebugName = name;
	}

	public void Init()
	{
		isStarted = false;
		id = string.Empty;
		InitInternal();
	}

	public Status Tick(Blackboard blackboard)
	{
		using (AIProfileContext.With(this))
		{
			using (ProfileScope.New(DebugName))
			{
				if (isStarted && Status != Status.Running)
				{
					return Status;
				}
				if (!isStarted)
				{
					isStarted = true;
					id = PFUuid.BehaviourTreeNode.CreateString();
					blackboard.Stack.Push(this);
				}
				Status = TickInternal(blackboard);
				if (Status != Status.Running)
				{
					blackboard.Stack.Pop();
				}
				return Status;
			}
		}
	}

	protected abstract void InitInternal();

	protected abstract Status TickInternal(Blackboard blackboard);

	public override string ToString()
	{
		return $"{DebugName} (Status: {Status}, Type: {GetType().Name})";
	}
}

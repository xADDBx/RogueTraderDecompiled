using Kingmaker.AI.DebugUtilities;
using Kingmaker.AI.Profiling;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.Random;

namespace Kingmaker.AI.BehaviourTrees;

public abstract class BehaviourTreeNode
{
	public string id;

	private bool isStarted;

	public string DebugName { get; set; }

	public Status Status { get; private set; }

	public BehaviourTreeNodeBreakpoint Breakpoint { get; } = new BehaviourTreeNodeBreakpoint();


	public string DebugDescription { get; }

	public string FailReason { get; protected set; }

	public BehaviourTreeNode()
	{
		DebugName = GetType().Name;
		DebugDescription = "";
	}

	public BehaviourTreeNode(string debugDescription)
		: this()
	{
		DebugDescription = debugDescription;
	}

	public void Init()
	{
		isStarted = false;
		Status = Status.Unknown;
		Breakpoint.ForceResetActiveness();
		FailReason = string.Empty;
		id = string.Empty;
		InitInternal();
	}

	public Status Tick(Blackboard blackboard)
	{
		using (AIProfileContext.With(this, blackboard))
		{
			using (AIViewerBridgeContext.With(this, blackboard))
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
	}

	protected abstract void InitInternal();

	protected abstract Status TickInternal(Blackboard blackboard);

	public override string ToString()
	{
		return $"{DebugName} (Status: {Status}, Type: {GetType().Name})";
	}
}

using Kingmaker.UnitLogic;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeTryCompleteScenario : TaskNode
{
	public TaskNodeTryCompleteScenario()
	{
	}

	public TaskNodeTryCompleteScenario(string debugDescription)
		: base(debugDescription)
	{
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		PartUnitBrain brain = blackboard.DecisionContext.Unit.Brain;
		if (brain.CurrentScenario != null && brain.CurrentScenario.ShouldComplete())
		{
			brain.CurrentScenario.Complete();
		}
		return Status.Success;
	}
}

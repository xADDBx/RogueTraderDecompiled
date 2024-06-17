using Kingmaker.UnitLogic;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeTryCompleteScenario : TaskNode
{
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

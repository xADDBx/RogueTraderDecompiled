using System.Collections.Generic;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.UnitLogic.Commands;
using Pathfinding;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeCastAbility : CoroutineTaskNode
{
	protected override IEnumerator<Status> CreateCoroutine(Blackboard blackboard)
	{
		DecisionContext context = blackboard.DecisionContext;
		if (context.Ability == null || context.AbilityTarget == null)
		{
			AILogger.Instance.Log(AILogAbility.CastFailed(context.Ability, context.AbilityTarget));
			yield return Status.Failure;
		}
		AILogger.Instance.Log(AILogAbility.Cast(context.Ability, context.AbilityTarget));
		UnitUseAbilityParams cmd = new UnitUseAbilityParams(context.Ability, context.AbilityTarget);
		while (!context.Unit.Brain.IsActingEnabled)
		{
			yield return Status.Running;
		}
		context.Unit.Commands.Run(cmd);
		context.Unit.Brain.IsIdling = false;
		yield return Status.Success;
	}

	private GraphNode GetTargetNode(DecisionContext context)
	{
		if (context.AbilityTarget == null)
		{
			return null;
		}
		return AstarPath.active.GetNearest(context.AbilityTarget.Point).node;
	}
}

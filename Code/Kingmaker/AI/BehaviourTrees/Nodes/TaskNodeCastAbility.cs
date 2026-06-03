using System.Collections.Generic;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Pathfinding;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeCastAbility : CoroutineTaskNode
{
	public TaskNodeCastAbility()
	{
	}

	public TaskNodeCastAbility(string debugDescription)
		: base(debugDescription)
	{
	}

	protected override IEnumerator<Status> CreateCoroutine(Blackboard blackboard)
	{
		DecisionContext context = blackboard.DecisionContext;
		if (context.Ability == null || context.AbilityTarget == null)
		{
			AILogger.Instance.Log(AILogAbility.CastFailed(context.Ability, context.AbilityTarget));
			base.FailReason = "Ability or target was not set";
			yield return Status.Failure;
		}
		AbilityTargetingCache.Instance.SetInactive();
		AILogger.Instance.Log(AILogAbility.Cast(context.Ability, context.AbilityTarget));
		UnitUseAbilityParams cmd = new UnitUseAbilityParams(context.Ability, context.AbilityTarget);
		while (!context.Unit.Brain.IsActingEnabled)
		{
			yield return Status.Running;
		}
		UnitCommandHandle commandHandle = context.Unit.Commands.Run(cmd);
		while (context.Unit.IsBusy)
		{
			yield return Status.Running;
		}
		context.Unit.Brain.IsIdling = false;
		Status status;
		if (commandHandle != null && commandHandle.Result == AbstractUnitCommand.ResultType.Success)
		{
			status = Status.Success;
		}
		else
		{
			base.FailReason = "Command run failed";
			status = Status.Failure;
		}
		yield return status;
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

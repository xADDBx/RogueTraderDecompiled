using Kingmaker.AI.DebugUtilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeSelectReferenceAbilityTarget : TaskNode
{
	protected override Status TickInternal(Blackboard blackboard)
	{
		DecisionContext decisionContext = blackboard.DecisionContext;
		decisionContext.AbilityTarget = null;
		AbilityData ability = decisionContext.Ability;
		TargetWrapper targetWrapper = SelectAbilityTarget(decisionContext, ability);
		if (targetWrapper != null)
		{
			decisionContext.AbilityTarget = targetWrapper;
			AILogger.Instance.Log(AILogAbility.TargetFound(CastTimepointType.None, ability, targetWrapper));
		}
		else
		{
			AILogger.Instance.Log(AILogAbility.TargetNotFound(CastTimepointType.None, ability));
		}
		return Status.Success;
	}

	private TargetWrapper SelectAbilityTarget(DecisionContext context, AbilityData ability)
	{
		return new AbilityInfo(ability).GetAbilityTargetSelector().SelectTarget(context, context.UnitNode);
	}
}

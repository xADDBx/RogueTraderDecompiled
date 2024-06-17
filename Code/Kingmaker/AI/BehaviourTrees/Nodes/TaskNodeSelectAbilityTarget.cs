using System.Collections.Generic;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeSelectAbilityTarget : TaskNode
{
	private readonly CastTimepointType m_CastTimepoint;

	public TaskNodeSelectAbilityTarget(CastTimepointType castTimepoint)
	{
		m_CastTimepoint = castTimepoint;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		AILogger.Instance.Log(AILogAbility.SelectAbility(m_CastTimepoint));
		DecisionContext decisionContext = blackboard.DecisionContext;
		decisionContext.AbilityTarget = null;
		List<AbilityData> sortedAbilityList = decisionContext.GetSortedAbilityList(m_CastTimepoint);
		if (sortedAbilityList != null)
		{
			foreach (AbilityData item in sortedAbilityList)
			{
				TargetWrapper targetWrapper = SelectAbilityTarget(decisionContext, item);
				if (targetWrapper != null)
				{
					decisionContext.Ability = item;
					decisionContext.AbilityTarget = targetWrapper;
					AILogger.Instance.Log(AILogAbility.TargetFound(m_CastTimepoint, item, targetWrapper));
					return Status.Success;
				}
				AILogger.Instance.Log(AILogAbility.TargetNotFound(m_CastTimepoint, item));
			}
		}
		AILogger.Instance.Log(AILogAbility.AbilityNotSelected(m_CastTimepoint));
		return Status.Failure;
	}

	private TargetWrapper SelectAbilityTarget(DecisionContext context, AbilityData ability)
	{
		return new AbilityInfo(ability).GetAbilityTargetSelector().SelectTarget(context, context.UnitNode);
	}
}

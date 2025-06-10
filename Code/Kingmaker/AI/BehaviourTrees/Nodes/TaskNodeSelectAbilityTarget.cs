using System.Collections.Generic;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeSelectAbilityTarget : TaskNode
{
	private readonly CastTimepointType m_CastTimepoint;

	private readonly bool m_TryTargetAllEnemies;

	public TaskNodeSelectAbilityTarget(CastTimepointType castTimepoint, bool tryTargetAllEnemies = false)
	{
		m_CastTimepoint = castTimepoint;
		m_TryTargetAllEnemies = tryTargetAllEnemies;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		AILogger.Instance.Log(AILogAbility.SelectAbility(m_CastTimepoint));
		DecisionContext decisionContext = blackboard.DecisionContext;
		decisionContext.AbilityTarget = null;
		decisionContext.TryTargetAllInsteadOfHatedOnly = m_TryTargetAllEnemies;
		List<AbilityData> sortedAbilityList = decisionContext.GetSortedAbilityList(m_CastTimepoint);
		if (m_CastTimepoint == CastTimepointType.BeforeMove && (bool)decisionContext.Unit.Brain.TryActionBeforeMove)
		{
			sortedAbilityList = decisionContext.GetSortedAbilityList(CastTimepointType.Any);
		}
		if (sortedAbilityList != null)
		{
			foreach (AbilityData item in sortedAbilityList)
			{
				TargetWrapper targetWrapper = SelectAbilityTarget(decisionContext, item);
				if (targetWrapper != null)
				{
					decisionContext.Ability = item;
					decisionContext.AbilityTarget = targetWrapper;
					decisionContext.TryTargetAllInsteadOfHatedOnly = false;
					AILogger.Instance.Log(AILogAbility.TargetFound(m_CastTimepoint, item, targetWrapper));
					return Status.Success;
				}
				AILogger.Instance.Log(AILogAbility.TargetNotFound(m_CastTimepoint, item));
			}
		}
		decisionContext.TryTargetAllInsteadOfHatedOnly = false;
		AILogger.Instance.Log(AILogAbility.AbilityNotSelected(m_CastTimepoint));
		return Status.Failure;
	}

	private TargetWrapper SelectAbilityTarget(DecisionContext context, AbilityData ability)
	{
		return new AbilityInfo(ability).GetAbilityTargetSelector().SelectTarget(context, context.UnitNode);
	}
}

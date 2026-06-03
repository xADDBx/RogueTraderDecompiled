using System.Collections.Generic;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeSelectAbilityTarget : TaskNode
{
	public CastTimepointType CastTimepoint { get; }

	public bool TryTargetAllEnemies { get; }

	public TaskNodeSelectAbilityTarget(CastTimepointType castTimepoint, bool tryTargetAllEnemies = false)
	{
		CastTimepoint = castTimepoint;
		TryTargetAllEnemies = tryTargetAllEnemies;
	}

	public TaskNodeSelectAbilityTarget(string debugDescription, CastTimepointType castTimepoint, bool tryTargetAllEnemies = false)
		: base(debugDescription)
	{
		CastTimepoint = castTimepoint;
		TryTargetAllEnemies = tryTargetAllEnemies;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		AILogger.Instance.Log(AILogAbility.SelectAbility(CastTimepoint));
		DecisionContext decisionContext = blackboard.DecisionContext;
		decisionContext.AbilityTarget = null;
		decisionContext.TryTargetAllInsteadOfHatedOnly = TryTargetAllEnemies;
		List<AbilityData> sortedAbilityList = decisionContext.GetSortedAbilityList(CastTimepoint);
		if (CastTimepoint == CastTimepointType.BeforeMove && (bool)decisionContext.Unit.Brain.TryActionBeforeMove)
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
					AILogger.Instance.Log(AILogAbility.TargetFound(CastTimepoint, item, targetWrapper));
					return Status.Success;
				}
				AILogger.Instance.Log(AILogAbility.TargetNotFound(CastTimepoint, item));
			}
		}
		decisionContext.TryTargetAllInsteadOfHatedOnly = false;
		AILogger.Instance.Log(AILogAbility.AbilityNotSelected(CastTimepoint));
		base.FailReason = "No ability target was found for any of abilities";
		return Status.Failure;
	}

	private TargetWrapper SelectAbilityTarget(DecisionContext context, AbilityData ability)
	{
		return new AbilityInfo(ability).GetAbilityTargetSelector().SelectTarget(context, context.UnitNode);
	}
}

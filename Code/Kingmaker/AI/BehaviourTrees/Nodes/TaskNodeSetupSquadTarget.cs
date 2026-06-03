using System.Linq;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Squads;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeSetupSquadTarget : TaskNode
{
	public TaskNodeSetupSquadTarget()
	{
	}

	public TaskNodeSetupSquadTarget(string debugDescription)
		: base(debugDescription)
	{
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		DecisionContext decisionContext = blackboard.DecisionContext;
		if (decisionContext.AbilityTarget == null)
		{
			AbilityData abilityData = decisionContext.ConsideringAbility ?? decisionContext.Ability;
			if (abilityData == null)
			{
				AILogger.Instance.Log(new AILogReason(AILogReasonType.AbilityForSquadTargetNotFound));
				base.FailReason = "No ability for squad target";
				return Status.Failure;
			}
			AbilityTargetSelector abilityTargetSelector = new AbilityInfo(abilityData).GetAbilityTargetSelector();
			CustomGridNodeBase casterNode = ((decisionContext.MoveCommand != null) ? ((CustomGridNodeBase)decisionContext.MoveCommand.ForcedPath.path.Last()) : decisionContext.UnitNode);
			decisionContext.AbilityTarget = abilityTargetSelector.SelectTarget(decisionContext, casterNode);
		}
		decisionContext.Unit.GetSquadOptional().Squad.CommonTarget = decisionContext.AbilityTarget;
		return Status.Success;
	}
}

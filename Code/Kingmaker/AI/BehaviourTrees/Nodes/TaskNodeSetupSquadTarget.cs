using System.Linq;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Squads;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeSetupSquadTarget : TaskNode
{
	protected override Status TickInternal(Blackboard blackboard)
	{
		DecisionContext decisionContext = blackboard.DecisionContext;
		if (decisionContext.AbilityTarget == null)
		{
			AbilityTargetSelector abilityTargetSelector = new AbilityInfo(decisionContext.ConsideringAbility ?? decisionContext.Ability).GetAbilityTargetSelector();
			CustomGridNodeBase casterNode = ((decisionContext.MoveCommand != null) ? ((CustomGridNodeBase)decisionContext.MoveCommand.ForcedPath.path.Last()) : decisionContext.UnitNode);
			decisionContext.AbilityTarget = abilityTargetSelector.SelectTarget(decisionContext, casterNode);
		}
		decisionContext.Unit.GetSquadOptional().Squad.CommonTarget = decisionContext.AbilityTarget;
		return Status.Success;
	}
}

using System.Linq;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeSelectReferenceAbility : TaskNode
{
	protected override Status TickInternal(Blackboard blackboard)
	{
		DecisionContext decisionContext = blackboard.DecisionContext;
		decisionContext.Ability = decisionContext.GetSortedMovementInfluentAbilities().FirstOrDefault();
		decisionContext.IsMovementInfluentAbility = decisionContext.Ability != null;
		DecisionContext decisionContext2 = decisionContext;
		if ((object)decisionContext2.Ability == null)
		{
			AbilityData abilityData = (decisionContext2.Ability = decisionContext.GetBestAbility());
		}
		AbilityTargetingCache.Instance.SetActive();
		AILogger.Instance.Log(AILogAbility.ReferenceAbilitySelected(decisionContext.Ability));
		return Status.Success;
	}
}

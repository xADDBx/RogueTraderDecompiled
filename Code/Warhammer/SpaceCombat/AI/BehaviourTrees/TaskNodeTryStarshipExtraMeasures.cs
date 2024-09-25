using System;
using Kingmaker.AI;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;

namespace Warhammer.SpaceCombat.AI.BehaviourTrees;

public class TaskNodeTryStarshipExtraMeasures : TaskNode
{
	private class LogMessage : AILogObject
	{
		private enum Type
		{
			TryUseExtraMeasures,
			TryUseAbility,
			CantUseAbility
		}

		private readonly Type type;

		private readonly AbilityData ability;

		public static LogMessage TryUseExtraMeasures()
		{
			return new LogMessage(Type.TryUseExtraMeasures);
		}

		public static LogMessage TryUseAbility(AbilityData ability)
		{
			return new LogMessage(Type.TryUseAbility, ability);
		}

		public static LogMessage CantUseAbility(AbilityData ability)
		{
			return new LogMessage(Type.CantUseAbility, ability);
		}

		private LogMessage(Type type, AbilityData ability = null)
		{
			this.type = type;
			this.ability = ability;
		}

		public override string GetLogString()
		{
			return type switch
			{
				Type.TryUseExtraMeasures => "Trajectory with good enough score wasn't found, try use extra measures", 
				Type.TryUseAbility => $"Try use extra measure: {ability}", 
				Type.CantUseAbility => $"Can't use extra measure: {ability}", 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		AILogger.Instance.Log(LogMessage.TryUseExtraMeasures());
		SpaceCombatDecisionContext spaceCombatDecisionContext = (SpaceCombatDecisionContext)blackboard.DecisionContext;
		foreach (BlueprintAbility extraMeasure in (spaceCombatDecisionContext.Unit.Brain.Blueprint as BlueprintStarshipBrain).ExtraMeasures)
		{
			Ability ability = spaceCombatDecisionContext.Unit.Abilities.GetAbility(extraMeasure);
			if (!spaceCombatDecisionContext.IsUsableAbility(ability))
			{
				AILogger.Instance.Log(LogMessage.CantUseAbility(ability.Data));
				continue;
			}
			foreach (TargetInfo availableTarget in spaceCombatDecisionContext.GetAvailableTargets(ability.Data))
			{
				if (UseExtraMeasure(spaceCombatDecisionContext, ability, new TargetWrapper(availableTarget.Entity)))
				{
					return Status.Success;
				}
			}
		}
		AILogger.Instance.Log(new AILogReason(AILogReasonType.CantUseExtraMeasures));
		return Status.Failure;
	}

	private bool UseExtraMeasure(SpaceCombatDecisionContext context, Ability ability, TargetWrapper target)
	{
		if (!ability.Data.CanTarget(target, out var unavailableReason))
		{
			AILogger.Instance.Log(AILogAbility.CantTargetWithAbility(ability.Data, target, unavailableReason));
			return false;
		}
		AILogger.Instance.Log(LogMessage.TryUseAbility(ability.Data));
		UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(ability.Data, target);
		context.Unit.Commands.Run(cmdParams);
		context.Unit.Brain.IsIdling = false;
		return true;
	}
}

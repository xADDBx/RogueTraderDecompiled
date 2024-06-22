using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.PostAddPatterns;

public class PatternPostAddEventSwitchApplyBuffAndDependAbility : PatternPostAddEvent
{
	public static PatternPostAddEvent Create()
	{
		return new PatternPostAddEventSwitchApplyBuffAndDependAbility();
	}

	private PatternPostAddEventSwitchApplyBuffAndDependAbility()
	{
	}

	protected override void ApplyImpl(List<GameLogEvent> queue, GameLogEvent @event)
	{
		if (!(@event is GameLogRuleEvent<RulePerformAbility> gameLogRuleEvent))
		{
			return;
		}
		MechanicsContext executionActionContext = gameLogRuleEvent.Rule.ExecutionActionContext;
		if (executionActionContext == null)
		{
			return;
		}
		BlueprintScriptableObject associatedBlueprint = executionActionContext.AssociatedBlueprint;
		BlueprintBuff blueprintBuff = associatedBlueprint as BlueprintBuff;
		if (blueprintBuff != null)
		{
			int num = queue.FindLastIndex((GameLogEvent evn) => evn is GameLogRuleEvent<RuleCalculateCanApplyBuff> gameLogRuleEvent2 && gameLogRuleEvent2.Rule.Blueprint == blueprintBuff);
			if (num != -1)
			{
				queue.Insert(num, @event);
			}
		}
	}
}

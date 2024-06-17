using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public class SwitchApplyBuffAndDependAbilityPattern : IPattern
{
	public static IPattern Create()
	{
		return new SwitchApplyBuffAndDependAbilityPattern();
	}

	private SwitchApplyBuffAndDependAbilityPattern()
	{
	}

	public bool Apply(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		if (@event is GameLogRuleEvent<RulePerformAbility> gameLogRuleEvent)
		{
			MechanicsContext executionActionContext = gameLogRuleEvent.Rule.ExecutionActionContext;
			if (executionActionContext != null)
			{
				BlueprintScriptableObject associatedBlueprint = executionActionContext.AssociatedBlueprint;
				BlueprintBuff blueprintBuff = associatedBlueprint as BlueprintBuff;
				if (blueprintBuff != null)
				{
					int num = eventsQueue.FindLastIndex((GameLogEvent evn) => evn is GameLogRuleEvent<RuleCalculateCanApplyBuff> gameLogRuleEvent2 && gameLogRuleEvent2.Rule.Blueprint == blueprintBuff);
					if (num != -1)
					{
						eventsQueue.Insert(num, @event);
						return true;
					}
				}
			}
		}
		return false;
	}
}

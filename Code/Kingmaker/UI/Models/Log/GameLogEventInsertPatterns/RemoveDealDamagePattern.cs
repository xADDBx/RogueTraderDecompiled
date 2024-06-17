using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public class RemoveDealDamagePattern : IPattern
{
	public static IPattern Create()
	{
		return new RemoveDealDamagePattern();
	}

	private RemoveDealDamagePattern()
	{
	}

	public bool Apply(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		if (@event is GameLogEventAttack)
		{
			int num = eventsQueue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent && gameLogRuleEvent.ParentEvent == @event);
			if (num != -1)
			{
				eventsQueue.RemoveAt(num);
			}
		}
		return false;
	}
}

using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public class InsertSeparatorPattern : IPattern
{
	public static IPattern Create()
	{
		return new InsertSeparatorPattern();
	}

	private InsertSeparatorPattern()
	{
	}

	bool IPattern.Apply(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		if (@event is GameLogEventAddSeparator { State: GameLogEventAddSeparator.States.Start })
		{
			int num = eventsQueue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RulePerformAbility>);
			if (num != -1)
			{
				eventsQueue.Insert(num, @event);
				return true;
			}
		}
		return false;
	}
}

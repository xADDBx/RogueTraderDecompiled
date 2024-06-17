using JetBrains.Annotations;
using Kingmaker.RuleSystem;

namespace Kingmaker.UI.Models.Log.Events;

public static class GameLogEventHelper
{
	[CanBeNull]
	public static GameLogRuleEvent AsRuleEvent(this GameLogEvent evt)
	{
		return evt as GameLogRuleEvent;
	}

	[CanBeNull]
	public static GameLogRuleEvent<T> AsRuleEvent<T>(this GameLogEvent evt) where T : RulebookEvent
	{
		return evt as GameLogRuleEvent<T>;
	}

	public static bool IsRuleEvent(this GameLogEvent evt)
	{
		return evt.AsRuleEvent() != null;
	}

	public static bool IsRuleEvent<T>(this GameLogEvent evt) where T : RulebookEvent
	{
		return evt.AsRuleEvent<T>() != null;
	}
}

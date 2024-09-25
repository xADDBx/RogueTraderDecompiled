using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent.Comparers;

public class PerformSavingThrowComparer : IMergeEventComparer
{
	public static IMergeEventComparer Create()
	{
		return new PerformSavingThrowComparer();
	}

	private PerformSavingThrowComparer()
	{
	}

	public bool Compare(GameLogEvent evn1, GameLogEvent evn2)
	{
		if (evn1.ParentEvent == evn2.ParentEvent && !(evn1.ParentEvent is GameLogEventAbility { IsScatter: not false }) && evn1 is GameLogRuleEvent<RulePerformSavingThrow> gameLogRuleEvent && evn2 is GameLogRuleEvent<RulePerformSavingThrow> gameLogRuleEvent2 && gameLogRuleEvent.Rule.Type == gameLogRuleEvent2.Rule.Type)
		{
			return gameLogRuleEvent.Rule.StatType == gameLogRuleEvent2.Rule.StatType;
		}
		return false;
	}
}

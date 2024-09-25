using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent.Comparers;

public class CalculateCanApplyBuffComparer : IMergeEventComparer
{
	public static IMergeEventComparer Create()
	{
		return new CalculateCanApplyBuffComparer();
	}

	private CalculateCanApplyBuffComparer()
	{
	}

	public bool Compare(GameLogEvent evn1, GameLogEvent evn2)
	{
		if (evn1.ParentEvent == evn2.ParentEvent && evn1 is GameLogRuleEvent<RuleCalculateCanApplyBuff> gameLogRuleEvent && evn2 is GameLogRuleEvent<RuleCalculateCanApplyBuff> gameLogRuleEvent2 && !gameLogRuleEvent.Rule.AppliedBuff.IsTraumas && !gameLogRuleEvent.Rule.AppliedBuff.IsWounds && !gameLogRuleEvent2.Rule.AppliedBuff.IsTraumas && !gameLogRuleEvent2.Rule.AppliedBuff.IsWounds && gameLogRuleEvent.Rule.AppliedBuff.Blueprint == gameLogRuleEvent2.Rule.AppliedBuff.Blueprint)
		{
			return gameLogRuleEvent.Rule.AppliedBuff.Context.MaybeCaster == gameLogRuleEvent2.Rule.AppliedBuff.Context.MaybeCaster;
		}
		return false;
	}
}

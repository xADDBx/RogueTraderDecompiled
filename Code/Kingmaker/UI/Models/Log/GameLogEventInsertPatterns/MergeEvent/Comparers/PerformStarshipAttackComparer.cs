using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent.Comparers;

public class PerformStarshipAttackComparer : IMergeEventComparer
{
	public static IMergeEventComparer Create()
	{
		return new PerformStarshipAttackComparer();
	}

	private PerformStarshipAttackComparer()
	{
	}

	public bool Compare(GameLogEvent evn1, GameLogEvent evn2)
	{
		if (evn1.ParentEvent == evn2.ParentEvent && evn1 is GameLogRuleEvent<RuleStarshipPerformAttack> gameLogRuleEvent && evn2 is GameLogRuleEvent<RuleStarshipPerformAttack> gameLogRuleEvent2)
		{
			return gameLogRuleEvent.Rule.Target == gameLogRuleEvent2.Rule.Target;
		}
		return false;
	}
}

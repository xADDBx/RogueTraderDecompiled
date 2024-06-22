using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.AddPatterns;

public class PatternAddEventConvertToEventScatterDealDamage : PatternAddEvent
{
	public static PatternAddEvent Create()
	{
		return new PatternAddEventConvertToEventScatterDealDamage();
	}

	private PatternAddEventConvertToEventScatterDealDamage()
	{
	}

	protected override bool TryApplyImpl(List<GameLogEvent> queue, GameLogEvent @in, out GameLogEvent @out)
	{
		GameLogRuleEvent<RuleDealDamage> dealDamage = @in as GameLogRuleEvent<RuleDealDamage>;
		if (dealDamage != null && @in.ParentEvent is GameLogEventAbility { IsScatter: not false })
		{
			@out = GameLogEventDealDamage.Create(dealDamage.Rule);
			int num = queue.FindLastIndex((GameLogEvent o) => o is GameLogEventAttack gameLogEventAttack && gameLogEventAttack.ParentEvent == dealDamage.ParentEvent);
			if (num != -1)
			{
				num++;
				queue.Insert(num, @out);
				int num2 = queue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RulePerformSavingThrow> gameLogRuleEvent && gameLogRuleEvent.ParentEvent == dealDamage.ParentEvent);
				if (num2 != -1 && (num2 == queue.Count - 1 || !(queue[num2 + 1] is GameLogEventDealDamage)))
				{
					GameLogEvent item = queue[num2];
					queue.RemoveAt(num2);
					queue.Insert(num, item);
				}
			}
			else
			{
				queue.Add(@out);
			}
			return true;
		}
		@out = null;
		return false;
	}
}

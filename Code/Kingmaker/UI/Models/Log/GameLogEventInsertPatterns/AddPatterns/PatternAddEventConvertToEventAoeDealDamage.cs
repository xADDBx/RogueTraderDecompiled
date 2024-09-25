using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.AddPatterns;

public class PatternAddEventConvertToEventAoeDealDamage : PatternAddEvent
{
	public static PatternAddEvent Create()
	{
		return new PatternAddEventConvertToEventAoeDealDamage();
	}

	private PatternAddEventConvertToEventAoeDealDamage()
	{
	}

	protected override bool TryApplyImpl(List<GameLogEvent> queue, GameLogEvent @in, out GameLogEvent @out)
	{
		GameLogRuleEvent<RuleDealDamage> dealDamage = @in as GameLogRuleEvent<RuleDealDamage>;
		if (dealDamage != null && @in.ParentEvent is GameLogEventAbility { IsAoe: not false })
		{
			@out = GameLogEventDealDamage.Create(dealDamage.Rule);
			int num = queue.FindLastIndex((GameLogEvent o) => o is GameLogEventAttack gameLogEventAttack && gameLogEventAttack.ParentEvent == dealDamage.ParentEvent);
			if (num != -1)
			{
				num++;
				queue.Insert(num, @out);
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

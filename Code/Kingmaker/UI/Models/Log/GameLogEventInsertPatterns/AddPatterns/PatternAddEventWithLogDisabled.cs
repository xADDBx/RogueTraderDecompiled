using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.AddPatterns;

public class PatternAddEventWithLogDisabled : PatternAddEvent
{
	public static PatternAddEvent Create()
	{
		return new PatternAddEventWithLogDisabled();
	}

	private PatternAddEventWithLogDisabled()
	{
	}

	protected override bool TryApplyImpl(List<GameLogEvent> queue, GameLogEvent @in, out GameLogEvent @out)
	{
		if (@in is GameLogRuleEvent<RuleDealDamage> && @in.ParentEvent is GameLogEventAbility gameLogEventAbility && gameLogEventAbility.Context.DisableLog)
		{
			@out = @in;
			return true;
		}
		@out = null;
		return false;
	}
}

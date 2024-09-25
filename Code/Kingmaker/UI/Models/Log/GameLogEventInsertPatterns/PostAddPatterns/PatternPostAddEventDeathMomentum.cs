using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.PostAddPatterns;

public class PatternPostAddEventDeathMomentum : PatternPostAddEvent
{
	public static PatternPostAddEvent Create()
	{
		return new PatternPostAddEventDeathMomentum();
	}

	private PatternPostAddEventDeathMomentum()
	{
	}

	protected override void ApplyImpl(List<GameLogEvent> queue, GameLogEvent @event)
	{
		GameLogEventLifeStateChanged eventLifeStateChanged = @event as GameLogEventLifeStateChanged;
		if (eventLifeStateChanged == null || !eventLifeStateChanged.Unit.LifeState.IsDead)
		{
			return;
		}
		int num = queue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RulePerformMomentumChange> gameLogRuleEvent2 && gameLogRuleEvent2.Rule.ChangeReason == MomentumChangeReason.KillEnemy && gameLogRuleEvent2.Rule.MaybeTarget == eventLifeStateChanged.Unit);
		if (num != -1)
		{
			GameLogEvent item = queue[num];
			queue.RemoveAt(num);
			queue.Remove(@event);
			num = queue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent && gameLogRuleEvent.Rule.ConcreteTarget == eventLifeStateChanged.Unit);
			if (num != -1 && num + 1 < queue.Count)
			{
				num++;
				queue.Insert(num, item);
				queue.Insert(num, @event);
			}
			else
			{
				queue.Add(@event);
				queue.Add(item);
			}
		}
	}
}

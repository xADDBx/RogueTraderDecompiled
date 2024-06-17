using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public class DeathMomentumPattern : IPattern
{
	public static IPattern Create()
	{
		return new DeathMomentumPattern();
	}

	private DeathMomentumPattern()
	{
	}

	public bool Apply(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		GameLogEventLifeStateChanged eventLifeStateChanged = @event as GameLogEventLifeStateChanged;
		if (eventLifeStateChanged != null && eventLifeStateChanged.Unit.LifeState.IsDead)
		{
			int num = eventsQueue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RulePerformMomentumChange> gameLogRuleEvent2 && gameLogRuleEvent2.Rule.ChangeReason == MomentumChangeReason.KillEnemy && gameLogRuleEvent2.Rule.MaybeTarget == eventLifeStateChanged.Unit);
			if (num != -1)
			{
				GameLogEvent item = eventsQueue[num];
				eventsQueue.RemoveAt(num);
				num = eventsQueue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent && gameLogRuleEvent.Rule.ConcreteTarget == eventLifeStateChanged.Unit);
				if (num != -1 && num + 1 < eventsQueue.Count)
				{
					num++;
					eventsQueue.Insert(num, item);
					eventsQueue.Insert(num, @event);
				}
				else
				{
					eventsQueue.Add(@event);
					eventsQueue.Add(item);
				}
				return true;
			}
		}
		return false;
	}
}

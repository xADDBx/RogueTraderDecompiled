using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public class ConvertToEventScatterDealDamage : IPattern
{
	public static IPattern Create()
	{
		return new ConvertToEventScatterDealDamage();
	}

	private ConvertToEventScatterDealDamage()
	{
	}

	public bool Apply(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		GameLogRuleEvent<RuleDealDamage> dealDamage = @event as GameLogRuleEvent<RuleDealDamage>;
		if (dealDamage != null && @event.ParentEvent is GameLogEventAbility { IsScatter: not false })
		{
			GameLogEventDealDamage item = GameLogEventDealDamage.Create(dealDamage.Rule);
			int num = eventsQueue.FindLastIndex((GameLogEvent o) => o is GameLogEventAttack gameLogEventAttack && gameLogEventAttack.ParentEvent == dealDamage.ParentEvent);
			if (num != -1)
			{
				num++;
				eventsQueue.Insert(num, item);
				int num2 = eventsQueue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RulePerformSavingThrow> gameLogRuleEvent && gameLogRuleEvent.ParentEvent == dealDamage.ParentEvent);
				if (num2 != -1 && (num2 == eventsQueue.Count - 1 || !(eventsQueue[num2 + 1] is GameLogEventDealDamage)))
				{
					GameLogEvent item2 = eventsQueue[num2];
					eventsQueue.RemoveAt(num2);
					eventsQueue.Insert(num, item2);
				}
			}
			else
			{
				eventsQueue.Add(item);
			}
			return true;
		}
		return false;
	}
}

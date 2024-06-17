using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public class ConvertToEventAoeDealDamage : IPattern
{
	public static IPattern Create()
	{
		return new ConvertToEventAoeDealDamage();
	}

	private ConvertToEventAoeDealDamage()
	{
	}

	public bool Apply(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		GameLogRuleEvent<RuleDealDamage> dealDamage = @event as GameLogRuleEvent<RuleDealDamage>;
		if (dealDamage != null && @event.ParentEvent is GameLogEventAbility { IsAoe: not false })
		{
			GameLogEventDealDamage item = GameLogEventDealDamage.Create(dealDamage.Rule);
			int num = eventsQueue.FindLastIndex((GameLogEvent o) => o is GameLogEventAttack gameLogEventAttack && gameLogEventAttack.ParentEvent == dealDamage.ParentEvent);
			if (num != -1)
			{
				num++;
				eventsQueue.Insert(num, item);
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

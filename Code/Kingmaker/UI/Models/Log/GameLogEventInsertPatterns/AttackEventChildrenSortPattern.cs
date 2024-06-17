using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public class AttackEventChildrenSortPattern : IPattern
{
	private Stack<GameLogEvent> m_Stack;

	public static IPattern Create()
	{
		return new AttackEventChildrenSortPattern();
	}

	private AttackEventChildrenSortPattern()
	{
	}

	bool IPattern.Apply(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		if (@event is GameLogEventAttack)
		{
			int num = eventsQueue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent && gameLogRuleEvent.ParentEvent == @event);
			if (num != -1)
			{
				if (m_Stack == null)
				{
					m_Stack = new Stack<GameLogEvent>();
				}
				m_Stack.Clear();
				GameLogEvent gameLogEvent = eventsQueue[num];
				eventsQueue.RemoveAt(num);
				int num2 = num - 1;
				while (num2 >= 0 && eventsQueue[num2].ParentEvent == gameLogEvent)
				{
					m_Stack.Push(eventsQueue[num2]);
					eventsQueue.RemoveAt(num2);
					num2--;
				}
				m_Stack.Push(gameLogEvent);
				eventsQueue.Add(@event);
				while (m_Stack.Count > 0)
				{
					eventsQueue.Add(m_Stack.Pop());
				}
				return true;
			}
		}
		return false;
	}
}

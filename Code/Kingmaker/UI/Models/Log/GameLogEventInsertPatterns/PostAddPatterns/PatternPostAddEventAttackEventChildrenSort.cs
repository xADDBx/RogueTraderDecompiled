using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.PostAddPatterns;

public class PatternPostAddEventAttackEventChildrenSort : PatternPostAddEvent
{
	private Queue<GameLogEvent> m_Queue;

	public static PatternPostAddEvent Create()
	{
		return new PatternPostAddEventAttackEventChildrenSort();
	}

	private PatternPostAddEventAttackEventChildrenSort()
	{
	}

	protected override void ApplyImpl(List<GameLogEvent> queue, GameLogEvent @event)
	{
		if (!(@event is GameLogEventAttack))
		{
			return;
		}
		int num = queue.FindLastIndex((GameLogEvent o) => o is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent && gameLogRuleEvent.ParentEvent == @event);
		if (num != -1)
		{
			if (m_Queue == null)
			{
				m_Queue = new Queue<GameLogEvent>();
			}
			m_Queue.Clear();
			GameLogEvent gameLogEvent = queue[num];
			queue.RemoveAt(num);
			int num2 = num - 1;
			while (num2 >= 0 && queue[num2].ParentEvent == gameLogEvent)
			{
				m_Queue.Enqueue(queue[num2]);
				queue.RemoveAt(num2);
				num2--;
			}
			m_Queue.Enqueue(gameLogEvent);
			num = queue.IndexOf(@event) + 1;
			while (m_Queue.Count > 0)
			{
				queue.Insert(num, m_Queue.Dequeue());
			}
		}
	}
}

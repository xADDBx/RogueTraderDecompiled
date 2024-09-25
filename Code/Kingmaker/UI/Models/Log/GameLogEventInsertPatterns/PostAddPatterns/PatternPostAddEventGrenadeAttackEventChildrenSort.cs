using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.PostAddPatterns;

public class PatternPostAddEventGrenadeAttackEventChildrenSort : PatternPostAddEvent
{
	private Queue<GameLogEvent> m_Queue;

	public static PatternPostAddEvent Create()
	{
		return new PatternPostAddEventGrenadeAttackEventChildrenSort();
	}

	private PatternPostAddEventGrenadeAttackEventChildrenSort()
	{
	}

	protected override void ApplyImpl(List<GameLogEvent> queue, GameLogEvent @event)
	{
		if (@event is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent && gameLogRuleEvent.Rule.SourceAbility != null && gameLogRuleEvent.Rule.SourceAbility.Blueprint.AbilityTag == AbilityTag.ThrowingGrenade)
		{
			if (m_Queue == null)
			{
				m_Queue = new Queue<GameLogEvent>();
			}
			m_Queue.Clear();
			int num = queue.Count - 1;
			while (num >= 0 && queue[num].ParentEvent == gameLogRuleEvent)
			{
				m_Queue.Enqueue(queue[num]);
				queue.RemoveAt(num);
				num--;
			}
			int index = queue.IndexOf(@event) + 1;
			while (m_Queue.Count > 0)
			{
				queue.Insert(index, m_Queue.Dequeue());
			}
		}
	}
}

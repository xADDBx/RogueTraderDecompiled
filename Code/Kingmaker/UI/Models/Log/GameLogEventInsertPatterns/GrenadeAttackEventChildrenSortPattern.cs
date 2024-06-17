using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public class GrenadeAttackEventChildrenSortPattern : IPattern
{
	private Stack<GameLogEvent> m_Stack;

	public static IPattern Create()
	{
		return new GrenadeAttackEventChildrenSortPattern();
	}

	private GrenadeAttackEventChildrenSortPattern()
	{
	}

	public bool Apply(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		if (@event is GameLogRuleEvent<RuleDealDamage> gameLogRuleEvent && gameLogRuleEvent.Rule.SourceAbility != null && gameLogRuleEvent.Rule.SourceAbility.Blueprint.AbilityTag == AbilityTag.ThrowingGrenade)
		{
			if (m_Stack == null)
			{
				m_Stack = new Stack<GameLogEvent>();
			}
			m_Stack.Clear();
			int num = eventsQueue.Count - 1;
			while (num >= 0 && eventsQueue[num].ParentEvent == gameLogRuleEvent)
			{
				m_Stack.Push(eventsQueue[num]);
				eventsQueue.RemoveAt(num);
				num--;
			}
			eventsQueue.Add(@event);
			while (m_Stack.Count > 0)
			{
				eventsQueue.Add(m_Stack.Pop());
			}
			return true;
		}
		return false;
	}
}

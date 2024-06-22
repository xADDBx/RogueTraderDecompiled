using System.Collections.Generic;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.AddPatterns;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.PostAddPatterns;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public class PatternCollection : IPatternCollection
{
	private readonly List<PatternAddEvent> m_AddPatterns = new List<PatternAddEvent>();

	private readonly List<PatternPostAddEvent> m_PostAddPatterns = new List<PatternPostAddEvent>();

	private static IPatternCollection s_Instance;

	public static IPatternCollection Instance => s_Instance ?? (s_Instance = new PatternCollection());

	IPatternCollection IPatternCollection.AddPattern(PatternAddEvent pattern)
	{
		if (!m_AddPatterns.Contains(pattern))
		{
			m_AddPatterns.Add(pattern);
		}
		return this;
	}

	IPatternCollection IPatternCollection.AddPattern(PatternPostAddEvent pattern)
	{
		if (!m_PostAddPatterns.Contains(pattern))
		{
			m_PostAddPatterns.Add(pattern);
		}
		return this;
	}

	void IPatternCollection.ApplyPatterns(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		if (@event == null)
		{
			return;
		}
		bool flag = false;
		GameLogEvent @out = null;
		for (int i = 0; i < m_AddPatterns.Count; i++)
		{
			if (m_AddPatterns[i].TryApply(eventsQueue, @event, out @out))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			eventsQueue.Add(@event);
		}
		GameLogEvent event2 = @out ?? @event;
		for (int j = 0; j < m_PostAddPatterns.Count; j++)
		{
			m_PostAddPatterns[j].Apply(eventsQueue, event2);
		}
	}

	void IPatternCollection.Cleanup()
	{
		m_AddPatterns?.Clear();
		m_PostAddPatterns?.Clear();
	}
}

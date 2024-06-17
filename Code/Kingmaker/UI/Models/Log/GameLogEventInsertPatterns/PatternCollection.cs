using System.Collections.Generic;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;

public class PatternCollection : IPatternCollection
{
	private List<IPattern> m_Patterns;

	private static IPatternCollection s_Instance;

	public static IPatternCollection Instance => s_Instance ?? (s_Instance = new PatternCollection());

	IPatternCollection IPatternCollection.AddPattern(IPattern pattern)
	{
		if (m_Patterns == null)
		{
			m_Patterns = new List<IPattern>(10);
		}
		if (!m_Patterns.Contains(pattern))
		{
			m_Patterns.Add(pattern);
		}
		return this;
	}

	bool IPatternCollection.ApplyPatterns(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		if (m_Patterns == null)
		{
			return false;
		}
		foreach (IPattern pattern in m_Patterns)
		{
			if (pattern.Apply(eventsQueue, @event))
			{
				return true;
			}
		}
		return false;
	}

	void IPatternCollection.Cleanup()
	{
		m_Patterns?.Clear();
	}
}

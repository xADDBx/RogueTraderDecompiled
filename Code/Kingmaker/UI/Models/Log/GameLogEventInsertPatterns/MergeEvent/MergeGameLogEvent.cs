using System.Collections.Generic;
using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent;

public class MergeGameLogEvent<T> : GameLogEvent<MergeGameLogEvent<T>> where T : GameLogEvent
{
	private readonly List<T> m_Events;

	public MergeGameLogEvent()
	{
		m_Events = new List<T>();
	}

	public void AddEvent(T @event)
	{
		m_Events.Add(@event);
	}

	public IReadOnlyList<T> GetEvents()
	{
		return m_Events;
	}
}

using System.Collections.Generic;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent.Comparers;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent;

public class MergeEventPattern<T> : IPattern where T : GameLogEvent
{
	private class MergeEventComparers : IMergeEventComparer
	{
		private readonly IMergeEventComparer[] m_Comparers;

		public static IMergeEventComparer Create(IMergeEventComparer[] comparers)
		{
			return new MergeEventComparers(comparers);
		}

		private MergeEventComparers(IMergeEventComparer[] comparers)
		{
			m_Comparers = comparers;
		}

		public bool Compare(GameLogEvent evn1, GameLogEvent evn2)
		{
			if (m_Comparers == null)
			{
				return false;
			}
			for (int i = 0; i < m_Comparers.Length; i++)
			{
				if (m_Comparers[i].Compare(evn1, evn2))
				{
					return true;
				}
			}
			return false;
		}
	}

	private readonly IMergeEventComparer m_Comparer;

	public static IPattern Create(IMergeEventComparer comparer)
	{
		return new MergeEventPattern<T>(comparer);
	}

	public static IPattern Create(IMergeEventComparer[] comparers)
	{
		return new MergeEventPattern<T>(comparers);
	}

	private MergeEventPattern(IMergeEventComparer comparer)
	{
		m_Comparer = comparer;
	}

	private MergeEventPattern(IMergeEventComparer[] comparers)
	{
		m_Comparer = MergeEventComparers.Create(comparers);
	}

	bool IPattern.Apply(List<GameLogEvent> eventsQueue, GameLogEvent @event)
	{
		T evn = @event as T;
		if (evn != null)
		{
			int num = eventsQueue.FindLastIndex((GameLogEvent o) => o is MergeGameLogEvent<T> mergeGameLogEvent3 && m_Comparer.Compare(mergeGameLogEvent3.GetEvents()[0], evn));
			if (num != -1 && eventsQueue[num] is MergeGameLogEvent<T> mergeGameLogEvent)
			{
				mergeGameLogEvent.AddEvent(evn);
				return true;
			}
			num = eventsQueue.FindLastIndex((GameLogEvent o) => o is T evn2 && m_Comparer.Compare(evn2, evn));
			if (num != -1)
			{
				MergeGameLogEvent<T> mergeGameLogEvent2 = new MergeGameLogEvent<T>();
				mergeGameLogEvent2.AddEvent((T)eventsQueue[num]);
				mergeGameLogEvent2.AddEvent(evn);
				eventsQueue.RemoveAt(num);
				eventsQueue.Insert(num, mergeGameLogEvent2);
				return true;
			}
		}
		return false;
	}
}

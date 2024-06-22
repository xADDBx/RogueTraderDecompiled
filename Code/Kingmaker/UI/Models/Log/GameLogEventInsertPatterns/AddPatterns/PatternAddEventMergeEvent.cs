using System.Collections.Generic;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent.Comparers;

namespace Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.AddPatterns;

public class PatternAddEventMergeEvent<T> : PatternAddEvent where T : GameLogEvent
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

	public static PatternAddEvent Create(IMergeEventComparer comparer)
	{
		return new PatternAddEventMergeEvent<T>(comparer);
	}

	public static PatternAddEvent Create(IMergeEventComparer[] comparers)
	{
		return new PatternAddEventMergeEvent<T>(comparers);
	}

	private PatternAddEventMergeEvent(IMergeEventComparer comparer)
	{
		m_Comparer = comparer;
	}

	private PatternAddEventMergeEvent(IMergeEventComparer[] comparers)
	{
		m_Comparer = MergeEventComparers.Create(comparers);
	}

	protected override bool TryApplyImpl(List<GameLogEvent> queue, GameLogEvent @in, out GameLogEvent @out)
	{
		T evn = @in as T;
		if (evn != null)
		{
			int num = queue.FindLastIndex((GameLogEvent o) => o is MergeGameLogEvent<T> mergeGameLogEvent3 && m_Comparer.Compare(mergeGameLogEvent3.GetEvents()[0], evn));
			if (num != -1 && queue[num] is MergeGameLogEvent<T> mergeGameLogEvent)
			{
				mergeGameLogEvent.AddEvent(evn);
				@out = mergeGameLogEvent;
				return true;
			}
			num = queue.FindLastIndex((GameLogEvent o) => o is T evn2 && m_Comparer.Compare(evn2, evn));
			if (num != -1)
			{
				MergeGameLogEvent<T> mergeGameLogEvent2 = new MergeGameLogEvent<T>();
				mergeGameLogEvent2.AddEvent((T)queue[num]);
				mergeGameLogEvent2.AddEvent(evn);
				@out = mergeGameLogEvent2;
				queue.RemoveAt(num);
				queue.Insert(num, mergeGameLogEvent2);
				return true;
			}
		}
		@out = null;
		return false;
	}
}

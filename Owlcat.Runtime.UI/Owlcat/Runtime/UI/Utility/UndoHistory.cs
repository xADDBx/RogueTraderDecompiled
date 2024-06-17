using Owlcat.Runtime.UI.Dependencies;

namespace Owlcat.Runtime.UI.Utility;

public class UndoHistory<T>
{
	private readonly T[] m_History;

	private readonly int m_Capacity;

	private int m_CurrentIndex;

	private int m_MinIndex;

	private int m_MaxIndex;

	public UndoHistory(int capacity, T initialState)
	{
		m_History = new T[capacity];
		m_CurrentIndex = 0;
		m_MinIndex = 0;
		m_MaxIndex = 0;
		m_Capacity = capacity;
		m_History[0] = initialState;
	}

	public UndoHistory(int capacity)
	{
		m_History = new T[capacity];
		m_CurrentIndex = -1;
		m_MinIndex = 0;
		m_MaxIndex = 0;
		m_Capacity = capacity;
	}

	public void Add(T item)
	{
		m_CurrentIndex++;
		m_MaxIndex = m_CurrentIndex;
		m_History[m_CurrentIndex % m_Capacity] = item;
		if (m_MaxIndex - m_MinIndex > m_Capacity - 1)
		{
			m_MinIndex = m_MaxIndex - (m_Capacity - 1);
			if (m_MinIndex >= m_Capacity)
			{
				m_MaxIndex -= m_Capacity;
				m_CurrentIndex -= m_Capacity;
				m_MaxIndex -= m_Capacity;
			}
		}
	}

	public bool HasPrev()
	{
		return m_CurrentIndex > m_MinIndex;
	}

	public T PopPrev()
	{
		if (!HasPrev())
		{
			UIKitLogger.Error("History has no prev object, but you are trying to access it");
			return default(T);
		}
		return m_History[m_CurrentIndex-- % m_Capacity];
	}

	public T PeekPrev()
	{
		if (!HasPrev())
		{
			UIKitLogger.Error("History has no prev object, but you are trying to access it");
			return default(T);
		}
		return m_History[m_CurrentIndex % m_Capacity];
	}

	public override string ToString()
	{
		return $"Current index: {m_CurrentIndex}; Max index: {m_MaxIndex}; Min index: {m_MinIndex}";
	}
}

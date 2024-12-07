using System;
using JetBrains.Annotations;

namespace Kingmaker.Utility;

public class RingBuffer<T>
{
	[NotNull]
	private readonly T[] m_Data;

	private int m_StartIndex;

	public int Count { get; private set; }

	private int EndIndex => (m_StartIndex + Count - 1) % m_Data.Length;

	public bool Empty => Count == 0;

	public bool Full => Count >= m_Data.Length;

	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return m_Data[(m_StartIndex + index) % m_Data.Length];
		}
		set
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			m_Data[(m_StartIndex + index) % m_Data.Length] = value;
		}
	}

	public RingBuffer(int capacity)
	{
		m_Data = new T[capacity];
		m_StartIndex = 0;
		Count = 0;
	}

	public void SetEndIndexAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		Count = index + 1;
	}

	public void PushBack(T t)
	{
		if (Full)
		{
			m_StartIndex = Next(m_StartIndex);
			Count--;
		}
		Count++;
		m_Data[EndIndex] = t;
	}

	public void PushFront(T t)
	{
		if (Full)
		{
			Count--;
		}
		Count++;
		m_StartIndex = Prev(m_StartIndex);
		m_Data[m_StartIndex] = t;
	}

	public T PopBack()
	{
		if (Empty)
		{
			throw new InvalidOperationException("ring list is empty");
		}
		int endIndex = EndIndex;
		Count--;
		return m_Data[endIndex];
	}

	public T PopFront()
	{
		if (Empty)
		{
			throw new InvalidOperationException("ring list is empty");
		}
		int startIndex = m_StartIndex;
		m_StartIndex = Next(m_StartIndex);
		Count--;
		return m_Data[startIndex];
	}

	public void Reserve(int count)
	{
		while (Count < count)
		{
			PushBack(default(T));
		}
	}

	private int Next(int i)
	{
		return (i + 1) % m_Data.Length;
	}

	private int Prev(int i)
	{
		return (i + m_Data.Length - 1) % m_Data.Length;
	}
}

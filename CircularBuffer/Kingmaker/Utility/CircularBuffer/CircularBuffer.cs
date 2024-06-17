using System;
using UnityEngine;

namespace Kingmaker.Utility.CircularBuffer;

public class CircularBuffer<T>
{
	private readonly T[] m_Array;

	private int m_CurrentIndex = -1;

	private int m_Count;

	public T this[int index]
	{
		get
		{
			int internalIndex = GetInternalIndex(index);
			return m_Array[internalIndex];
		}
		set
		{
			int internalIndex = GetInternalIndex(index);
			m_Array[internalIndex] = value;
		}
	}

	public T First
	{
		get
		{
			return this[0];
		}
		set
		{
			this[0] = value;
		}
	}

	public T Last
	{
		get
		{
			return this[m_Count - 1];
		}
		set
		{
			this[m_Count - 1] = value;
		}
	}

	public int Count => m_Count;

	public int Capacity => m_Array.Length;

	public bool IsFull => m_Count == m_Array.Length;

	public CircularBuffer(int capacity)
	{
		m_Array = new T[capacity];
		m_Count = 0;
	}

	public void Append(T value)
	{
		m_CurrentIndex = (m_CurrentIndex + 1) % m_Array.Length;
		m_Array[m_CurrentIndex] = value;
		m_Count = Mathf.Min(m_Count + 1, m_Array.Length);
	}

	public void Clear()
	{
		Array.Fill(m_Array, default(T));
		m_CurrentIndex = -1;
		m_Count = 0;
	}

	private int GetInternalIndex(int externalIndex)
	{
		if (externalIndex < 0 || m_Count <= externalIndex)
		{
			throw new IndexOutOfRangeException($"i={externalIndex} n={m_Count}");
		}
		return (m_CurrentIndex - m_Count + 1 + m_Array.Length + externalIndex) % m_Array.Length;
	}
}

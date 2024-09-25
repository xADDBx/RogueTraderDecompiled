using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.Utility;

[JsonObject]
public class PriorityQueue<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	[JsonProperty]
	private readonly List<T> m_Data = new List<T>();

	private readonly IComparer<T> m_Comparer;

	private readonly IEqualityComparer<T> m_EqualityComparer;

	public int Count => m_Data.Count;

	public bool IsReadOnly => false;

	public PriorityQueue()
	{
		m_Comparer = Comparer<T>.Default;
		m_EqualityComparer = EqualityComparer<T>.Default;
	}

	public PriorityQueue(IComparer<T> comparer, IEqualityComparer<T> equalityComparer)
	{
		m_Comparer = comparer;
		m_EqualityComparer = equalityComparer;
	}

	public bool Contains(T value)
	{
		return Find(value) != -1;
	}

	public void Enqueue(T item)
	{
		m_Data.Add(item);
		int num = m_Data.Count - 1;
		while (num > 0)
		{
			int num2 = (num - 1) / 2;
			if (m_Comparer.Compare(m_Data[num], m_Data[num2]) < 0)
			{
				List<T> data = m_Data;
				int index = num;
				List<T> data2 = m_Data;
				int index2 = num2;
				T val = m_Data[num2];
				T val2 = m_Data[num];
				T val4 = (data[index] = val);
				val4 = (data2[index2] = val2);
				num = num2;
				continue;
			}
			break;
		}
	}

	public T Dequeue()
	{
		return RemoveAt(0);
	}

	public int Find(T val)
	{
		if (Count == 0)
		{
			return -1;
		}
		return FindFrom(0, val);
	}

	public int FindIndex(Predicate<T> val)
	{
		return m_Data.FindIndex(val);
	}

	private int FindFrom(int index, T val)
	{
		if (m_Comparer.Compare(val, m_Data[index]) < 0)
		{
			return -1;
		}
		if (m_EqualityComparer.Equals(m_Data[index], val))
		{
			return index;
		}
		int num = index * 2 + 1;
		int num2 = index * 2 + 2;
		if (num < Count)
		{
			int num3 = FindFrom(num, val);
			if (num3 != -1)
			{
				return num3;
			}
		}
		if (num2 < Count)
		{
			int num4 = FindFrom(num2, val);
			if (num4 != -1)
			{
				return num4;
			}
		}
		return -1;
	}

	public bool Remove(T val)
	{
		int num = Find(val);
		if (num == -1)
		{
			return false;
		}
		RemoveAt(num);
		return true;
	}

	public T RemoveAt(int index)
	{
		if (index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int index2 = Count - 1;
		T result = m_Data[index];
		m_Data[index] = m_Data[index2];
		m_Data.RemoveAt(index2);
		index2 = Count - 1;
		int num = index;
		while (true)
		{
			int num2 = num * 2 + 1;
			if (num2 > index2)
			{
				break;
			}
			int num3 = num2 + 1;
			if (num3 <= index2 && m_Comparer.Compare(m_Data[num3], m_Data[num2]) < 0)
			{
				num2 = num3;
			}
			if (m_Comparer.Compare(m_Data[num], m_Data[num2]) <= 0)
			{
				break;
			}
			List<T> data = m_Data;
			int index3 = num;
			List<T> data2 = m_Data;
			int index4 = num2;
			T val = m_Data[num2];
			T val2 = m_Data[num];
			T val4 = (data[index3] = val);
			val4 = (data2[index4] = val2);
			num = num2;
		}
		return result;
	}

	public T Peek()
	{
		return m_Data[0];
	}

	public T[] ToArray()
	{
		return m_Data.ToArray();
	}

	public List<T>.Enumerator GetEnumerator()
	{
		return m_Data.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	public override string ToString()
	{
		string text = "";
		for (int i = 0; i < m_Data.Count; i++)
		{
			text = text + m_Data[i]?.ToString() + " ";
		}
		return text + "count = " + m_Data.Count;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		m_Data.Clear();
	}

	public bool IsConsistent()
	{
		if (m_Data.Count == 0)
		{
			return true;
		}
		int num = m_Data.Count - 1;
		for (int i = 0; i < m_Data.Count; i++)
		{
			int num2 = 2 * i + 1;
			int num3 = 2 * i + 2;
			if (num2 <= num && m_Comparer.Compare(m_Data[i], m_Data[num2]) > 0)
			{
				return false;
			}
			if (num3 <= num && m_Comparer.Compare(m_Data[i], m_Data[num3]) > 0)
			{
				return false;
			}
		}
		return true;
	}

	public void Add(T item)
	{
		Enqueue(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		((ICollection<T>)m_Data).CopyTo(array, arrayIndex);
	}
}

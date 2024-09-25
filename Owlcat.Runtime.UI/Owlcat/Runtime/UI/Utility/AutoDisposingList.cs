using System;
using System.Collections;
using System.Collections.Generic;

namespace Owlcat.Runtime.UI.Utility;

public class AutoDisposingList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T : IDisposable
{
	private List<T> m_List = new List<T>();

	public T this[int index]
	{
		get
		{
			return m_List[index];
		}
		set
		{
			m_List[index] = value;
		}
	}

	public int Count => m_List.Count;

	public bool IsReadOnly => false;

	public void Add(T item)
	{
		m_List.Add(item);
	}

	public void Clear()
	{
		foreach (T item in m_List)
		{
			item.Dispose();
		}
		m_List.Clear();
	}

	public bool Contains(T item)
	{
		return m_List.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		m_List.CopyTo(array, arrayIndex);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_List.GetEnumerator();
	}

	public int IndexOf(T item)
	{
		return m_List.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		m_List.Insert(index, item);
	}

	public void RemoveAndDispose(T item)
	{
		if (m_List.Remove(item))
		{
			item.Dispose();
			return;
		}
		throw new InvalidOperationException("Item not in list and cannot be disposed safely");
	}

	public bool Remove(T item)
	{
		return m_List.Remove(item);
	}

	public void RemoveAndDisposeAt(int index)
	{
		T val = m_List[index];
		m_List.RemoveAt(index);
		val.Dispose();
	}

	public void RemoveAt(int index)
	{
		m_List.RemoveAt(index);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_List.GetEnumerator();
	}

	public void AddRange(IEnumerable<T> collection)
	{
		m_List.AddRange(collection);
	}

	public void RemoveRangeAndDispose(int start, int end)
	{
		for (int i = start; i < end; i++)
		{
			m_List[i].Dispose();
		}
		m_List.RemoveRange(start, end);
	}

	public void ForEach(Action<T> action)
	{
		m_List.ForEach(action);
	}

	public T[] ToArray()
	{
		return m_List.ToArray();
	}
}

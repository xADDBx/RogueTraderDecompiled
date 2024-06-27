using System;
using System.Collections;
using System.Collections.Generic;

namespace Kingmaker.Utility.DotNetExtensions;

public struct FilteredList<T>
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly IList<T> m_List;

		private readonly Func<T, bool> m_Filter;

		private int m_Index;

		public T Current
		{
			get
			{
				if (m_Index >= 0 && m_Index < m_List.Count)
				{
					return m_List[m_Index];
				}
				return default(T);
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(IList<T> list, Func<T, bool> filter)
		{
			m_List = list;
			m_Filter = filter;
			m_Index = -1;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			while (++m_Index < m_List.Count && !m_Filter(m_List[m_Index]))
			{
			}
			return m_Index < m_List.Count;
		}

		void IEnumerator.Reset()
		{
			m_Index = -1;
		}
	}

	private readonly IList<T> m_List;

	private readonly Func<T, bool> m_Filter;

	public FilteredList(IList<T> list, Func<T, bool> filter)
	{
		m_List = list;
		m_Filter = filter;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_List, m_Filter);
	}

	public bool HasItem(T item)
	{
		if (m_Filter(item))
		{
			return m_List.HasItem(item);
		}
		return false;
	}

	public bool HasItem(Func<T, bool> pred)
	{
		for (int i = 0; i < m_List.Count; i++)
		{
			T arg = m_List[i];
			if (m_Filter(arg) && pred(arg))
			{
				return true;
			}
		}
		return false;
	}

	public T FirstItem(Func<T, bool> pred = null)
	{
		for (int i = 0; i < m_List.Count; i++)
		{
			T val = m_List[i];
			if (m_Filter(val) && (pred == null || pred(val)))
			{
				return val;
			}
		}
		return default(T);
	}
}

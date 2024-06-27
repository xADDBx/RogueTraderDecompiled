using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.Utility.DotNetExtensions;

public struct ListEnumerator<T> : IEnumerator<T>, IEnumerator, IDisposable
{
	[CanBeNull]
	private readonly IList<T> m_List;

	private int m_Index;

	public T Current
	{
		get
		{
			if (m_List == null)
			{
				throw new NullReferenceException();
			}
			return m_List[m_Index];
		}
	}

	object IEnumerator.Current => Current;

	public ListEnumerator([CanBeNull] IList<T> list)
	{
		this = default(ListEnumerator<T>);
		m_List = list;
		m_Index = -1;
	}

	public bool MoveNext()
	{
		if (m_List != null)
		{
			return ++m_Index < m_List.Count;
		}
		return false;
	}

	public void Reset()
	{
		m_Index = -1;
	}

	public void Dispose()
	{
	}
}

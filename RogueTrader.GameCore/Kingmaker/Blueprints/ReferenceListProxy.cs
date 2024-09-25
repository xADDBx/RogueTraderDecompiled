using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints;

public class ReferenceListProxy<T, TRef> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T : BlueprintScriptableObject where TRef : BlueprintReference<T>, new()
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		[CanBeNull]
		private readonly IList<TRef> m_List;

		private int m_Index;

		public T Current
		{
			get
			{
				IList<TRef> list = m_List;
				if (list == null)
				{
					return null;
				}
				TRef val = list.Get(m_Index);
				if (val == null)
				{
					return null;
				}
				return val.Get();
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(IList<TRef> list)
		{
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

	[CanBeNull]
	private readonly List<TRef> m_List;

	public int Count => m_List?.Count ?? 0;

	public bool IsReadOnly => false;

	public T this[int index]
	{
		get
		{
			List<TRef> list = m_List;
			if (list == null)
			{
				return null;
			}
			TRef val = list[index];
			if (val == null)
			{
				return null;
			}
			return val.Get();
		}
		set
		{
			if (m_List != null)
			{
				m_List[index] = BlueprintReference<T>.CreateTyped<TRef>(value);
			}
		}
	}

	public ReferenceListProxy([CanBeNull] List<TRef> list)
	{
		m_List = list;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		if (m_List != null)
		{
			for (int i = 0; i < m_List.Count; i++)
			{
				TRef val = m_List[i];
				yield return val.Get();
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (m_List != null)
		{
			for (int i = 0; i < m_List.Count; i++)
			{
				TRef val = m_List[i];
				yield return val.Get();
			}
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_List);
	}

	public void Add(T item)
	{
		m_List?.Add(BlueprintReference<T>.CreateTyped<TRef>(item));
	}

	public void Clear()
	{
		m_List?.Clear();
	}

	public bool Contains(T item)
	{
		return m_List.Any((TRef r) => r.Is(item));
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (m_List != null)
		{
			for (int i = 0; i < m_List.Count; i++)
			{
				int num = arrayIndex + i;
				TRef val = m_List[i];
				array[num] = ((val != null) ? val.Get() : null);
			}
		}
	}

	public bool Remove(T item)
	{
		List<TRef> list = m_List;
		if (list == null)
		{
			return false;
		}
		return list.RemoveAll((TRef r) => r.Is(item)) > 0;
	}

	public int IndexOf(T item)
	{
		return m_List?.FindIndex((TRef r) => r.Is(item)) ?? (-1);
	}

	public void Insert(int index, T item)
	{
		m_List?.Insert(index, BlueprintReference<T>.CreateTyped<TRef>(item));
	}

	public void RemoveAt(int index)
	{
		m_List?.RemoveAt(index);
	}

	public void RemoveAll(Predicate<T> pred)
	{
		m_List?.RemoveAll((TRef r) => pred(r.Get()));
	}

	public static implicit operator List<TRef>(ReferenceListProxy<T, TRef> proxy)
	{
		return proxy.m_List;
	}

	public static implicit operator ReferenceListProxy<T, TRef>(List<TRef> list)
	{
		return new ReferenceListProxy<T, TRef>(list);
	}
}

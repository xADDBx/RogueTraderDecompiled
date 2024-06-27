using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Utility.DotNetExtensions;

public readonly struct ReadonlyList<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
{
	public static readonly ReadonlyList<T> Empty = new ReadonlyList<T>(null);

	[CanBeNull]
	private readonly IList<T> m_List;

	public int Count => m_List?.Count ?? 0;

	public T this[int index]
	{
		get
		{
			if (m_List == null)
			{
				throw new IndexOutOfRangeException("Index out of range");
			}
			return m_List[index];
		}
	}

	public ReadonlyList(IList<T> list)
	{
		m_List = list;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public ListEnumerator<T> GetEnumerator()
	{
		return new ListEnumerator<T>(m_List);
	}

	public static implicit operator ReadonlyList<T>(List<T> list)
	{
		return new ReadonlyList<T>(list);
	}

	public static implicit operator ReadonlyList<T>(T[] list)
	{
		return new ReadonlyList<T>(list);
	}

	public bool HasItem(Func<T, bool> pred)
	{
		return m_List.HasItem(pred);
	}

	public bool AllItems(Func<T, bool> pred)
	{
		return m_List.AllItems(pred);
	}

	public bool Contains(T value)
	{
		return m_List?.Contains(value) ?? false;
	}

	[CanBeNull]
	public T FirstItem(Func<T, bool> pred)
	{
		return m_List.FirstItem(pred);
	}

	public int FindIndex(Predicate<T> pred)
	{
		return m_List.FindIndex(pred);
	}

	[CanBeNull]
	public T FirstItem()
	{
		return m_List.FirstItem();
	}

	public T[] ToArray()
	{
		return m_List?.ToArray();
	}

	public List<T> ToList()
	{
		return m_List?.ToList();
	}

	public List<T> ToTempList()
	{
		return m_List?.ToTempList();
	}
}

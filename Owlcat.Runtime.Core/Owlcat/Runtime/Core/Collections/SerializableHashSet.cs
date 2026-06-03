using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Owlcat.Runtime.Core.Collections;

[Serializable]
public class SerializableHashSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ISerializationCallbackReceiver
{
	[SerializeField]
	private List<T> m_Values;

	private HashSet<T> m_HashSet;

	public int Count => m_HashSet.Count;

	bool ICollection<T>.IsReadOnly => false;

	public SerializableHashSet()
	{
		m_HashSet = new HashSet<T>();
	}

	public SerializableHashSet(IEnumerable<T> collection)
	{
		m_HashSet = new HashSet<T>(collection);
	}

	void ICollection<T>.Add(T item)
	{
		m_HashSet.Add(item);
	}

	public void ExceptWith(IEnumerable<T> other)
	{
		m_HashSet.ExceptWith(other);
	}

	public void IntersectWith(IEnumerable<T> other)
	{
		m_HashSet.IntersectWith(other);
	}

	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		return m_HashSet.IsProperSubsetOf(other);
	}

	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		return m_HashSet.IsProperSupersetOf(other);
	}

	public bool IsSubsetOf(IEnumerable<T> other)
	{
		return m_HashSet.IsSubsetOf(other);
	}

	public bool IsSupersetOf(IEnumerable<T> other)
	{
		return m_HashSet.IsSupersetOf(other);
	}

	public bool Overlaps(IEnumerable<T> other)
	{
		return m_HashSet.Overlaps(other);
	}

	public bool SetEquals(IEnumerable<T> other)
	{
		return m_HashSet.SetEquals(other);
	}

	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		m_HashSet.SymmetricExceptWith(other);
	}

	public void UnionWith(IEnumerable<T> other)
	{
		m_HashSet.UnionWith(other);
	}

	public void Clear()
	{
		m_HashSet.Clear();
	}

	public bool Add(T item)
	{
		return m_HashSet.Add(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		m_HashSet.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		return m_HashSet.Remove(item);
	}

	public bool Contains(T item)
	{
		return m_HashSet.Contains(item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_HashSet.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void OnBeforeSerialize()
	{
		m_Values = m_HashSet.ToList();
	}

	public void OnAfterDeserialize()
	{
		m_HashSet.Clear();
		foreach (T value in m_Values)
		{
			m_HashSet.Add(value);
		}
	}
}

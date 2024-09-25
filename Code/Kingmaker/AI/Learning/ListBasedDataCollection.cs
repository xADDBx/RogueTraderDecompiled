using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.AI.Learning;

[Serializable]
public abstract class ListBasedDataCollection<T> : DataCollection<T>
{
	[JsonProperty]
	protected List<T> m_Collection = new List<T>();

	public T this[int index] => m_Collection[index];

	public sealed override int Count => m_Collection.Count;

	public override void Add(T item)
	{
		m_Collection.Add(item);
	}

	public override bool Remove(T item)
	{
		return m_Collection.Remove(item);
	}

	public override void Clear()
	{
		m_Collection.Clear();
	}

	public sealed override bool Contains(T item)
	{
		return m_Collection.Contains(item);
	}

	public sealed override void CopyTo(T[] array, int arrayIndex)
	{
		m_Collection.CopyTo(array, arrayIndex);
	}

	public sealed override IEnumerator<T> GetEnumerator()
	{
		return m_Collection.GetEnumerator();
	}
}

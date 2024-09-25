using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Owlcat.Runtime.UI.Utility;

public class AutoDisposingDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable where TValue : IDisposable
{
	private Dictionary<TKey, TValue> m_Dict = new Dictionary<TKey, TValue>();

	public TValue this[TKey key]
	{
		get
		{
			return m_Dict[key];
		}
		set
		{
			m_Dict[key] = value;
		}
	}

	public ICollection<TKey> Keys => m_Dict.Keys;

	public ICollection<TValue> Values => m_Dict.Values;

	public int Count => m_Dict.Count;

	public bool IsReadOnly => false;

	public void Add(TKey key, TValue value)
	{
		m_Dict.Add(key, value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		m_Dict.Add(item.Key, item.Value);
	}

	public void Clear()
	{
		foreach (TValue value in m_Dict.Values)
		{
			value.Dispose();
		}
		m_Dict.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return m_Dict.Contains(item);
	}

	public bool ContainsKey(TKey key)
	{
		return m_Dict.ContainsKey(key);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		throw new NotImplementedException("Cannot copy AutoDisposingDictionary");
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return m_Dict.GetEnumerator();
	}

	public bool Remove(TKey key)
	{
		return m_Dict.Remove(key);
	}

	public bool RemoveAndDispose(TKey key)
	{
		if (m_Dict.TryGetValue(key, out var value))
		{
			value.Dispose();
			m_Dict.Remove(key);
			return true;
		}
		throw new KeyNotFoundException("Key not found, value cannot be disposed");
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return m_Dict.Remove(item.Key);
	}

	public bool RemoveAndDispose(KeyValuePair<TKey, TValue> item)
	{
		return RemoveAndDispose(item.Key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return m_Dict.TryGetValue(key, out value);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_Dict.GetEnumerator();
	}
}

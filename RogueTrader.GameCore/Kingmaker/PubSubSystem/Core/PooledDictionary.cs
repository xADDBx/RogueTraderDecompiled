using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pathfinding.Util;

namespace Kingmaker.PubSubSystem.Core;

public class PooledDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable where TValue : class, IAstarPooledObject, new()
{
	[NotNull]
	private readonly Dictionary<TKey, TValue> m_Dictionary = new Dictionary<TKey, TValue>();

	public int Count => m_Dictionary.Count;

	public IEnumerable<TValue> Values => m_Dictionary.Values;

	[NotNull]
	public TValue Sure(TKey key)
	{
		if (!m_Dictionary.TryGetValue(key, out var value))
		{
			value = ObjectPool<TValue>.Claim();
			m_Dictionary[key] = value;
		}
		return value;
	}

	[CanBeNull]
	public TValue Get(TKey key)
	{
		if (m_Dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public void Remove(TKey key)
	{
		if (m_Dictionary.TryGetValue(key, out var value))
		{
			m_Dictionary.Remove(key);
			ObjectPool<TValue>.Release(ref value);
		}
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return m_Dictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_Dictionary.GetEnumerator();
	}

	public void Clear()
	{
		m_Dictionary.Clear();
	}
}

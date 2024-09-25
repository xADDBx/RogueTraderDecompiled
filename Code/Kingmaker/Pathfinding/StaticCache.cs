using System;
using System.Collections.Concurrent;

namespace Kingmaker.Pathfinding;

public class StaticCache<TKey, TValue>
{
	private readonly Func<TKey, TValue> m_Factory;

	private readonly ConcurrentDictionary<TKey, TValue> m_Cache = new ConcurrentDictionary<TKey, TValue>();

	public StaticCache(Func<TKey, TValue> factory)
	{
		m_Factory = factory;
	}

	public TValue Get(TKey key)
	{
		return m_Cache.GetOrAdd(key, m_Factory);
	}
}

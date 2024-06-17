using System;
using System.Collections.Generic;

namespace Kingmaker.PubSubSystem.Core;

public class PooledHashSet<T> : HashSet<T>, IDisposable
{
	private static readonly Stack<PooledHashSet<T>> Pool = new Stack<PooledHashSet<T>>();

	public static PooledHashSet<T> Get()
	{
		if (Pool.Count <= 0)
		{
			return new PooledHashSet<T>();
		}
		return Pool.Pop();
	}

	public static PooledHashSet<T> Get(IList<T> list)
	{
		PooledHashSet<T> pooledHashSet = Get();
		for (int i = 0; i < list.Count; i++)
		{
			pooledHashSet.Add(list[i]);
		}
		return pooledHashSet;
	}

	public static PooledHashSet<T> Get(HashSet<T> set)
	{
		PooledHashSet<T> pooledHashSet = Get();
		foreach (T item in set)
		{
			pooledHashSet.Add(item);
		}
		return pooledHashSet;
	}

	public static PooledHashSet<T> Get(IEnumerable<T> collection)
	{
		PooledHashSet<T> pooledHashSet = Get();
		foreach (T item in collection)
		{
			pooledHashSet.Add(item);
		}
		return pooledHashSet;
	}

	private static void Return(PooledHashSet<T> list)
	{
		list.Clear();
		Pool.Push(list);
	}

	void IDisposable.Dispose()
	{
		Return(this);
	}
}

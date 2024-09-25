using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.PubSubSystem.Core;

public class PooledList<T> : List<T>, IDisposable
{
	private static Stack<PooledList<T>> _pool = new Stack<PooledList<T>>();

	private bool _inPool;

	public PooledList()
	{
	}

	public PooledList(int capacity)
		: base(capacity)
	{
	}

	public PooledList(IEnumerable<T> collection)
		: base(collection)
	{
	}

	public static PooledList<T> Get()
	{
		PooledList<T> obj = ((_pool.Count > 0) ? _pool.Pop() : new PooledList<T>());
		obj._inPool = false;
		return obj;
	}

	public static PooledList<T> Get(int capacity)
	{
		PooledList<T> obj = ((_pool.Count > 0) ? _pool.Pop() : new PooledList<T>(capacity));
		obj._inPool = false;
		return obj;
	}

	public static PooledList<T> Get(IEnumerable<T> collection)
	{
		PooledList<T> pooledList;
		if (_pool.Count > 0)
		{
			pooledList = _pool.Pop();
			pooledList.AddRange(collection);
		}
		else
		{
			pooledList = new PooledList<T>(collection);
		}
		pooledList._inPool = false;
		return pooledList;
	}

	public static void Return(PooledList<T> item)
	{
		if (!item._inPool)
		{
			item._inPool = true;
			item.Clear();
			_pool.Push(item);
		}
		else
		{
			Debug.LogError("Повторный возврат в пул кешируемого листа");
		}
	}

	public void Dispose()
	{
		Return(this);
	}

	public override string ToString()
	{
		return $"{base.Count} элементов {typeof(T)}";
	}
}

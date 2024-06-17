using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;

namespace Kingmaker.Utility;

[JsonObject]
public class MultiSet<T> : IEnumerable<T>, IEnumerable
{
	[JsonProperty]
	private readonly Dictionary<T, int> m_Data = new Dictionary<T, int>();

	public Dictionary<T, int>.KeyCollection Values => m_Data.Keys;

	public MultiSet()
	{
	}

	public MultiSet(IEnumerable<T> items)
		: this()
	{
		items.ForEach(Add);
	}

	public bool Contains(T item)
	{
		return m_Data.ContainsKey(item);
	}

	public bool Any(Func<T, bool> pred)
	{
		foreach (T key in m_Data.Keys)
		{
			if (pred(key))
			{
				return true;
			}
		}
		return false;
	}

	public void Add(T item)
	{
		if (m_Data.ContainsKey(item))
		{
			m_Data[item]++;
		}
		else
		{
			m_Data[item] = 1;
		}
	}

	public void Add(IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			Add(item);
		}
	}

	public void Remove(T item)
	{
		if (!m_Data.ContainsKey(item))
		{
			PFLog.Default.Error("Has no item in set");
		}
		else if (--m_Data[item] == 0)
		{
			m_Data.Remove(item);
		}
	}

	public void RemoveAll(T item)
	{
		m_Data.Remove(item);
	}

	public void Clear()
	{
		m_Data.Clear();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_Data.Select((KeyValuePair<T, int> i) => i.Key).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void RemoveWhere(Func<T, bool> pred)
	{
		IEnumerable<T> source = m_Data.Keys.Where(pred);
		if (source.Any())
		{
			source.ToList().ForEach(delegate(T k)
			{
				m_Data.Remove(k);
			});
		}
	}
}

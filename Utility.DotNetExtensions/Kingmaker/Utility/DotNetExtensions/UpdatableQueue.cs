using System;
using System.Collections.Generic;

namespace Kingmaker.Utility.DotNetExtensions;

public class UpdatableQueue<T>
{
	private readonly List<T> m_CurrentList = new List<T>();

	private readonly List<T> m_NextList = new List<T>();

	public void Add(T elem)
	{
		if (!m_NextList.Contains(elem))
		{
			m_NextList.Add(elem);
		}
	}

	public void Remove(T elem)
	{
		m_CurrentList.Remove(elem);
		m_NextList.Remove(elem);
	}

	public void Prepare()
	{
		m_CurrentList.AddRange(m_NextList);
		m_CurrentList.Reverse();
	}

	public bool Next(out T value)
	{
		return m_CurrentList.TryPop(out value);
	}

	public bool Contains(T value)
	{
		return m_NextList.IndexOf(value) != -1;
	}

	public void Clear()
	{
		m_CurrentList.Clear();
		m_NextList.Clear();
	}

	public bool TryFind(Predicate<T> predicate, out T result)
	{
		result = default(T);
		result = m_CurrentList.Find(predicate);
		if (result != null)
		{
			return true;
		}
		result = m_NextList.Find(predicate);
		if (result != null)
		{
			return true;
		}
		return false;
	}
}

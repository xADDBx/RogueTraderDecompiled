using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.Blueprints;

public struct BlueprintComponentsEnumerator<T> : IEnumerator<T>, IEnumerator, IDisposable, IEnumerable<T>, IEnumerable
{
	[CanBeNull]
	private readonly BlueprintScriptableObject m_Blueprint;

	private int m_Index;

	public T Current
	{
		get
		{
			if (m_Blueprint == null)
			{
				throw new InvalidOperationException();
			}
			if (m_Index < 0 || m_Blueprint.ComponentsArray.Length <= m_Index)
			{
				throw new InvalidOperationException();
			}
			BlueprintComponent blueprintComponent = m_Blueprint.ComponentsArray[m_Index];
			if (blueprintComponent is T)
			{
				return (T)(object)((blueprintComponent is T) ? blueprintComponent : null);
			}
			throw new InvalidOperationException("Current is not valid");
		}
	}

	object IEnumerator.Current => Current;

	public BlueprintComponentsEnumerator(BlueprintScriptableObject blueprint)
	{
		m_Blueprint = blueprint;
		m_Index = -1;
	}

	public bool MoveNext()
	{
		if (m_Blueprint == null)
		{
			return false;
		}
		m_Index++;
		BlueprintComponent[] componentsArray = m_Blueprint.ComponentsArray;
		int num = componentsArray.Length;
		while (m_Index < num)
		{
			if (componentsArray[m_Index] is T)
			{
				return true;
			}
			m_Index++;
		}
		return false;
	}

	public void Reset()
	{
		m_Index = -1;
	}

	public void Dispose()
	{
	}

	public BlueprintComponentsEnumerator<T> GetEnumerator()
	{
		return new BlueprintComponentsEnumerator<T>(m_Blueprint);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Any()
	{
		if (m_Blueprint == null)
		{
			return false;
		}
		BlueprintComponent[] componentsArray = m_Blueprint.ComponentsArray;
		int i = 0;
		for (int num = componentsArray.Length; i < num; i++)
		{
			if (componentsArray[i] is T)
			{
				return true;
			}
		}
		return false;
	}

	public bool Empty()
	{
		return !Any();
	}

	public T FirstOrDefault()
	{
		if (m_Blueprint == null)
		{
			return default(T);
		}
		BlueprintComponent[] componentsArray = m_Blueprint.ComponentsArray;
		int i = 0;
		for (int num = componentsArray.Length; i < num; i++)
		{
			BlueprintComponent blueprintComponent = componentsArray[i];
			if (blueprintComponent is T)
			{
				return (T)(object)((blueprintComponent is T) ? blueprintComponent : null);
			}
		}
		return default(T);
	}
}

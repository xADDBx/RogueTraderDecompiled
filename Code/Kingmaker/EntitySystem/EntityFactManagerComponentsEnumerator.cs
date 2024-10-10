using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;

namespace Kingmaker.EntitySystem;

public struct EntityFactManagerComponentsEnumerator<T> : IEnumerator<T>, IEnumerator, IDisposable, IEnumerable<T>, IEnumerable where T : BlueprintComponent
{
	[CanBeNull]
	private readonly EntityFactsManager m_EntityFactsManager;

	private readonly Func<T, bool> m_Predicate;

	private int m_Index;

	public T Current
	{
		get
		{
			if (m_EntityFactsManager == null)
			{
				throw new InvalidOperationException();
			}
			if (m_Index < 0 || m_EntityFactsManager.List.Count <= m_Index)
			{
				throw new InvalidOperationException();
			}
			if (TryGetElement(m_EntityFactsManager, m_Predicate, m_Index, out var component, out var _))
			{
				return component;
			}
			throw new InvalidOperationException("Current is not valid");
		}
	}

	object IEnumerator.Current => Current;

	public EntityFactManagerComponentsEnumerator([CanBeNull] EntityFactsManager entityFactsManager, [CanBeNull] Func<T, bool> predicate)
	{
		m_EntityFactsManager = entityFactsManager;
		m_Predicate = predicate;
		m_Index = -1;
	}

	public bool MoveNext()
	{
		m_Index++;
		bool endReached = false;
		while (!endReached)
		{
			if (TryGetElement(m_EntityFactsManager, m_Predicate, m_Index, out var _, out endReached))
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

	public EntityFactManagerComponentsEnumerator<T> GetEnumerator()
	{
		return new EntityFactManagerComponentsEnumerator<T>(m_EntityFactsManager, m_Predicate);
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
		bool endReached = false;
		int num = 0;
		while (!endReached)
		{
			if (TryGetElement(m_EntityFactsManager, m_Predicate, num, out var _, out endReached))
			{
				return true;
			}
			num++;
		}
		return false;
	}

	public bool Empty()
	{
		return !Any();
	}

	public T FirstOrDefault()
	{
		if (TryGetElement(m_EntityFactsManager, m_Predicate, 0, out var component, out var _))
		{
			return component;
		}
		return null;
	}

	private static bool TryGetElement(EntityFactsManager entityFactsManager, Func<T, bool> predicate, int index, out T component, out bool endReached)
	{
		if (entityFactsManager == null)
		{
			component = null;
			endReached = true;
			return false;
		}
		if (index < 0)
		{
			component = null;
			endReached = false;
			return false;
		}
		foreach (EntityFact item in entityFactsManager.List)
		{
			foreach (T component2 in item.GetComponents(predicate))
			{
				if (index == 0)
				{
					component = component2;
					endReached = false;
					return true;
				}
				index--;
			}
		}
		component = null;
		endReached = true;
		return false;
	}
}

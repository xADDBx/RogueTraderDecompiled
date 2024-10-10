using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;

namespace Kingmaker.EntitySystem;

public struct EntityFactComponentsEnumerator<T> : IEnumerator<T>, IEnumerator, IDisposable, IEnumerable<T>, IEnumerable where T : BlueprintComponent
{
	[CanBeNull]
	private readonly EntityFact m_EntityFact;

	private readonly Func<T, bool> m_Predicate;

	private int m_Index;

	public T Current
	{
		get
		{
			if (m_EntityFact == null)
			{
				throw new InvalidOperationException();
			}
			if (m_Index < 0)
			{
				throw new InvalidOperationException();
			}
			if (TryGetElement(m_EntityFact, m_Predicate, m_Index, out var component, out var _))
			{
				return component;
			}
			throw new InvalidOperationException();
		}
	}

	object IEnumerator.Current => Current;

	public EntityFactComponentsEnumerator([CanBeNull] EntityFact entityFact, [CanBeNull] Func<T, bool> predicate)
	{
		m_EntityFact = entityFact;
		m_Predicate = predicate;
		m_Index = -1;
	}

	public bool MoveNext()
	{
		m_Index++;
		bool endReached = false;
		while (!endReached)
		{
			if (TryGetElement(m_EntityFact, m_Predicate, m_Index, out var _, out endReached))
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

	public EntityFactComponentsEnumerator<T> GetEnumerator()
	{
		return new EntityFactComponentsEnumerator<T>(m_EntityFact, m_Predicate);
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
			if (TryGetElement(m_EntityFact, m_Predicate, num, out var _, out endReached))
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

	private static bool TryGetElement(EntityFact entityFact, Func<T, bool> predicate, int index, out T component, out bool endReached)
	{
		if (entityFact == null)
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
		List<EntityFactComponent> components = entityFact.Components;
		int count = components.Count;
		if (index < count)
		{
			BlueprintComponent sourceBlueprintComponent = components[index].SourceBlueprintComponent;
			if (sourceBlueprintComponent is T val && !sourceBlueprintComponent.Disabled && (predicate == null || predicate(val)))
			{
				component = val;
				endReached = false;
				return true;
			}
			component = null;
			endReached = false;
			return false;
		}
		index -= count;
		BlueprintComponent[] componentsArray = entityFact.Blueprint.ComponentsArray;
		count = componentsArray.Length;
		if (index < count)
		{
			BlueprintComponent blueprintComponent = componentsArray[index];
			if (blueprintComponent is IRuntimeEntityFactComponentProvider)
			{
				component = null;
				endReached = false;
				return false;
			}
			if (blueprintComponent is T val2 && !blueprintComponent.Disabled && (predicate == null || predicate(val2)))
			{
				component = val2;
				endReached = false;
				return true;
			}
			component = null;
			endReached = false;
			return false;
		}
		component = null;
		endReached = true;
		return false;
	}
}

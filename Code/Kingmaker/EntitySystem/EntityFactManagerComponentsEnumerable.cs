using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;

namespace Kingmaker.EntitySystem;

public readonly struct EntityFactManagerComponentsEnumerable<T> : IEnumerable<T>, IEnumerable where T : BlueprintComponent
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		[CanBeNull]
		private readonly EntityFactsManager m_EntityFactsManager;

		private readonly Func<T, bool> m_Predicate;

		private List<EntityFact>.Enumerator m_Facts;

		private EntityFactComponentsEnumerator<T> m_Components;

		public T Current => m_Components.Current;

		object IEnumerator.Current => Current;

		public Enumerator([CanBeNull] EntityFactsManager entityFactsManager, Func<T, bool> predicate)
		{
			this = default(Enumerator);
			m_EntityFactsManager = entityFactsManager;
			m_Predicate = predicate;
			m_Facts = m_EntityFactsManager?.List.GetEnumerator() ?? default(List<EntityFact>.Enumerator);
			m_Components = default(EntityFactComponentsEnumerator<T>);
		}

		public bool MoveNext()
		{
			if (m_Components.MoveNext())
			{
				return true;
			}
			while (m_Facts.MoveNext())
			{
				m_Components = m_Facts.Current.GetComponents(m_Predicate);
				if (m_Components.MoveNext())
				{
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
			m_Facts = m_EntityFactsManager?.List.GetEnumerator() ?? default(List<EntityFact>.Enumerator);
			m_Components = default(EntityFactComponentsEnumerator<T>);
		}

		public void Dispose()
		{
			m_Facts.Dispose();
			m_Components.Dispose();
		}
	}

	[CanBeNull]
	private readonly EntityFactsManager m_EntityFactsManager;

	private readonly Func<T, bool> m_Predicate;

	public EntityFactManagerComponentsEnumerable([CanBeNull] EntityFactsManager entityFactsManager, [CanBeNull] Func<T, bool> predicate)
	{
		m_EntityFactsManager = entityFactsManager;
		m_Predicate = predicate;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_EntityFactsManager, m_Predicate);
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
		using (Enumerator enumerator = GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
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
		using (Enumerator enumerator = GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
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

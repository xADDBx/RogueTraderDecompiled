using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.EntitySystem;

public class EntityPool<T> : IEnumerable<T>, IEnumerable where T : Entity
{
	private readonly List<T> m_Entities = new List<T>();

	[CanBeNull]
	private readonly Predicate<T> m_Filter;

	public List<T> All => m_Entities;

	public event Action<T> EventOnAddedEntity;

	public event Action<T> EventOnRemovedEntity;

	public EntityPool(Predicate<T> filter = null)
	{
		m_Filter = filter;
		SceneEntitiesState.OnAdded += HandleAdded;
		SceneEntitiesState.OnRemoved += HandleRemoved;
	}

	public EntityPoolEnumerator<T> GetEnumerator()
	{
		return new EntityPoolEnumerator<T>(m_Entities);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new EntityPoolEnumerator<T>(m_Entities);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new EntityPoolEnumerator<T>(m_Entities);
	}

	private void HandleRemoved(SceneEntitiesState state, Entity data)
	{
		if (data is T val)
		{
			this.EventOnRemovedEntity?.Invoke(val);
			m_Entities.Remove(val);
		}
	}

	private void HandleAdded(SceneEntitiesState state, Entity data)
	{
		if (data is T val && (m_Filter == null || m_Filter(val)))
		{
			if (m_Entities.Contains(val))
			{
				PFLog.Default.Warning($"Trying to add to pool {val} twice");
				return;
			}
			this.EventOnAddedEntity?.Invoke(val);
			m_Entities.Add(val);
		}
	}
}

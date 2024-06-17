using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Entities.Base;

public class EntityListsCache
{
	private readonly Dictionary<Type, IList> m_Cache = new Dictionary<Type, IList>();

	public ReadonlyList<TEntity> Get<TEntity>() where TEntity : IEntity
	{
		return (List<TEntity>)m_Cache.Get(typeof(TEntity));
	}

	public void Update<TEntity>(ConcurrentDictionary<string, EntityServiceProxy> entities) where TEntity : class, IEntity
	{
		List<TEntity> list = (List<TEntity>)m_Cache.Get(typeof(TEntity));
		if (list != null)
		{
			return;
		}
		list = new List<TEntity>();
		m_Cache.Add(typeof(TEntity), list);
		foreach (KeyValuePair<string, EntityServiceProxy> entity in entities)
		{
			entity.Deconstruct(out var _, out var value);
			if (value.Entity is TEntity item)
			{
				list.Add(item);
			}
		}
		list.Sort(EntityComparison<TEntity>.Instance);
	}

	public void Add(IEntity entity)
	{
		foreach (KeyValuePair<Type, IList> item in m_Cache)
		{
			if (item.Key.IsInstanceOfType(entity))
			{
				item.Value.InsertIntoSortedList(entity, EntityComparison<IEntity>.Instance);
			}
		}
	}

	public void Remove(IEntity entity)
	{
		foreach (KeyValuePair<Type, IList> item in m_Cache)
		{
			if (item.Key.IsInstanceOfType(entity))
			{
				item.Value.Remove(entity);
			}
		}
	}
}

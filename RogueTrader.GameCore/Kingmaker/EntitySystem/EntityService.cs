using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;

namespace Kingmaker.EntitySystem;

public class EntityService : IService, IDisposable
{
	private static ServiceProxy<EntityService> s_InstanceProxy;

	private readonly ConcurrentDictionary<string, EntityServiceProxy> m_Entities = new ConcurrentDictionary<string, EntityServiceProxy>(1, 31, StringComparer.Ordinal);

	private readonly EntityListsCache m_Cache = new EntityListsCache();

	private static readonly object Sync = new object();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.GameSession;

	public static EntityService Instance
	{
		get
		{
			lock (Sync)
			{
				if (s_InstanceProxy?.Instance != null)
				{
					return s_InstanceProxy.Instance;
				}
				Services.RegisterServiceInstance(new EntityService());
				s_InstanceProxy = Services.GetProxy<EntityService>();
				return s_InstanceProxy.Instance;
			}
		}
	}

	public void Dispose()
	{
		List<EntityServiceProxy> list = m_Entities.Values.ToTempList();
		list.Sort((EntityServiceProxy a, EntityServiceProxy b) => string.Compare(a?.Id, b?.Id, StringComparison.Ordinal));
		foreach (EntityServiceProxy item in list)
		{
			item.Entity?.Dispose();
			item.Dispose();
		}
	}

	public void Register(IEntity entity)
	{
		try
		{
			EntityServiceProxy proxy = GetProxy(entity.UniqueId);
			if (proxy.Entity != entity)
			{
				if (proxy.Entity != null)
				{
					throw new InvalidOperationException($"Multiple entities with same ID {entity.UniqueId}. Old: {proxy.Entity}, New: {entity}");
				}
				proxy.Entity = entity;
				entity.Proxy = proxy;
				m_Cache.Add(entity);
			}
		}
		catch (Exception ex)
		{
			LogChannel.System.Exception(ex);
		}
	}

	public void Unregister(IEntity entity)
	{
		try
		{
			if (m_Entities.Remove(entity.UniqueId, out var value))
			{
				value.Dispose();
			}
			entity.Proxy = null;
			m_Cache.Remove(entity);
		}
		catch (Exception ex)
		{
			LogChannel.System.Exception(ex);
		}
	}

	[NotNull]
	public EntityServiceProxy GetProxy(string id)
	{
		return m_Entities.GetOrAdd(id, (string key) => new EntityServiceProxy(key));
	}

	[CanBeNull]
	public IEntity GetEntity(string id)
	{
		return m_Entities.Get(id)?.Entity;
	}

	[CanBeNull]
	public TEntity GetEntity<TEntity>(string id) where TEntity : class, IEntity
	{
		return GetEntity(id) as TEntity;
	}

	public bool Contains(IEntity entity)
	{
		return GetProxy(entity.UniqueId).Entity == entity;
	}

	public ReadonlyList<TEntity> GetListUnsafe<TEntity>() where TEntity : class, IEntity
	{
		m_Cache.Update<TEntity>(m_Entities);
		return m_Cache.Get<TEntity>();
	}

	public ReadonlyList<IEntity> GetListUnsafe()
	{
		m_Cache.Update<IEntity>(m_Entities);
		return m_Cache.Get<IEntity>();
	}

	public List<TEntity> GetTempList<TEntity>() where TEntity : class, IEntity
	{
		m_Cache.Update<TEntity>(m_Entities);
		return m_Cache.Get<TEntity>().ToTempList();
	}

	private List<TEntity> GetTempListNoCache<TEntity>() where TEntity : class, IEntity
	{
		List<TEntity> list = TempList.Get<TEntity>();
		foreach (KeyValuePair<string, EntityServiceProxy> entity in m_Entities)
		{
			entity.Deconstruct(out var _, out var value);
			if (value.Entity is TEntity item)
			{
				list.Add(item);
			}
		}
		list.Sort(EntityComparison<TEntity>.Instance);
		return list;
	}

	public List<IEntity> GetTempListNoCache()
	{
		return GetTempListNoCache<IEntity>();
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem;

public class EntityFactsCache
{
	private IList[] m_Cache;

	public List<TFact> Get<TFact>() where TFact : EntityFact
	{
		return (List<TFact>)m_Cache.Get(EntityFactService.GetFactIndex<TFact>());
	}

	public IList GetOrDefault<TFact>(List<EntityFact> @default) where TFact : class
	{
		return m_Cache.Get(EntityFactService.GetFactIndex<TFact>()) ?? @default;
	}

	public void Update<TFact>(List<EntityFact> facts) where TFact : class
	{
		if (typeof(TFact) == typeof(EntityFact))
		{
			return;
		}
		if (m_Cache == null)
		{
			m_Cache = new IList[EntityFactService.FactIndexCount];
		}
		int factIndex = EntityFactService.GetFactIndex<TFact>();
		List<TFact> list = (List<TFact>)m_Cache.Get(factIndex);
		if (list != null)
		{
			return;
		}
		list = new List<TFact>();
		m_Cache[factIndex] = list;
		foreach (EntityFact fact in facts)
		{
			if (fact is TFact item)
			{
				list.Add(item);
			}
		}
	}

	public void Add(EntityFact fact)
	{
		Type type = fact.GetType();
		while (type != typeof(EntityFact))
		{
			m_Cache.Get(EntityFactService.GetFactIndex(type))?.Add(fact);
			type = type.BaseType;
		}
	}

	public void Remove(EntityFact fact)
	{
		Type type = fact.GetType();
		while (type != typeof(EntityFact))
		{
			m_Cache.Get(EntityFactService.GetFactIndex(type))?.Remove(fact);
			type = type.BaseType;
		}
	}

	public void Clear()
	{
		m_Cache = null;
	}
}

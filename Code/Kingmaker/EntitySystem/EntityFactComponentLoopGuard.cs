using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem;

public readonly struct EntityFactComponentLoopGuard : IDisposable
{
	private static readonly Dictionary<EntityFactComponent, int> Requested = new Dictionary<EntityFactComponent, int>(32);

	private readonly EntityFactComponent m_Component;

	public bool Blocked => Requested.Get(m_Component, 0) > 1;

	public static EntityFactComponentLoopGuard Request(EntityFactComponent component)
	{
		Requested[component] = Requested.Get(component, 0) + 1;
		return new EntityFactComponentLoopGuard(component);
	}

	private EntityFactComponentLoopGuard(EntityFactComponent component)
	{
		m_Component = component;
	}

	public void Dispose()
	{
		int num = Requested.Get(m_Component, 0);
		if (num < 2)
		{
			Requested.Remove(m_Component);
		}
		else
		{
			Requested[m_Component] = num - 1;
		}
	}
}

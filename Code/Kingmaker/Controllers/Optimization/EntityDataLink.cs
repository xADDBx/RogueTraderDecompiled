using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.Controllers.Optimization;

public class EntityDataLink : MonoBehaviour
{
	private static readonly Dictionary<Component, Entity> m_EntityDataLinksCache = new Dictionary<Component, Entity>();

	public Entity Entity { get; set; }

	[CanBeNull]
	public static Entity GetEntity(Component component)
	{
		if (!m_EntityDataLinksCache.TryGetValue(component, out var value))
		{
			value = (component.TryGetComponent<EntityDataLink>(out var component2) ? component2.Entity : null);
			if (!value.IsDisposingNow)
			{
				m_EntityDataLinksCache.Add(component, value);
			}
		}
		return value;
	}

	public static void ClearCache()
	{
		m_EntityDataLinksCache.Clear();
	}
}

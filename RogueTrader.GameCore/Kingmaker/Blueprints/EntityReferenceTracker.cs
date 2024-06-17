using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.Blueprints;

public static class EntityReferenceTracker
{
	[NotNull]
	private static readonly List<EntityReference> s_CachedReferences = new List<EntityReference>();

	public static void Register(EntityReference r)
	{
		s_CachedReferences.Add(r);
	}

	public static void DropCached()
	{
		foreach (EntityReference s_CachedReference in s_CachedReferences)
		{
			s_CachedReference.DropCached();
		}
		s_CachedReferences.Clear();
	}
}

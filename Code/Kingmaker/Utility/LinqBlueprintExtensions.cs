using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;

namespace Kingmaker.Utility;

public static class LinqBlueprintExtensions
{
	[CanBeNull]
	public static TFact GetFact<TFact>(this IList<TFact> source, BlueprintScriptableObject blueprint) where TFact : EntityFact
	{
		foreach (TFact item in source)
		{
			if (item.Blueprint == blueprint)
			{
				return item;
			}
		}
		return null;
	}

	public static bool HasFact<TFact>(this IList<TFact> source, BlueprintScriptableObject blueprint) where TFact : EntityFact
	{
		return source.GetFact(blueprint) != null;
	}
}

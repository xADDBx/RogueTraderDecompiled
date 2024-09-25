using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingmaker.Blueprints;

public static class BlueprintReferenceEx
{
	public static T ToReference<T>(this BlueprintScriptableObject bp) where T : BlueprintReferenceBase, new()
	{
		return BlueprintReferenceBase.CreateTyped<T>(bp);
	}

	public static IEnumerable<T> Dereference<T>(this IEnumerable<BlueprintReference<T>> list) where T : BlueprintScriptableObject
	{
		return list.Select((BlueprintReference<T> r) => (r == null) ? null : r.Get());
	}

	public static bool HasReference<T>(this IEnumerable<BlueprintReference<T>> list, T bp) where T : BlueprintScriptableObject
	{
		return list.Any((BlueprintReference<T> r) => r?.Is(bp) ?? false);
	}

	public static T FirstOrDefault<T, TRef>(this ReferenceArrayProxy<T> array) where T : BlueprintScriptableObject where TRef : BlueprintReference<T>, new()
	{
		if (array.Length <= 0)
		{
			return null;
		}
		return array[0];
	}

	public static T FirstOrDefault<T, TRef>(this ReferenceArrayProxy<T> array, Func<T, bool> predicate) where T : BlueprintScriptableObject where TRef : BlueprintReference<T>, new()
	{
		int i = 0;
		for (int length = array.Length; i < length; i++)
		{
			T val = array[i];
			if (predicate(val))
			{
				return val;
			}
		}
		return null;
	}

	public static bool Any<T, TRef>(this ReferenceArrayProxy<T> array) where T : BlueprintScriptableObject where TRef : BlueprintReference<T>, new()
	{
		return array.Length > 0;
	}
}

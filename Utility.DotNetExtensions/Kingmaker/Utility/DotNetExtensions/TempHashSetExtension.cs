using System.Collections.Generic;

namespace Kingmaker.Utility.DotNetExtensions;

public static class TempHashSetExtension
{
	public static HashSet<T> ToTempHashSet<T>(this IList<T> list)
	{
		HashSet<T> hashSet = TempHashSet.Get<T>();
		for (int i = 0; i < list.Count; i++)
		{
			T item = list[i];
			hashSet.Add(item);
		}
		return hashSet;
	}

	public static HashSet<T> ToTempHashSet<T>(this IEnumerable<T> enumerable)
	{
		HashSet<T> hashSet = TempHashSet.Get<T>();
		hashSet.AddRange(enumerable);
		return hashSet;
	}
}

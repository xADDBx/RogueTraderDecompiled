using System.Collections.Generic;

namespace Kingmaker.Utility.DotNetExtensions;

public static class HashSetExtensions
{
	public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> range)
	{
		foreach (T item in range)
		{
			set.Add(item);
		}
	}

	public static void RemoveRange<T>(this HashSet<T> set, IEnumerable<T> range)
	{
		foreach (T item in range)
		{
			set.Remove(item);
		}
	}
}

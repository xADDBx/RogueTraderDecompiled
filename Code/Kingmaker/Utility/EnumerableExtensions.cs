using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Utility;

public static class EnumerableExtensions
{
	public static List<T> GetSortedTempList<T>(this IEnumerable<T> enumerable)
	{
		List<T> list = enumerable.ToTempList();
		list.Sort();
		return list;
	}

	public static List<T> GetSortedTempList<T>(this IEnumerable<T> enumerable, Comparison<T> comparison)
	{
		List<T> list = enumerable.ToTempList();
		list.Sort(comparison);
		return list;
	}
}

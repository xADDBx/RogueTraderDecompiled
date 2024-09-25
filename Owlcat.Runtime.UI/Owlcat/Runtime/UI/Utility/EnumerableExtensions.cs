using System;
using System.Collections.Generic;

namespace Owlcat.Runtime.UI.Utility;

internal static class EnumerableExtensions
{
	internal static int IndexOf<T>(this IEnumerable<T> list, T element)
	{
		int num = 0;
		foreach (T item in list)
		{
			if (item.Equals(element))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	internal static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
	{
		foreach (T item in list)
		{
			action(item);
		}
	}

	internal static T MaxBy<T>(this IEnumerable<T> enumerable, Func<T, float> selector)
	{
		float num = float.MinValue;
		T result = default(T);
		foreach (T item in enumerable)
		{
			float num2 = selector(item);
			if (num2 > num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Core;

internal static class LinqExtensions
{
	public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> source, Action<TKey, TValue> func)
	{
		foreach (KeyValuePair<TKey, TValue> item in source)
		{
			func(item.Key, item.Value);
		}
	}

	public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> func)
	{
		foreach (TSource item in source)
		{
			func(item);
		}
	}

	public static void ForEach<TSource>(this IList<TSource> source, Action<TSource> func)
	{
		foreach (TSource item in source)
		{
			func(item);
		}
	}

	public static void ForEachWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> func)
	{
		foreach (TSource item in source)
		{
			if (!func(item))
			{
				break;
			}
		}
	}

	public static bool Empty<TSource>([CanBeNull] this IEnumerable<TSource> source)
	{
		if (source != null)
		{
			return !source.Any();
		}
		return true;
	}

	public static bool Empty<TSource>([CanBeNull] this IList<TSource> source)
	{
		if (source != null)
		{
			return source.Count == 0;
		}
		return true;
	}

	public static bool Any<TSource>([CanBeNull] this IList<TSource> source)
	{
		if (source != null)
		{
			return source.Count > 0;
		}
		return false;
	}

	[NotNull]
	public static IEnumerable<T> EmptyIfNull<T>([CanBeNull] this IEnumerable<T> list)
	{
		return list ?? Enumerable.Empty<T>();
	}

	public static IEnumerable<T> NotNull<T>([CanBeNull] this IEnumerable<T> list)
	{
		return list.Where((T i) => i != null);
	}

	public static IEnumerable<T> Valid<T>([CanBeNull] this IEnumerable<T> list) where T : UnityEngine.Object
	{
		return from i in list.EmptyIfNull()
			where i
			select i;
	}

	public static int IndexOf<T>(this IEnumerable<T> list, T obj) where T : class
	{
		int num = 0;
		foreach (T item in list)
		{
			if (item == obj)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static int FindIndex<T>(this IEnumerable<T> list, Func<T, bool> pred) where T : class
	{
		int num = 0;
		foreach (T item in list)
		{
			if (pred(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static T MaxBy<T>(this IEnumerable<T> enumerable, Func<T, int> selector)
	{
		int num = int.MinValue;
		T result = default(T);
		foreach (T item in enumerable)
		{
			int num2 = selector(item);
			if (num2 > num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	public static T MaxBy<T>(this IEnumerable<T> enumerable, Func<T, float> selector)
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

	public static T Random<T>(this IEnumerable<T> enumerable, Func<int, int, int> randomFromRange = null)
	{
		int num = 0;
		T result = default(T);
		foreach (T item in enumerable.EmptyIfNull())
		{
			if ((randomFromRange?.Invoke(0, num + 1) ?? UnityEngine.Random.Range(0, num + 1)) >= num)
			{
				result = item;
			}
			num++;
		}
		return result;
	}

	public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, Func<T, bool> pred)
	{
		foreach (T item in enumerable)
		{
			if (!pred(item))
			{
				yield return item;
			}
		}
	}

	public static T Random<T>(this IList<T> list)
	{
		if (list == null || list.Count <= 0)
		{
			return default(T);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			int index = UnityEngine.Random.Range(0, num) % num;
			num--;
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	[CanBeNull]
	public static T WeightedRandom<T>(this IList<T> list) where T : IWeighted
	{
		if (list.Count <= 0)
		{
			return default(T);
		}
		float maxInclusive = list.Select((T t) => t.Weight).Sum();
		float num = UnityEngine.Random.Range(0f, maxInclusive);
		float num2 = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			T result = list[i];
			num2 += result.Weight;
			if (num2 >= num)
			{
				return result;
			}
		}
		return list[list.Count - 1];
	}

	public static TValue Get<TKey, TValue>([CanBeNull] this IDictionary<TKey, TValue> source, TKey key, TValue defaultValue = default(TValue))
	{
		if (source == null)
		{
			return defaultValue;
		}
		if (!source.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public static TValue Get<TValue>([CanBeNull] this IList<TValue> source, int index, TValue defaultValue = default(TValue))
	{
		if (source == null)
		{
			return defaultValue;
		}
		if (index < 0 || index >= source.Count)
		{
			return defaultValue;
		}
		return source[index];
	}

	public static TValue GetClamped<TValue>([CanBeNull] this IList<TValue> source, int index)
	{
		if (source == null || source.Count == 0)
		{
			return default(TValue);
		}
		if (index < 0 || index >= source.Count)
		{
			if (index >= 0)
			{
				return source[source.Count - 1];
			}
			return source[0];
		}
		return source[index];
	}

	public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new Queue<T>(source);
	}

	public static Stack<T> ToStack<T>(this IEnumerable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new Stack<T>(source);
	}

	public static bool HasItem<TValue>([CanBeNull] this IList<TValue> source, TValue item)
	{
		if (source == null)
		{
			return false;
		}
		for (int i = 0; i < source.Count; i++)
		{
			if (EqualityComparer<TValue>.Default.Equals(source[i], item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasItem<TValue>([CanBeNull] this IList<TValue> source, Func<TValue, bool> pred)
	{
		if (source == null)
		{
			return false;
		}
		for (int i = 0; i < source.Count; i++)
		{
			if (pred(source[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Any<TSource>([CanBeNull] this IList<TSource> source, TSource item)
	{
		return source.HasItem(item);
	}

	public static bool Any<TSource>([CanBeNull] this IList<TSource> source, Func<TSource, bool> pred)
	{
		return source.HasItem(pred);
	}

	[CanBeNull]
	public static TValue FirstItem<TValue>([CanBeNull] this IList<TValue> source)
	{
		if (source == null)
		{
			return default(TValue);
		}
		if (source.Count <= 0)
		{
			return default(TValue);
		}
		return source[0];
	}

	[CanBeNull]
	public static TValue FirstItem<TValue>([CanBeNull] this IList<TValue> source, Func<TValue, bool> pred)
	{
		if (source == null)
		{
			return default(TValue);
		}
		for (int i = 0; i < source.Count; i++)
		{
			if (pred(source[i]))
			{
				return source[i];
			}
		}
		return default(TValue);
	}

	[CanBeNull]
	public static TValue FirstOrDefault<TValue>([CanBeNull] this IList<TValue> source)
	{
		return source.FirstItem();
	}

	[CanBeNull]
	public static TValue FirstOrDefault<TValue>([CanBeNull] this IList<TValue> source, Func<TValue, bool> pred)
	{
		return source.FirstItem(pred);
	}

	[CanBeNull]
	public static TValue LastItem<TValue>([CanBeNull] this IList<TValue> source)
	{
		if (source == null)
		{
			return default(TValue);
		}
		if (source.Count <= 0)
		{
			return default(TValue);
		}
		return source[source.Count - 1];
	}

	public static void SwapItemsAt<T>(this IList<T> list, int index0, int index1)
	{
		T value = list[index1];
		list[index1] = list[index0];
		list[index0] = value;
	}

	public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T item)
	{
		foreach (T item2 in source)
		{
			yield return item2;
		}
		yield return item;
	}
}

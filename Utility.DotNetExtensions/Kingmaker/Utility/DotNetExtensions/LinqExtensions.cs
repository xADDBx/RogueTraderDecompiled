using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.StatefulRandom;
using UnityEngine;

namespace Kingmaker.Utility.DotNetExtensions;

public static class LinqExtensions
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

	public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> func)
	{
		foreach (KeyValuePair<TKey, TValue> item in source)
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

	public static bool Empty<TSource>([CanBeNull] this ICollection<TSource> source)
	{
		if (source != null)
		{
			return source.Count == 0;
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
		return source.AnyItem();
	}

	public static bool AnyItem<TSource>([CanBeNull] this IList<TSource> source)
	{
		if (source != null)
		{
			return source.Count > 0;
		}
		return false;
	}

	[NotNull]
	public static TElement[] EmptyIfNull<TElement>([CanBeNull] this TElement[] list)
	{
		return list ?? Array.Empty<TElement>();
	}

	[NotNull]
	public static IEnumerable<T> EmptyIfNull<T>([CanBeNull] this IEnumerable<T> list)
	{
		return list ?? Enumerable.Empty<T>();
	}

	public static IEnumerable<T> NotNull<T>([CanBeNull] this IEnumerable<T> list)
	{
		return list?.Where((T i) => i != null);
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

	public static int IndexOf<T>(this T[] array, T value)
	{
		return Array.IndexOf(array, value);
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

	public static int FindIndex<T>(this IList<T> list, Predicate<T> pred)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (pred(list[i]))
			{
				return i;
			}
		}
		return -1;
	}

	[CanBeNull]
	public static T FindOrDefault<T>(this T[] array, Func<T, bool> pred) where T : class
	{
		foreach (T val in array)
		{
			if (pred(val))
			{
				return val;
			}
		}
		return null;
	}

	[CanBeNull]
	public static T FindOrDefault<T>(this ICollection<T> array, Func<T, bool> pred) where T : class
	{
		foreach (T item in array)
		{
			if (pred(item))
			{
				return item;
			}
		}
		return null;
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

	public static T MinBy<T>(this IEnumerable<T> enumerable, Func<T, float> selector)
	{
		float num = float.MaxValue;
		T result = default(T);
		foreach (T item in enumerable)
		{
			float num2 = selector(item);
			if (num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	[CanBeNull]
	public static T Random<T>(this IEnumerable<T> enumerable, Kingmaker.Utility.StatefulRandom.StatefulRandom statefulRandom, Func<int, int, int> randomFromRange = null)
	{
		int num = 0;
		T result = default(T);
		if (enumerable == null)
		{
			return result;
		}
		foreach (T item in enumerable)
		{
			if ((randomFromRange?.Invoke(0, num + 1) ?? statefulRandom.Range(0, num + 1)) >= num)
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

	public static T Random<T>(this IList<T> list, Kingmaker.Utility.StatefulRandom.StatefulRandom random)
	{
		if (list == null || list.Count <= 0)
		{
			return default(T);
		}
		return list[random.Range(0, list.Count)];
	}

	public static void Shuffle<T>(this IList<T> list, Kingmaker.Utility.StatefulRandom.StatefulRandom random)
	{
		int num = list.Count;
		while (num > 1)
		{
			int index = random.Range(0, num) % num;
			num--;
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	[CanBeNull]
	public static T WeightedRandom<T>(this IList<T> list, Kingmaker.Utility.StatefulRandom.StatefulRandom random) where T : IWeighted
	{
		if (list.Count <= 0)
		{
			return default(T);
		}
		float maxInclusive = list.Select((T t) => t.Weight).Sum();
		float num = random.Range(0f, maxInclusive);
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

	[CanBeNull]
	public static T MostWeighted<T>(this IEnumerable<T> list) where T : IWeighted
	{
		return list.MaxBy((T e) => e.Weight);
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

	public static bool TryFind<T>(this IEnumerable<T> source, Predicate<T> pred, out T result) where T : class
	{
		result = null;
		foreach (T item in source)
		{
			if (pred(item))
			{
				result = item;
				break;
			}
		}
		return result != null;
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

	public static bool HasItem<TValue>([CanBeNull] this HashSet<TValue> source, Func<TValue, bool> pred)
	{
		if (source == null)
		{
			return false;
		}
		foreach (TValue item in source)
		{
			if (pred(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AllItems<TValue>([CanBeNull] this IList<TValue> source, Func<TValue, bool> pred)
	{
		if (source == null)
		{
			return true;
		}
		for (int i = 0; i < source.Count; i++)
		{
			if (!pred(source[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool Contains<TValue>([CanBeNull] this IReadOnlyList<TValue> source, Func<TValue, bool> pred)
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

	public static bool Contains<TValue>([CanBeNull] this HashSet<TValue> source, Func<TValue, bool> pred)
	{
		if (source == null)
		{
			return false;
		}
		foreach (TValue item in source)
		{
			if (pred(item))
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

	public static bool AnyItem<TSource>([CanBeNull] this IList<TSource> source, TSource item)
	{
		return source.HasItem(item);
	}

	public static bool Any<TSource>([CanBeNull] this IList<TSource> source, Func<TSource, bool> pred)
	{
		return source.HasItem(pred);
	}

	public static bool AnyItem<TSource>([CanBeNull] this IList<TSource> source, Func<TSource, bool> pred)
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
		if (source == null || source.Count <= 0)
		{
			return default(TValue);
		}
		return source[source.Count - 1];
	}

	[CanBeNull]
	public static TValue LastItem<TValue>([CanBeNull] this IList<TValue> source, Func<TValue, bool> pred)
	{
		if (source == null || source.Count <= 0)
		{
			return default(TValue);
		}
		for (int num = source.Count - 1; num >= 0; num--)
		{
			if (pred(source[num]))
			{
				return source[num];
			}
		}
		return default(TValue);
	}

	[CanBeNull]
	public static TValue LastOrDefault<TValue>([CanBeNull] this IList<TValue> source)
	{
		return source.LastItem();
	}

	[CanBeNull]
	public static TValue LastOrDefault<TValue>([CanBeNull] this IList<TValue> source, Func<TValue, bool> pred)
	{
		return source.LastItem(pred);
	}

	public static bool TryPop<TValue>(this List<TValue> list, out TValue value)
	{
		int count = list.Count;
		if (0 < count)
		{
			int index = count - 1;
			value = list[index];
			list.RemoveAt(index);
			return true;
		}
		value = default(TValue);
		return false;
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

	public static IEnumerable<T> IfEmpty<T>(this IEnumerable<T> source, IEnumerable<T> target)
	{
		IEnumerable<T> enumerable = (source.Any() ? source : target);
		foreach (T item in enumerable)
		{
			yield return item;
		}
	}

	public static List<List<T>> Slice<T>([NotNull] this List<T> source, int chunkSize)
	{
		List<List<T>> list = new List<List<T>>();
		for (int i = 0; i < source.Count; i += chunkSize)
		{
			int count = Math.Min(chunkSize, source.Count - i);
			list.Add(source.GetRange(i, count));
		}
		return list;
	}

	public static void IncreaseCapacity<T>([NotNull] this List<T> list, int capacity)
	{
		if (list.Capacity < capacity)
		{
			capacity = Mathf.Max(capacity, 4);
			capacity = capacity.Pow2RoundUp();
			list.Capacity = capacity;
		}
	}

	public static void EnsureIndex<T>([NotNull] this List<T> list, int index, T value = default(T))
	{
		if (index < 0)
		{
			throw new IndexOutOfRangeException($"index={index}");
		}
		while (list.Count <= index)
		{
			list.Add(value);
		}
	}

	public static int TryCount<T>([CanBeNull] this T[] array)
	{
		if (array == null)
		{
			return 0;
		}
		return array.Length;
	}

	public static bool TryGet<T>([CanBeNull] this T[] array, int index, out T element) where T : class
	{
		if (array != null && 0 <= index && index < array.Length)
		{
			element = array[index];
			return true;
		}
		element = null;
		return false;
	}

	public static T[] SubArray<T>(this T[] data, int index, int length)
	{
		T[] array = new T[length];
		Array.Copy(data, index, array, 0, length);
		return array;
	}

	public static bool IsValidIndex<T>(this T[] data, int index)
	{
		if (0 <= index)
		{
			return index < data.TryCount();
		}
		return false;
	}

	public static bool AddUnique<T>([NotNull] this List<T> list, T value)
	{
		if (list.Contains(value))
		{
			return false;
		}
		list.Add(value);
		return true;
	}

	public static int TryCount<T>([CanBeNull] this List<T> list)
	{
		return list?.Count ?? 0;
	}

	public static bool TryGet<T>([CanBeNull] this List<T> list, int index, [CanBeNull] out T element)
	{
		if (list != null && 0 <= index && index < list.Count)
		{
			element = list[index];
			return true;
		}
		element = default(T);
		return false;
	}

	public static bool IsValidIndex<T>(this List<T> list, int index)
	{
		if (0 <= index)
		{
			return index < list.TryCount();
		}
		return false;
	}

	public static void AddRange<T>([NotNull] this List<T> list, T[] array, int startIndex, int length)
	{
		if (array.Length <= startIndex)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (array.Length < startIndex + length)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		for (int i = startIndex; i < length; i++)
		{
			list.Add(array[i]);
		}
	}

	public static TSource SingleItem<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				TSource current = enumerator.Current;
				if (!predicate(current))
				{
					continue;
				}
				while (enumerator.MoveNext())
				{
					if (predicate(enumerator.Current))
					{
						PFLog.Default.Error("Sequence contains more than one suitable items: {0}, {1}", current, enumerator.Current);
						break;
					}
				}
				return current;
			}
		}
		return default(TSource);
	}

	public static void Remove<T>(this IList<T> source, Predicate<T> predicate)
	{
		for (int i = 0; i < source.Count; i++)
		{
			T obj = source[i];
			if (predicate(obj))
			{
				source.RemoveAt(i);
				break;
			}
		}
	}

	public static void RemoveAll<T>(this IList<T> source, Predicate<T> predicate)
	{
		for (int i = 0; i < source.Count; i++)
		{
			T obj = source[i];
			if (predicate(obj))
			{
				source.RemoveAt(i);
				i--;
			}
		}
	}

	public static void RemoveLast<T>(this IList<T> source)
	{
		source.RemoveAt(source.Count - 1);
	}

	public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, params T[] @params)
	{
		foreach (T item in source)
		{
			yield return item;
		}
		for (int i = 0; i < @params.Length; i++)
		{
			yield return @params[i];
		}
	}

	public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
	{
		key = tuple.Key;
		value = tuple.Value;
	}

	public static IEnumerable<T> Except<T>(this IEnumerable<T> source, params T[] @params)
	{
		return source.Where((T i) => !@params.HasItem(i));
	}

	public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
	{
		return source.Select((T item, int index) => (item: item, index: index));
	}

	public static void InsertIntoSortedList<T>(this IList list, T value, Comparison<T> comparison)
	{
		int num = 0;
		int num2 = list.Count;
		while (num2 > num)
		{
			int num3 = num2 - num;
			int num4 = num + num3 / 2;
			T x = (T)list[num4];
			int num5 = comparison(x, value);
			if (num5 == 0)
			{
				list.Insert(num4, value);
				return;
			}
			if (num5 < 0)
			{
				num = num4 + 1;
			}
			else
			{
				num2 = num4;
			}
		}
		list.Insert(num, value);
	}
}

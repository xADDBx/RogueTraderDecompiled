using System;
using System.Collections.Generic;

namespace Owlcat.Runtime.Core.Utility;

public static class TempListExtension
{
	public static List<T> ToTempList<T>(this List<T> _this)
	{
		List<T> list = TempList.Get<T>();
		list.Capacity = System.Math.Max(list.Capacity, _this.Capacity);
		foreach (T _thi in _this)
		{
			list.Add(_thi);
		}
		return list;
	}

	public static List<KeyValuePair<TKey, TValue>> ToTempList<TKey, TValue>(this Dictionary<TKey, TValue> _this)
	{
		List<KeyValuePair<TKey, TValue>> list = TempList.Get<KeyValuePair<TKey, TValue>>();
		foreach (KeyValuePair<TKey, TValue> _thi in _this)
		{
			list.Add(_thi);
		}
		return list;
	}

	public static List<T> ToTempList<T>(this IEnumerable<T> _this)
	{
		List<T> list = TempList.Get<T>();
		foreach (T _thi in _this)
		{
			list.Add(_thi);
		}
		return list;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Kingmaker.Utility;

[JsonObject]
public class TagList<T> : TagListBase, IEnumerable<T>, IEnumerable
{
	public bool[] Values;

	private static int[] ValuesIntArray { get; } = Enum.GetValues(typeof(T)).Cast<int>().ToArray();


	private static T[] ValuesArray { get; } = Enum.GetValues(typeof(T)).Cast<T>().ToArray();


	public bool this[T tag]
	{
		get
		{
			return HasTag(tag);
		}
		set
		{
			SetTag(tag, value);
		}
	}

	public bool HasTag(T tag)
	{
		int num = Convert.ToInt32(tag);
		if (num >= 0 && num < Values.Length)
		{
			return Values[num];
		}
		return false;
	}

	public void SetTag(T tag, bool value)
	{
		int num = Convert.ToInt32(tag);
		if (num >= 0)
		{
			if (Values == null)
			{
				Values = new bool[ValuesIntArray.Max() + 1];
			}
			if (Values.Length < num + 1)
			{
				Array.Resize(ref Values, num + 1);
			}
			Values[num] = value;
		}
	}

	public bool HasAll(TagList<T> other)
	{
		if (other?.Values == null)
		{
			return true;
		}
		for (int i = 0; i < Values.Length && i < other.Values.Length; i++)
		{
			if (other.Values[i] && !Values[i])
			{
				return false;
			}
		}
		return true;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int ii = 0; ii < ValuesIntArray.Length; ii++)
		{
			int num = ValuesIntArray[ii];
			if (num >= 0 && num < Values.Length && Values[num])
			{
				yield return ValuesArray[ii];
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

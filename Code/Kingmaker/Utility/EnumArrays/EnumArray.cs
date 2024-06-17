using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.Utility.EnumArrays;

public class EnumArray<TValue>
{
	[SerializeField]
	private TValue[] m_Values;

	public TValue[] Values => m_Values;

	public TValue this[int i] => m_Values[i];

	public TValue GetValue<TEnum>(TEnum enumIndex) where TEnum : Enum
	{
		IOrderedEnumerable<TEnum> orderedEnumerable = (Enum.GetValues(typeof(TEnum)) as IEnumerable<TEnum>).OrderBy((TEnum e) => Convert.ToInt32(e));
		int num = 0;
		foreach (TEnum item in orderedEnumerable)
		{
			if (object.Equals(item, enumIndex))
			{
				return this[num];
			}
			num++;
		}
		return this[0];
	}

	public static implicit operator TValue[](EnumArray<TValue> enumArray)
	{
		return enumArray.m_Values;
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Kingmaker.Utility.Enum;

public class EnumHelper<TEnum> where TEnum : System.Enum, IConvertible
{
	public static readonly EnumHelper<TEnum> Instance = new EnumHelper<TEnum>();

	private readonly (TEnum, int)[] m_Values;

	private EnumHelper()
	{
		TEnum[] array = (TEnum[])System.Enum.GetValues(typeof(TEnum));
		int num = array.Length;
		m_Values = new(TEnum, int)[num];
		for (int i = 0; i < num; i++)
		{
			TEnum item = array[i];
			m_Values[i] = (item, item.ToInt32(CultureInfo.InvariantCulture));
		}
	}

	public TEnum FromInt32(int value)
	{
		int i = 0;
		for (int num = m_Values.Length; i < num; i++)
		{
			(TEnum, int) tuple = m_Values[i];
			if (tuple.Item2 == value)
			{
				return tuple.Item1;
			}
		}
		return default(TEnum);
	}

	public int ToInt32(TEnum value)
	{
		EqualityComparer<TEnum> @default = EqualityComparer<TEnum>.Default;
		int i = 0;
		for (int num = m_Values.Length; i < num; i++)
		{
			(TEnum, int) tuple = m_Values[i];
			if (@default.Equals(tuple.Item1, value))
			{
				return tuple.Item2;
			}
		}
		return 0;
	}
}

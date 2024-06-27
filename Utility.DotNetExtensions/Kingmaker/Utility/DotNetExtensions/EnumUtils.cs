using System;
using System.Collections.Generic;
using System.Linq;

namespace Kingmaker.Utility.DotNetExtensions;

public static class EnumUtils
{
	public static IEnumerable<TEnum> GetValues<TEnum>()
	{
		if (!typeof(TEnum).IsEnum)
		{
			return Enumerable.Empty<TEnum>();
		}
		return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
	}

	public static IEnumerable<Enum> GetValues(Type enumType)
	{
		if (!enumType.IsEnum)
		{
			return Enumerable.Empty<Enum>();
		}
		return Enum.GetValues(enumType).Cast<Enum>().OrderBy(Convert.ToInt32);
	}

	public static IEnumerable<Enum> GetValues64(Type enumType)
	{
		if (!enumType.IsEnum)
		{
			return Enumerable.Empty<Enum>();
		}
		return Enum.GetValues(enumType).Cast<Enum>().OrderBy(Convert.ToInt64);
	}

	public static int GetMaxValue<TEnum>() where TEnum : Enum
	{
		return Enum.GetValues(typeof(TEnum)).Cast<int>().Max() + 1;
	}

	public static int GetOrdinalNumber<TEnum>(TEnum value) where TEnum : Enum
	{
		return Enum.GetValues(typeof(TEnum)).Cast<Enum>().OrderBy(Convert.ToInt32)
			.FindIndex((Enum e) => object.Equals(e, value));
	}

	public static TEnum GetValueInOrder<TEnum>(int order) where TEnum : Enum
	{
		return (TEnum)Enum.GetValues(typeof(TEnum)).Cast<Enum>().OrderBy(Convert.ToInt32)
			.ElementAt(order);
	}
}

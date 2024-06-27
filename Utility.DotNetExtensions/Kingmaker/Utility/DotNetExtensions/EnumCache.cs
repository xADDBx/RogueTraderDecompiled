using System;

namespace Kingmaker.Utility.DotNetExtensions;

public static class EnumCache<TEnum> where TEnum : Enum
{
	private static readonly TEnum[] s_Values;

	public static TEnum[] Values => s_Values;

	static EnumCache()
	{
		Array values = Enum.GetValues(typeof(TEnum));
		int length = values.Length;
		s_Values = new TEnum[length];
		for (int i = 0; i < length; i++)
		{
			s_Values[i] = (TEnum)values.GetValue(i);
		}
	}
}

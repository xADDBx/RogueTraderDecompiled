using System;

namespace Owlcat.Plugins.DotNetExtensions;

public static class StringUtility
{
	public static bool IsNullOrInvisible(string str)
	{
		return string.IsNullOrWhiteSpace(str);
	}

	public static bool IsNullOrEmpty(this string _this)
	{
		return string.IsNullOrEmpty(_this);
	}

	public static string EmptyToNull(this string @this)
	{
		if (!(@this != ""))
		{
			return null;
		}
		return @this;
	}

	public static string RemoveFirstOccurrence(string str, string substringToRemove, bool removeIfStartsWith = false)
	{
		if (removeIfStartsWith && !str.StartsWith(substringToRemove))
		{
			return str;
		}
		int num = str.IndexOf(substringToRemove, StringComparison.InvariantCulture);
		if (num != -1)
		{
			str = str.Substring(num + substringToRemove.Length);
		}
		return str;
	}

	public static int GetMatchingPrefixLength(string str1, string str2)
	{
		int num = Math.Min(str1.Length, str2.Length);
		int num2 = 0;
		for (int i = 0; i < num && str1[i] == str2[i]; i++)
		{
			num2++;
		}
		return num2;
	}

	public static int LengthWithoutWhitespace(this string str)
	{
		int num = 0;
		for (int i = 0; i < str.Length; i++)
		{
			if (!char.IsWhiteSpace(str[i]))
			{
				num++;
			}
		}
		return num;
	}
}

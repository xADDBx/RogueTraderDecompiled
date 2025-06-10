using System;

namespace Kingmaker.Utility.DotNetExtensions;

public static class StringArrayExtensions
{
	public static string GetCommonPrefix(this string[] lines)
	{
		if (lines.Length == 0)
		{
			return string.Empty;
		}
		Array.Sort(lines);
		string text = string.Empty;
		string text2 = lines[0];
		for (int i = 0; i < text2.Length; i++)
		{
			char c = text2[i];
			for (int j = 1; j < lines.Length; j++)
			{
				string text3 = lines[j];
				if (i >= text3.Length || text3[i] != c)
				{
					return text;
				}
			}
			text += c;
		}
		return text;
	}
}

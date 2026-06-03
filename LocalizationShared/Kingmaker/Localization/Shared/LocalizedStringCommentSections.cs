using System;

namespace Kingmaker.Localization.Shared;

public static class LocalizedStringCommentSections
{
	public const string StringTag = "[string]";

	public const string BlueprintTag = "[blueprint]";

	private const string SectionSeparator = "\n\n";

	public static (string stringPart, string blueprintPart) Parse(string raw)
	{
		if (string.IsNullOrEmpty(raw))
		{
			return (stringPart: string.Empty, blueprintPart: string.Empty);
		}
		int num = raw.IndexOf("[blueprint]", StringComparison.Ordinal);
		string text;
		string text2;
		if (num >= 0)
		{
			text = raw.Substring(0, num);
			int num2 = num + "[blueprint]".Length;
			text2 = raw.Substring(num2, raw.Length - num2);
		}
		else
		{
			text = raw;
			text2 = string.Empty;
		}
		text = StripTag(text, "[string]").Trim();
		text2 = text2.Trim();
		return (stringPart: text, blueprintPart: text2);
	}

	public static string Compose(string stringPart, string blueprintPart)
	{
		stringPart = stringPart?.Trim() ?? string.Empty;
		blueprintPart = blueprintPart?.Trim() ?? string.Empty;
		bool flag = !string.IsNullOrEmpty(stringPart);
		bool flag2 = !string.IsNullOrEmpty(blueprintPart);
		if (!flag && !flag2)
		{
			return string.Empty;
		}
		if (flag && flag2)
		{
			return "[string] " + stringPart + "\n\n[blueprint] " + blueprintPart;
		}
		if (flag)
		{
			return "[string] " + stringPart;
		}
		return "[blueprint] " + blueprintPart;
	}

	public static string ReplaceStringPart(string raw, string newStringPart)
	{
		string item = Parse(raw).blueprintPart;
		return Compose(newStringPart, item);
	}

	public static string ReplaceBlueprintPart(string raw, string newBlueprintPart)
	{
		return Compose(Parse(raw).stringPart, newBlueprintPart);
	}

	private static string StripTag(string text, string tag)
	{
		text = text.TrimStart();
		if (!text.StartsWith(tag, StringComparison.Ordinal))
		{
			return text;
		}
		string text2 = text;
		int length = tag.Length;
		return text2.Substring(length, text2.Length - length);
	}
}

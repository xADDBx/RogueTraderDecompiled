using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Localization.Shared;

public static class TraitUtility
{
	private const string StringTraitsPathMods = "\\..\\Localization\\traits-string.txt";

	private const string LocaleTraitsPathMods = "\\..\\Localization\\traits-locale.txt";

	private const string StringTraitsPath = "\\..\\..\\Localization\\traits-string.txt";

	private const string LocaleTraitsPath = "\\..\\..\\Localization\\traits-locale.txt";

	[NotNull]
	private static readonly LocaleTrait[] ValuesEnum = new LocaleTrait[4]
	{
		LocaleTrait.CheckMe,
		LocaleTrait.Translated,
		LocaleTrait.Relevant,
		LocaleTrait.Final
	};

	[NotNull]
	public static readonly string[] Values = ValuesEnum.Select((LocaleTrait v) => v.ToString()).ToArray();

	private static string[] s_StringTraits;

	private static string[] s_LocaleTraits;

	public static string[] StringTraits => s_StringTraits ?? (s_StringTraits = LoadStringTraits());

	public static string[] LocaleTraits => s_LocaleTraits ?? (s_LocaleTraits = LoadLocaleTraits());

	public static void ReloadTraits()
	{
		s_StringTraits = LoadStringTraits();
		s_LocaleTraits = LoadLocaleTraits();
	}

	private static string[] LoadStringTraits()
	{
		string text = Application.dataPath + "\\..\\..\\Localization\\traits-string.txt";
		if (!File.Exists(text))
		{
			text = Application.dataPath + "\\..\\Localization\\traits-string.txt";
			if (!File.Exists(text))
			{
				throw new Exception("String Traits file not found.Expected path : \\..\\..\\Localization\\traits-string.txt For mods Template : \\..\\Localization\\traits-string.txt");
			}
		}
		return File.ReadAllLines(text ?? "");
	}

	private static string[] LoadLocaleTraits()
	{
		string text = Application.dataPath + "\\..\\..\\Localization\\traits-locale.txt";
		if (!File.Exists(text))
		{
			text = Application.dataPath + "\\..\\Localization\\traits-locale.txt";
			if (!File.Exists(text))
			{
				throw new Exception("Locale Traits file not found.Expected path : \\..\\..\\Localization\\traits-locale.txt For mods Template : \\..\\Localization\\traits-locale.txt");
			}
		}
		return File.ReadAllLines(text ?? "");
	}
}

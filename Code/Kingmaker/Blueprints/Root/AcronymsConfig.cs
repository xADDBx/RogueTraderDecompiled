using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Localization.Enums;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class AcronymsConfig
{
	public string enGBWordsToExclude;

	public string ruRUWordsToExclude;

	public string deDEWordsToExclude;

	public string frFRWordsToExclude;

	public string zhCNWordsToExclude;

	public string esESWordsToExclude;

	public string jaJPWordsToExclude;

	public string trTRWordsToExclude;

	private static readonly List<string> CommonSymbols = new List<string>
	{
		"+", "-", "0", "1", "2", "3", "4", "5", "6", "7",
		"8", "9", " — ", "!"
	};

	public List<string> GetWordsToExcludeFor(Locale locale)
	{
		string text = locale switch
		{
			Locale.enGB => enGBWordsToExclude, 
			Locale.ruRU => ruRUWordsToExclude, 
			Locale.deDE => deDEWordsToExclude, 
			Locale.frFR => frFRWordsToExclude, 
			Locale.zhCN => zhCNWordsToExclude, 
			Locale.esES => esESWordsToExclude, 
			Locale.jaJP => jaJPWordsToExclude, 
			Locale.trTR => trTRWordsToExclude, 
			Locale.Sound => string.Empty, 
			Locale.dev => string.Empty, 
			_ => throw new ArgumentOutOfRangeException("locale", locale, null), 
		};
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(text))
		{
			list = text.Split(',').ToList();
		}
		list.AddRange(CommonSymbols);
		return list;
	}

	public void GetLettersInAcronym(Locale locale, out int maxLettersCount, out int preferredLettersCount)
	{
		maxLettersCount = 3;
		preferredLettersCount = 2;
		if (locale == Locale.zhCN)
		{
			maxLettersCount = 1;
			preferredLettersCount = 1;
		}
	}
}

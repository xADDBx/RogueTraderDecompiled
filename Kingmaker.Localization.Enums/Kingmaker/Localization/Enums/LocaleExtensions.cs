using System.Globalization;
using JetBrains.Annotations;

namespace Kingmaker.Localization.Enums;

public static class LocaleExtensions
{
	[NotNull]
	public static Locale[] GameLanguageValues { get; } = new Locale[8]
	{
		Locale.enGB,
		Locale.ruRU,
		Locale.deDE,
		Locale.frFR,
		Locale.esES,
		Locale.zhCN,
		Locale.jaJP,
		Locale.trTR
	};


	public static string GetForbiddenRegexp(this Locale locale)
	{
		return locale switch
		{
			Locale.enGB => "([а-яА-Я]+[\\s]*)+", 
			Locale.ruRU => "([a-zA-Z]+[\\s]*)+(?![^{]*})(?![^<]*>)", 
			Locale.deDE => "([а-яА-Я]+[\\s]*)+", 
			Locale.frFR => "([а-яА-Я]+[\\s]*)+", 
			Locale.zhCN => "([а-яА-Я]+[\\s]*)+", 
			Locale.esES => "([а-яА-Я]+[\\s]*)+", 
			Locale.Sound => "([а-яА-Я]+[\\s]*)+", 
			Locale.jaJP => "([а-яА-Я]+[\\s]*)+", 
			Locale.trTR => "([а-яА-Я]+[\\s]*)+", 
			Locale.dev => "([а-яА-Я]+[\\s]*)+", 
			_ => "", 
		};
	}

	public static CultureInfo GetCulture(this Locale locale)
	{
		return locale switch
		{
			Locale.enGB => new CultureInfo("en-GB"), 
			Locale.ruRU => new CultureInfo("ru-RU"), 
			Locale.deDE => new CultureInfo("de-DE"), 
			Locale.frFR => new CultureInfo("fr-FR"), 
			Locale.zhCN => new CultureInfo("zh-CN"), 
			Locale.esES => new CultureInfo("es-ES"), 
			Locale.jaJP => new CultureInfo("ja-JP"), 
			Locale.trTR => new CultureInfo("tr-TR"), 
			Locale.Sound => new CultureInfo("en-GB"), 
			Locale.dev => new CultureInfo("en-GB"), 
			_ => new CultureInfo("en-GB"), 
		};
	}
}

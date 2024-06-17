using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Localization.Enums;
using Newtonsoft.Json;

namespace Kingmaker.Localization.Shared;

[JsonObject(MemberSerialization.OptIn)]
public class LocalizedStringData
{
	[JsonProperty(PropertyName = "source")]
	public Locale Source;

	[JsonProperty(PropertyName = "packingGroup", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public string PackingGroupId;

	[NotNull]
	[JsonProperty(PropertyName = "key")]
	public string Key = "";

	[NotNull]
	[JsonProperty(PropertyName = "comment", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[DefaultValue("")]
	public string Comment = "";

	[NotNull]
	[JsonProperty(PropertyName = "speaker", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[DefaultValue("")]
	public string Speaker = "";

	[NotNull]
	[JsonProperty(PropertyName = "speakerGender", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[DefaultValue("")]
	public string SpeakerGender = "";

	[NotNull]
	[JsonProperty(PropertyName = "ownerGuid", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[DefaultValue("")]
	public string OwnerGuid = "";

	[NotNull]
	[JsonProperty(PropertyName = "languages")]
	public List<LocaleData> Languages = new List<LocaleData>();

	[CanBeNull]
	[JsonProperty(PropertyName = "string_traits", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[DefaultValue(null)]
	public List<TraitData> StringTraits;

	public LocalizedStringData(Locale source, string key)
	{
		Source = source;
		Key = key;
	}

	[JsonConstructor]
	public LocalizedStringData()
	{
	}

	public string GetText(Locale locale)
	{
		TryGetText(locale, out var text);
		return text;
	}

	public bool TryGetText(Locale locale, out string text)
	{
		LocaleData locale2 = GetLocale(locale);
		text = locale2?.Text ?? "";
		return locale2?.Text != null;
	}

	public string[] GetTraits(Locale locale)
	{
		LocaleData locale2 = GetLocale(locale);
		if (locale2 == null)
		{
			return Array.Empty<string>();
		}
		if (locale2.Traits == null)
		{
			return Array.Empty<string>();
		}
		return locale2.Traits.Select((TraitData t) => t.Trait).ToArray();
	}

	public string[] GetStringTraits()
	{
		if (StringTraits == null)
		{
			return Array.Empty<string>();
		}
		return StringTraits.Select((TraitData t) => t.Trait).ToArray();
	}

	[CanBeNull]
	public LocaleData GetLocale(Locale locale)
	{
		foreach (LocaleData language in Languages)
		{
			if (language.Locale == locale)
			{
				return language;
			}
		}
		return null;
	}

	[CanBeNull]
	public TraitData GetTraitData(Locale locale, string trait)
	{
		return GetLocale(locale)?.Traits?.FirstOrDefault((TraitData td) => td.Trait == trait);
	}

	[CanBeNull]
	public TraitData GetStringTraitData(string trait)
	{
		return StringTraits?.FirstOrDefault((TraitData td) => td.Trait == trait);
	}

	public bool UpdateText(Locale locale, string text, bool updateDate = true)
	{
		text = ApplyFixups(locale, text);
		bool result = false;
		LocaleData localeData = GetLocale(locale);
		if (localeData == null)
		{
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			localeData = new LocaleData(locale);
			if (Languages.Count == 0)
			{
				Source = locale;
			}
			Languages.Add(localeData);
			result = true;
		}
		if (localeData.Text != text)
		{
			localeData.Text = text;
			if (updateDate)
			{
				localeData.ModificationDate = DateTimeOffset.UtcNow;
			}
			result = true;
		}
		return result;
	}

	public bool ReapplyFixups()
	{
		bool result = false;
		foreach (LocaleData language in Languages)
		{
			string text = ApplyFixups(language.Locale, language.Text);
			if (text != language.Text)
			{
				language.Text = text;
				result = true;
			}
			text = ApplyFixups(language.TranslatedFrom ?? language.Locale, language.OriginalText);
			if (text != language.OriginalText)
			{
				language.OriginalText = text;
				result = true;
			}
			if (language.Traits == null)
			{
				continue;
			}
			foreach (TraitData trait in language.Traits)
			{
				text = ApplyFixups(language.Locale, trait.LocaleText);
				if (text != trait.LocaleText)
				{
					trait.LocaleText = text;
					result = true;
				}
			}
		}
		return result;
	}

	private static string ApplyFixups(Locale locale, string text)
	{
		if (text == null)
		{
			return "";
		}
		if (locale == Locale.ruRU || locale == Locale.enGB)
		{
			text = text.Replace("«", "\"");
			text = text.Replace("»", "\"");
			text = text.Replace("“", "\"");
			text = text.Replace("”", "\"");
			text = text.Replace("’", "'");
		}
		text = text.Replace(" - ", " — ");
		text = text.Replace("\r", "");
		text = text.TrimEnd();
		while (text.Contains("  "))
		{
			text = text.Replace("  ", " ");
		}
		while (text.Contains(" \n"))
		{
			text = text.Replace(" \n", "\n");
		}
		while (text.Contains("\n "))
		{
			text = text.Replace("\n ", "\n");
		}
		while (text.Contains("\n\n"))
		{
			text = text.Replace("\n\n", "\n");
		}
		return text;
	}

	public void UpdateTranslation(Locale locale, string text, Locale translatedFrom, string originalText)
	{
		UpdateText(locale, text);
		LocaleData locale2 = GetLocale(locale);
		if (locale2 != null && locale != translatedFrom)
		{
			locale2.TranslatedFrom = translatedFrom;
			locale2.OriginalText = originalText;
			locale2.TranslationDate = DateTimeOffset.UtcNow;
		}
	}

	public void AddTrait(Locale locale, string trait)
	{
		LocaleData locale2 = GetLocale(locale);
		if (locale2 != null)
		{
			LocaleData localeData = locale2;
			if (localeData.Traits == null)
			{
				localeData.Traits = new List<TraitData>();
			}
			TraitData traitData = locale2.Traits.FirstOrDefault((TraitData t) => t.Trait == trait);
			if (traitData == null)
			{
				traitData = new TraitData(trait);
				locale2.Traits.Add(traitData);
			}
			traitData.LocaleText = locale2.Text;
			traitData.ModificationDate = DateTimeOffset.UtcNow;
		}
	}

	public void RemoveTrait(Locale locale, string trait)
	{
		LocaleData locale2 = GetLocale(locale);
		locale2?.Traits?.RemoveAll((TraitData t) => t.Trait == trait);
		List<TraitData> list = locale2?.Traits;
		if (list != null && list.Count == 0)
		{
			locale2.Traits = null;
		}
	}

	public bool HasStringTrait(string trait)
	{
		if (StringTraits != null)
		{
			return StringTraits.Any((TraitData t) => t.Trait == trait);
		}
		return false;
	}

	public bool HasLocaleTrait(Locale locale, string trait)
	{
		LocaleData locale2 = GetLocale(locale);
		if (locale2 != null)
		{
			List<TraitData> traits = locale2.Traits;
			if (traits != null)
			{
				return traits.Any((TraitData t) => t.Trait == trait);
			}
		}
		return false;
	}

	public void AddStringTrait(string trait)
	{
		if (StringTraits == null)
		{
			StringTraits = new List<TraitData>();
		}
		TraitData traitData = StringTraits.FirstOrDefault((TraitData t) => t.Trait == trait);
		if (traitData == null)
		{
			traitData = new TraitData(trait);
			StringTraits.Add(traitData);
		}
		traitData.ModificationDate = DateTimeOffset.UtcNow;
	}

	public void RemoveStringTrait(string trait)
	{
		StringTraits?.RemoveAll((TraitData t) => t.Trait == trait);
		List<TraitData> stringTraits = StringTraits;
		if (stringTraits != null && stringTraits.Count == 0)
		{
			StringTraits = null;
		}
	}

	public bool UpdateComment(string comment)
	{
		if (Comment == comment)
		{
			return false;
		}
		Comment = comment;
		return true;
	}
}

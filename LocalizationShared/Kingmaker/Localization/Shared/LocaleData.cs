using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Kingmaker.Localization.Enums;
using Newtonsoft.Json;

namespace Kingmaker.Localization.Shared;

[JsonObject(MemberSerialization.OptIn)]
public class LocaleData
{
	[JsonProperty(PropertyName = "locale")]
	public Locale Locale;

	[NotNull]
	[JsonProperty(PropertyName = "text")]
	public string Text = "";

	[JsonProperty(PropertyName = "modification_date")]
	public DateTimeOffset ModificationDate;

	[CanBeNull]
	[JsonProperty(PropertyName = "translated_from", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[DefaultValue(null)]
	public Locale? TranslatedFrom;

	[CanBeNull]
	[JsonProperty(PropertyName = "translation_date", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[DefaultValue(null)]
	public DateTimeOffset? TranslationDate;

	[NotNull]
	[JsonProperty(PropertyName = "original_text", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[DefaultValue("")]
	public string OriginalText = "";

	[CanBeNull]
	[JsonProperty(PropertyName = "translation_comment", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[DefaultValue("")]
	public string TranslationComment = "";

	[CanBeNull]
	[JsonProperty(PropertyName = "traits", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	[DefaultValue(null)]
	public List<TraitData> Traits;

	public bool HasTrait(string trait)
	{
		if (Traits != null)
		{
			return Traits.Any((TraitData t) => t.Trait == trait);
		}
		return false;
	}

	public LocaleData(Locale locale)
	{
		Locale = locale;
		ModificationDate = DateTimeOffset.UtcNow;
	}

	[JsonConstructor]
	public LocaleData()
	{
	}

	public bool IsTextValid()
	{
		string forbiddenRegexp = Locale.GetForbiddenRegexp();
		return !Regex.Match(Text, forbiddenRegexp).Success;
	}
}

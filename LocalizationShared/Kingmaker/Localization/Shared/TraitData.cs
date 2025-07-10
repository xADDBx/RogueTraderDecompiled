using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Kingmaker.Localization.Shared;

[JsonObject(MemberSerialization.OptIn)]
public class TraitData
{
	[NotNull]
	[JsonProperty(PropertyName = "trait")]
	public readonly string Trait;

	[JsonProperty(PropertyName = "trait_date")]
	public DateTimeOffset ModificationDate;

	[NotNull]
	[JsonProperty(PropertyName = "locale_text", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	public string LocaleText = "";

	[NotNull]
	[JsonProperty(PropertyName = "speaker", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	public string Speaker;

	[NotNull]
	[JsonProperty(PropertyName = "speaker_gender", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	public string SpeakerGender;

	[JsonConstructor]
	public TraitData()
	{
	}

	public TraitData([NotNull] string trait)
	{
		Trait = trait;
	}
}

using System;

namespace Kingmaker.Localization.Shared;

public class StringCreateTemplateAttribute : Attribute
{
	public enum StringType
	{
		UnitName,
		UIText,
		ItemText,
		MapObject
	}

	public StringType Type { get; }

	public StringCreateTemplateAttribute(StringType type)
	{
		Type = type;
	}
}

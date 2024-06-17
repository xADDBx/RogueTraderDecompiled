using System;

namespace Kingmaker.Localization.Shared;

public class StringCreateWindowAttribute : Attribute
{
	public enum StringType
	{
		Invalid,
		Bark,
		MapMarker,
		UIText
	}

	public bool GetNameFromAsset;

	public StringType Type { get; }

	public StringCreateWindowAttribute(StringType type)
	{
		Type = type;
	}
}

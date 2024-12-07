using System;

namespace Kingmaker.Localization.Shared;

public class StringCreateWindowAttribute : Attribute
{
	public enum StringType
	{
		Invalid,
		Action,
		Bark,
		Buff,
		EntryPoint,
		Item,
		LocationName,
		Name,
		Other,
		MapMarker,
		UIText
	}

	public bool GetNameFromAsset = true;

	public StringType Type { get; }

	public StringCreateWindowAttribute(StringType type)
	{
		Type = type;
	}
}

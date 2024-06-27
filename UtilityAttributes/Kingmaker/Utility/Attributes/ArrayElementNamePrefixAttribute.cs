using UnityEngine;

namespace Kingmaker.Utility.Attributes;

public class ArrayElementNamePrefixAttribute : PropertyAttribute, ITitleAttribute
{
	public string Prefix { get; }

	public bool StartWithOne { get; }

	public ArrayElementNamePrefixAttribute(string prefix, bool startWithOne = false)
	{
		Prefix = prefix;
		StartWithOne = startWithOne;
	}

	public string GetName(int index)
	{
		return $"{Prefix} {index + (StartWithOne ? 1 : 0)}";
	}
}

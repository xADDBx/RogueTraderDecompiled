using System;

namespace Kingmaker.Cheats;

[AttributeUsage(AttributeTargets.Method)]
public class CustomIndexerAttribute : Attribute
{
	public string Prefix { get; }

	public CustomIndexerAttribute(string prefix)
	{
		Prefix = prefix;
	}
}

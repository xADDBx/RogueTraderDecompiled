using System;

namespace Kingmaker.Cheats;

[AttributeUsage(AttributeTargets.Method)]
public class RootFinderAttribute : Attribute
{
	public string Prefix { get; }

	public RootFinderAttribute(string prefix)
	{
		Prefix = prefix;
	}
}

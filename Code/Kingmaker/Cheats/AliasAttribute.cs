using System;

namespace Kingmaker.Cheats;

[AttributeUsage(AttributeTargets.Method)]
public class AliasAttribute : Attribute
{
	public string Prefix { get; }

	public AliasAttribute(string prefix)
	{
		Prefix = prefix;
	}
}

using System;

namespace Kingmaker.ElementsSystem;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public class KDBAttribute : Attribute
{
	public readonly string Text;

	public KDBAttribute(string text)
	{
	}
}

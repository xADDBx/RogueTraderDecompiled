using System;

namespace Owlcat.Utility.Attributes;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class EnumFilterAttribute : Attribute
{
	public readonly string EnumValuesSource;

	public EnumFilterAttribute(string enumValuesSource)
	{
		EnumValuesSource = enumValuesSource;
	}
}

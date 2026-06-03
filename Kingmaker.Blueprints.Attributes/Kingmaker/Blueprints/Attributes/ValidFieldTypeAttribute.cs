using System;

namespace Kingmaker.Blueprints.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ValidFieldTypeAttribute : Attribute
{
	public readonly Type Type;

	public ValidFieldTypeAttribute(Type type)
	{
		Type = type;
	}
}

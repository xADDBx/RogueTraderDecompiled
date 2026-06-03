using System;

namespace Kingmaker.Blueprints.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class IncompatibleWithAttribute : Attribute
{
	public readonly Type Type;

	public IncompatibleWithAttribute(Type type)
	{
		Type = type;
	}
}

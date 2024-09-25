using System;

namespace Kingmaker.Blueprints.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AllowedOnAttribute : Attribute
{
	public readonly Type Type;

	public AllowedOnAttribute(Type type)
	{
		Type = type;
	}
}

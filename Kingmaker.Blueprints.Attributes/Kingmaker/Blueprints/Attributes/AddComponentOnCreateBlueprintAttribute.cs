using System;

namespace Kingmaker.Blueprints.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AddComponentOnCreateBlueprintAttribute : Attribute
{
	public readonly Type Type;

	public AddComponentOnCreateBlueprintAttribute(Type type)
	{
		Type = type;
	}
}

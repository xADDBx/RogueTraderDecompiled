using System;

namespace Kingmaker.Blueprints.Attributes;

public class ComponentNameAttribute : Attribute
{
	public readonly string Name;

	public ComponentNameAttribute(string name)
	{
		Name = name;
	}
}

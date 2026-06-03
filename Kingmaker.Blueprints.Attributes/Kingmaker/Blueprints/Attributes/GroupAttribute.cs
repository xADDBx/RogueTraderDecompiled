using System;

namespace Kingmaker.Blueprints.Attributes;

public class GroupAttribute : Attribute
{
	public readonly string Name;

	public GroupAttribute(string name)
	{
		Name = name;
	}
}

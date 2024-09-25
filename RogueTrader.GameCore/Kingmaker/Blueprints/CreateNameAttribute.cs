using System;

namespace Kingmaker.Blueprints;

public class CreateNameAttribute : Attribute
{
	public string Name;

	public CreateNameAttribute(string name)
	{
		Name = name;
	}
}

using System;

namespace Kingmaker.Blueprints.JsonSystem.PropertyUtility;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class BlueprintContextMenuAttribute : Attribute
{
	public Type BlueprintType { get; set; }

	public string Name { get; set; }

	public BlueprintContextMenuAttribute(string name)
	{
		Name = name;
	}
}

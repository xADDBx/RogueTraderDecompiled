using System;

namespace Kingmaker.Utility.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class BlueprintButtonAttribute : Attribute
{
	public string Name;

	public int Order;

	public string ValidationMethod;
}

using System;

namespace Kingmaker.Blueprints.JsonSystem.Helpers;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TypeIdAttribute : Attribute
{
	public readonly string GuidString;

	public readonly Guid Guid;

	public TypeIdAttribute(string s)
	{
		GuidString = s;
		Guid = Guid.Parse(s);
	}
}

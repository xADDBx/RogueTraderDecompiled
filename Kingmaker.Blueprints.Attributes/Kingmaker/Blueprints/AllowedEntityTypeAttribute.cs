using System;

namespace Kingmaker.Blueprints;

public class AllowedEntityTypeAttribute : Attribute
{
	public Type Type { get; private set; }

	public AllowedEntityTypeAttribute(Type type)
	{
		Type = type;
	}
}

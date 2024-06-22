using System;

namespace Kingmaker.Blueprints.JsonSystem.Helpers;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class KnowledgeDatabaseIDAttribute : Attribute, IIdAttribute
{
	public string GuidString { get; private set; }

	public Guid Guid { get; private set; }

	public KnowledgeDatabaseIDAttribute(string s)
	{
		GuidString = s;
		Guid = Guid.Parse(s);
	}
}

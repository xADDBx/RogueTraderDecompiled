using System;

namespace UnityModManagerNet;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class CopyAttribute : Attribute
{
	public string Alias;

	public CopyAttribute()
	{
	}

	public CopyAttribute(string Alias)
	{
		this.Alias = Alias;
	}
}

using System;

namespace StateHasher.Core;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class HasherForAttribute : Attribute
{
	public Type Type { get; set; }
}

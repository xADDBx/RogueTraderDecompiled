using System;

namespace StateHasher.Core;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class HasherCustomAttribute : Attribute
{
	public Type Type { get; set; }
}

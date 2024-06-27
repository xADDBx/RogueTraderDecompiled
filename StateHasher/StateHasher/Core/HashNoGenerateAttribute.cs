using System;

namespace StateHasher.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class HashNoGenerateAttribute : Attribute
{
}

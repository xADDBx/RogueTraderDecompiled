using System;

namespace Kingmaker.ResourceLinks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
public class RequireSeparateBundleAttribute : Attribute
{
}

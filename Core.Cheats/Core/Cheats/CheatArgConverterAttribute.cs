using System;
using JetBrains.Annotations;

namespace Core.Cheats;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class CheatArgConverterAttribute : Attribute
{
	public int Order { get; set; }
}

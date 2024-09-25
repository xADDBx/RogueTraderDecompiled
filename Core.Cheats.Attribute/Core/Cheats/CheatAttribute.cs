using System;
using JetBrains.Annotations;

namespace Core.Cheats;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
[MeansImplicitUse]
public sealed class CheatAttribute : Attribute
{
	public string Name { get; set; }

	public string Description { get; set; }

	public string ExampleArgs { get; set; }

	public ExecutionPolicy ExecutionPolicy { get; set; } = ExecutionPolicy.All;

}

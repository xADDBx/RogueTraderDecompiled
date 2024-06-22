using System;
using JetBrains.Annotations;

namespace Kingmaker.QA.Arbiter.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class MeasurementAttribute : Attribute
{
	public string Name { get; }

	public MeasurementAttribute([NotNull] string name)
	{
		Name = name;
	}
}

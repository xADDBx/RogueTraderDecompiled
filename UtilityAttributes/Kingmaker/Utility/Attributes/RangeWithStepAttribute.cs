using System;
using UnityEngine;

namespace Kingmaker.Utility.Attributes;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class RangeWithStepAttribute : PropertyAttribute
{
	public readonly int min;

	public readonly int max;

	public readonly int step;

	public RangeWithStepAttribute(int min, int max, int step)
	{
		this.min = min;
		this.max = max;
		this.step = step;
	}
}

using System;

namespace Kingmaker.Utility.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalShowAttribute : ConditionalAttribute
{
	public override bool ValueForVisible => true;

	public ConditionalShowAttribute(string conditionSource)
		: base(conditionSource)
	{
	}
}

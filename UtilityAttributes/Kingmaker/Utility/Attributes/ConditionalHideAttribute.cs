using System;

namespace Kingmaker.Utility.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ConditionalHideAttribute : ConditionalAttribute
{
	public override bool ValueForVisible => false;

	public ConditionalHideAttribute(string conditionSource)
		: base(conditionSource)
	{
	}
}

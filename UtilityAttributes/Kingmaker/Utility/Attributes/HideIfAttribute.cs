using System;

namespace Kingmaker.Utility.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class HideIfAttribute : ConditionalAttribute
{
	public override bool ValueForVisible => false;

	public HideIfAttribute(string conditionSource)
		: base(conditionSource)
	{
	}
}

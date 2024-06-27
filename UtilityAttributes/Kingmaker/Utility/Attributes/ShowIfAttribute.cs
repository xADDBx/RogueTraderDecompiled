using System;

namespace Kingmaker.Utility.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ShowIfAttribute : ConditionalAttribute
{
	public override bool ValueForVisible => true;

	public ShowIfAttribute(string conditionSource)
		: base(conditionSource)
	{
	}
}

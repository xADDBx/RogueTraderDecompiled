using UnityEngine;

namespace Kingmaker.Utility.Attributes;

public abstract class ConditionalAttribute : PropertyAttribute
{
	public string ConditionSource { get; set; }

	public virtual bool ValueForVisible { get; } = true;


	protected ConditionalAttribute(string conditionSource)
	{
		ConditionSource = conditionSource;
	}
}

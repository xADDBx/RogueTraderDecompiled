using UnityEngine;

namespace Kingmaker.Utility.Attributes;

public class OnValueChangedAttribute : PropertyAttribute
{
	public readonly string MethodName;

	public OnValueChangedAttribute(string methodName)
	{
		MethodName = methodName;
	}
}

using System;
using UnityEngine;

public class LongAsEnumAttribute : PropertyAttribute
{
	public Type EnumType { get; private set; }

	public LongAsEnumAttribute(Type enumType)
	{
		EnumType = enumType;
	}
}

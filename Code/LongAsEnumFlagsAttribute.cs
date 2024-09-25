using System;
using UnityEngine;

public class LongAsEnumFlagsAttribute : PropertyAttribute
{
	public Type EnumType { get; private set; }

	public LongAsEnumFlagsAttribute(Type enumType)
	{
		EnumType = enumType;
	}
}

using System;
using UnityEngine;

namespace Kingmaker.Utility.EnumArrays;

public class EnumArrayAttribute : PropertyAttribute
{
	public readonly Type ValueType;

	public EnumArrayAttribute(Type valueType)
	{
		ValueType = valueType;
	}
}

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Owlcat.QA.Validation;

public static class ValidationUtility
{
	public static string GetEnumDescription(this Enum value)
	{
		FieldInfo field = value.GetType().GetField(value.ToString());
		if (field != null && field.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false) is DescriptionAttribute[] source && source.Any())
		{
			return source.First().Description;
		}
		return value.ToString();
	}

	public static void FixupPrefab(GameObject prefab, Action<GameObject> action)
	{
	}
}

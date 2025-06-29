using System.Reflection;
using Kingmaker;
using UnityEngine;

namespace UnityModManagerNet;

public static class Utils
{
	public static void CopyFields<T1, T2>(object from, object to, CopyFieldMask defaultMask) where T1 : new() where T2 : new()
	{
		CopyFieldMask copyFieldMask = defaultMask;
		object[] customAttributes = typeof(T1).GetCustomAttributes(typeof(CopyFieldsAttribute), inherit: false);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			CopyFieldsAttribute copyFieldsAttribute = (CopyFieldsAttribute)customAttributes[i];
			copyFieldMask = copyFieldsAttribute.Mask;
		}
		FieldInfo[] fields = typeof(T1).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			CopyAttribute copyAttribute = new CopyAttribute();
			object[] customAttributes2 = fieldInfo.GetCustomAttributes(typeof(CopyAttribute), inherit: false);
			if (customAttributes2.Length != 0)
			{
				object[] array2 = customAttributes2;
				for (int k = 0; k < array2.Length; k++)
				{
					CopyAttribute copyAttribute2 = (CopyAttribute)array2[k];
					copyAttribute = copyAttribute2;
				}
			}
			else if ((copyFieldMask & CopyFieldMask.OnlyCopyAttr) != 0 || ((copyFieldMask & CopyFieldMask.SkipNotSerialized) != 0 && fieldInfo.IsNotSerialized) || (((copyFieldMask & CopyFieldMask.Public) <= CopyFieldMask.Any || !fieldInfo.IsPublic) && ((copyFieldMask & CopyFieldMask.Serialized) <= CopyFieldMask.Any || fieldInfo.GetCustomAttributes(typeof(SerializeField), inherit: false).Length == 0) && ((copyFieldMask & CopyFieldMask.Public) != 0 || (copyFieldMask & CopyFieldMask.Serialized) != 0)))
			{
				continue;
			}
			if (string.IsNullOrEmpty(copyAttribute.Alias))
			{
				copyAttribute.Alias = fieldInfo.Name;
			}
			FieldInfo field = typeof(T2).GetField(copyAttribute.Alias, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				if ((copyFieldMask & CopyFieldMask.Matching) == 0)
				{
					PFLog.UnityModManager.Error("Field '" + typeof(T2).Name + "." + copyAttribute.Alias + "' not found");
				}
			}
			else if (fieldInfo.FieldType != field.FieldType)
			{
				PFLog.UnityModManager.Error("Fields '" + typeof(T1).Name + "." + fieldInfo.Name + "' and '" + typeof(T2).Name + "." + field.Name + "' have different types");
			}
			else
			{
				field.SetValue(to, fieldInfo.GetValue(from));
			}
		}
	}
}

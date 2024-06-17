using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Owlcat.QA;

internal static class TypeExtensions
{
	internal static bool HasAttribute<T>(this Type t) where T : Attribute
	{
		return t.GetCustomAttributes(typeof(T), inherit: true).Length != 0;
	}

	internal static bool HasAttribute<T>(this MemberInfo m) where T : Attribute
	{
		object[] customAttributes = m.GetCustomAttributes(inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (customAttributes[i] is T)
			{
				return true;
			}
		}
		return false;
	}

	internal static IEnumerable<Type> GetSubclasses(this Type type, bool includeAbstract = false)
	{
		if (type.IsArray)
		{
			return type.GetElementType().GetSubclasses(includeAbstract);
		}
		if (type.IsGenericTypeDefinition)
		{
			return type.GetSubclassesGeneric(includeAbstract);
		}
		return from t in type.Assembly.GetTypes()
			where t.IsSubclassOf(type) && (includeAbstract || !t.IsAbstract)
			select t;
	}

	internal static IEnumerable<Type> GetSubclassesGeneric(this Type type, bool includeAbstract = false)
	{
		Type[] types = type.Assembly.GetTypes();
		foreach (Type otherType in types)
		{
			if (otherType.IsInterface || (otherType.IsAbstract && !includeAbstract))
			{
				continue;
			}
			Type type2 = otherType;
			while (type2 != null)
			{
				if (type2.IsGenericType)
				{
					type2 = type2.GetGenericTypeDefinition();
				}
				if (type2.IsGenericTypeDefinition && (type2.IsSubclassOf(type) || type2 == type))
				{
					yield return otherType;
					type2 = null;
				}
				else
				{
					type2 = type2.BaseType;
				}
			}
		}
	}

	internal static IEnumerable<FieldInfo> GetUnitySerializedFields(this Type type)
	{
		IEnumerable<FieldInfo> enumerable = from f in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where f.IsPublic || f.HasAttribute<SerializeField>()
			select f;
		if (type.BaseType != null)
		{
			enumerable = enumerable.Concat(type.BaseType.GetUnitySerializedFields());
		}
		return enumerable;
	}

	internal static FieldInfo GetFieldByPath(this Type type, string path)
	{
		if (path.Contains('.'))
		{
			return type.GetFieldByPath(path.Split('.'));
		}
		return type.GetField(path, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	internal static FieldInfo GetFieldByPath(this Type sourceType, IEnumerable<string> path)
	{
		FieldInfo fieldInfo = null;
		Type type = sourceType;
		foreach (string item in path)
		{
			fieldInfo = type.GetFieldByPath(item);
			if (fieldInfo == null)
			{
				break;
			}
			type = fieldInfo.FieldType;
		}
		return fieldInfo;
	}

	internal static string GetGuidValue(this FieldInfo field, string path)
	{
		if (!File.Exists(path))
		{
			return null;
		}
		string[] array = File.ReadAllLines(path);
		foreach (string text in array)
		{
			if (text.Contains("guid: ") && text.Contains(field.Name + ":"))
			{
				return Regex.Match(text, "[({]?[a-fA-F0-9]{8}[-]?([a-fA-F0-9]{4}[-]?){3}[a-fA-F0-9]{12}[})]?").Value;
			}
		}
		return null;
	}
}

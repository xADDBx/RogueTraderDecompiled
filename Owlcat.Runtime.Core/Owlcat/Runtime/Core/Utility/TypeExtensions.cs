using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Owlcat.Runtime.Core.Utility;

public static class TypeExtensions
{
	public static bool HasAttribute<T>(this Type t) where T : Attribute
	{
		return t.GetCustomAttributes(typeof(T), inherit: true).Length != 0;
	}

	public static T GetAttribute<T>(this Type t) where T : Attribute
	{
		return t.GetCustomAttributes(typeof(T), inherit: true).Cast<T>().FirstOrDefault();
	}

	public static IEnumerable<T> GetAttributes<T>(this Type t) where T : Attribute
	{
		return t.GetCustomAttributes(typeof(T), inherit: true).Cast<T>();
	}

	public static bool Implements<T>(this Type t)
	{
		return t.GetInterfaces().Contains(typeof(T));
	}

	public static bool HasAttribute<T>(this MemberInfo m) where T : Attribute
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

	public static T GetAttribute<T>(this MemberInfo m) where T : Attribute
	{
		object[] customAttributes = m.GetCustomAttributes(inherit: true);
		foreach (object obj in customAttributes)
		{
			if (obj is T)
			{
				return (T)obj;
			}
		}
		return null;
	}

	public static IEnumerable<Type> GetSubclasses<T>(bool includeAbstract = false)
	{
		return typeof(T).GetSubclasses(includeAbstract);
	}

	public static IEnumerable<Type> GetSubclasses(this Type type, bool includeAbstract = false)
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

	public static IEnumerable<Type> GetAllBaseTypes(this Type type, bool includeSelf)
	{
		if (includeSelf)
		{
			yield return type;
		}
		Type currentBaseType = type.BaseType;
		while (currentBaseType != null)
		{
			yield return currentBaseType;
			currentBaseType = currentBaseType.BaseType;
		}
	}

	public static IEnumerable<Type> GetSubclassesGeneric(this Type type, bool includeAbstract = false)
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

	public static IEnumerable<FieldInfo> GetUnitySerializedFields(this Type type)
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

	public static bool IsUnityCollection(this Type type)
	{
		if (!type.IsArray)
		{
			if (type.IsGenericType)
			{
				return type.GetGenericTypeDefinition() == typeof(List<>);
			}
			return false;
		}
		return true;
	}

	public static FieldInfo GetFieldByPath(this Type type, string path)
	{
		if (path.Contains('.'))
		{
			return type.GetFieldByPath(path.Split('.'));
		}
		return type.GetField(path, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static FieldInfo GetFieldByPath(this Type sourceType, IEnumerable<string> path)
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

	public static bool IsList(this Type t)
	{
		if (t.IsGenericType)
		{
			return t.GetGenericTypeDefinition() == typeof(List<>);
		}
		return false;
	}

	public static bool IsListOf<T>(this Type t)
	{
		if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
		{
			return t.GenericTypeArguments[0] == typeof(T);
		}
		return false;
	}

	public static bool IsOrSubclassOf<T>(this Type t)
	{
		if (!(t == typeof(T)))
		{
			return t.IsSubclassOf(typeof(T));
		}
		return true;
	}

	public static string GetGuidValue(this FieldInfo field, string path)
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

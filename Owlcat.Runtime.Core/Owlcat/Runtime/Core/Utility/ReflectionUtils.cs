using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Owlcat.Runtime.Core.Utility;

public static class ReflectionUtils
{
	private static Dictionary<KeyValuePair<object, string>, FieldInfo> s_FieldInfoFromPaths = new Dictionary<KeyValuePair<object, string>, FieldInfo>();

	public static FieldInfo GetFieldInfoFromPath(object source, string path)
	{
		FieldInfo value = null;
		KeyValuePair<object, string> key = new KeyValuePair<object, string>(source, path);
		if (!s_FieldInfoFromPaths.TryGetValue(key, out value))
		{
			string[] array = path.Split('.');
			Type type = source.GetType();
			string[] array2 = array;
			foreach (string name in array2)
			{
				value = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (value == null)
				{
					break;
				}
				type = value.FieldType;
			}
			s_FieldInfoFromPaths.Add(key, value);
		}
		return value;
	}

	public static string GetFieldPath<T, TValue>(Expression<Func<T, TValue>> expr)
	{
		ExpressionType nodeType = expr.Body.NodeType;
		MemberExpression memberExpression = (((uint)(nodeType - 10) > 1u) ? (expr.Body as MemberExpression) : (((expr.Body is UnaryExpression unaryExpression) ? unaryExpression.Operand : null) as MemberExpression));
		List<string> list = new List<string>();
		while (memberExpression != null)
		{
			list.Add(memberExpression.Member.Name);
			memberExpression = memberExpression.Expression as MemberExpression;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			stringBuilder.Append(list[num]);
			if (num > 0)
			{
				stringBuilder.Append('.');
			}
		}
		return stringBuilder.ToString();
	}

	public static object GetFieldValue(object source, string name)
	{
		Type type = source.GetType();
		while (type != null)
		{
			FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return field.GetValue(source);
			}
			type = type.BaseType;
		}
		return null;
	}

	public static object GetFieldValueFromPath(object source, ref Type baseType, string path)
	{
		string[] array = path.Split('.');
		object obj = source;
		string[] array2 = array;
		foreach (string name in array2)
		{
			FieldInfo field = baseType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				baseType = null;
				break;
			}
			baseType = field.FieldType;
			obj = GetFieldValue(obj, name);
		}
		if (!(baseType == null))
		{
			return obj;
		}
		return null;
	}

	public static void SetFieldValueByPath(object source, string path, object value)
	{
		string[] array = path.Replace("Array.data[", "Array_data[").Split('.');
		for (int i = 0; i < array.Length; i++)
		{
			Type type = source.GetType();
			bool flag = i == array.Length - 1;
			string text = array[i];
			if (text.StartsWith("Array_data["))
			{
				int num = int.Parse(Regex.Match(text, "Array_data\\[(\\d+)\\]").Groups[1].Value);
				if (type.IsArray && source is Array array2)
				{
					if (flag)
					{
						array2.SetValue(value, num);
						break;
					}
					source = array2.GetValue(num);
					continue;
				}
				PropertyInfo property = type.GetProperty("Item", new Type[1] { typeof(int) });
				if (property != null)
				{
					if (flag)
					{
						property.SetValue(source, value, new object[1] { num });
						break;
					}
					source = property.GetValue(source, new object[1] { num });
					continue;
				}
			}
			FieldInfo field = type.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				break;
			}
			if (flag)
			{
				field.SetValue(source, value);
				break;
			}
			source = GetFieldValue(source, text);
		}
	}

	public static object GetParentObject(string path, object obj)
	{
		string[] array = path.Split('.');
		if (array.Length == 1)
		{
			return obj;
		}
		obj = obj.GetType().GetField(array[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj);
		return GetParentObject(string.Join(".", array, 1, array.Length - 1), obj);
	}
}

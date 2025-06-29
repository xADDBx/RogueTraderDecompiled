using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace TinyJson;

public static class JSONWriter
{
	public static string ToJson(this object item)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendValue(stringBuilder, item);
		return stringBuilder.ToString();
	}

	private static void AppendValue(StringBuilder stringBuilder, object item)
	{
		if (item == null)
		{
			stringBuilder.Append("null");
			return;
		}
		Type type = item.GetType();
		if (type == typeof(string))
		{
			stringBuilder.Append('"');
			string text = (string)item;
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] < ' ' || text[i] == '"' || text[i] == '\\')
				{
					stringBuilder.Append('\\');
					int num = "\"\\\n\r\t\b\f".IndexOf(text[i]);
					if (num >= 0)
					{
						stringBuilder.Append("\"\\nrtbf"[num]);
					}
					else
					{
						stringBuilder.AppendFormat("u{0:X4}", (uint)text[i]);
					}
				}
				else
				{
					stringBuilder.Append(text[i]);
				}
			}
			stringBuilder.Append('"');
			return;
		}
		if (type == typeof(byte) || type == typeof(int))
		{
			stringBuilder.Append(item.ToString());
			return;
		}
		if (type == typeof(float))
		{
			stringBuilder.Append(((float)item).ToString(CultureInfo.InvariantCulture));
			return;
		}
		if (type == typeof(double))
		{
			stringBuilder.Append(((double)item).ToString(CultureInfo.InvariantCulture));
			return;
		}
		if (type == typeof(bool))
		{
			stringBuilder.Append(((bool)item) ? "true" : "false");
			return;
		}
		if (type.IsEnum)
		{
			stringBuilder.Append('"');
			stringBuilder.Append(item.ToString());
			stringBuilder.Append('"');
			return;
		}
		if (item is IList)
		{
			stringBuilder.Append('[');
			bool flag = true;
			IList list = item as IList;
			for (int j = 0; j < list.Count; j++)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(',');
				}
				AppendValue(stringBuilder, list[j]);
			}
			stringBuilder.Append(']');
			return;
		}
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
		{
			Type type2 = type.GetGenericArguments()[0];
			if (type2 != typeof(string))
			{
				stringBuilder.Append("{}");
				return;
			}
			stringBuilder.Append('{');
			IDictionary dictionary = item as IDictionary;
			bool flag2 = true;
			foreach (object key in dictionary.Keys)
			{
				if (flag2)
				{
					flag2 = false;
				}
				else
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append('"');
				stringBuilder.Append((string)key);
				stringBuilder.Append("\":");
				AppendValue(stringBuilder, dictionary[key]);
			}
			stringBuilder.Append('}');
			return;
		}
		stringBuilder.Append('{');
		bool flag3 = true;
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		for (int k = 0; k < fields.Length; k++)
		{
			object value = fields[k].GetValue(item);
			if (value != null)
			{
				if (flag3)
				{
					flag3 = false;
				}
				else
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append('"');
				stringBuilder.Append(GetMemberName(fields[k]));
				stringBuilder.Append("\":");
				AppendValue(stringBuilder, value);
			}
		}
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		for (int l = 0; l < properties.Length; l++)
		{
			object value2 = properties[l].GetValue(item, null);
			if (value2 != null)
			{
				if (flag3)
				{
					flag3 = false;
				}
				else
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append('"');
				stringBuilder.Append(GetMemberName(properties[l]));
				stringBuilder.Append("\":");
				AppendValue(stringBuilder, value2);
			}
		}
		stringBuilder.Append('}');
	}

	private static string GetMemberName(MemberInfo member)
	{
		return member.Name;
	}
}

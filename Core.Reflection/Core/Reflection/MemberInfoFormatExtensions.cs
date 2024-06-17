using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Reflection;

public static class MemberInfoFormatExtensions
{
	public static string GetSignature(this PropertyInfo property)
	{
		MethodInfo getMethod = property.GetGetMethod(nonPublic: true);
		MethodInfo setMethod = property.GetSetMethod(nonPublic: true);
		StringBuilder stringBuilder = new StringBuilder();
		MethodInfo method = LeastRestrictiveVisibility(getMethod, setMethod);
		BuildReturnSignature(stringBuilder, property, method);
		stringBuilder.Append(" { ");
		if (getMethod != null)
		{
			if (Visibility(method) != Visibility(getMethod))
			{
				stringBuilder.Append(Visibility(getMethod) + " ");
			}
			stringBuilder.Append("get; ");
		}
		if (setMethod != null)
		{
			if (Visibility(method) != Visibility(setMethod))
			{
				stringBuilder.Append(Visibility(setMethod) + " ");
			}
			stringBuilder.Append("set; ");
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	public static string GetSignature(this MethodInfo method, bool callable = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		BuildReturnSignature(stringBuilder, null, method, fullNamespace: true, callable);
		stringBuilder.Append("(");
		bool flag = true;
		bool flag2 = false;
		ParameterInfo[] parameters = method.GetParameters();
		foreach (ParameterInfo parameterInfo in parameters)
		{
			if (flag)
			{
				flag = false;
				if (method.IsDefined(typeof(ExtensionAttribute), inherit: false))
				{
					if (callable)
					{
						flag2 = true;
						continue;
					}
					stringBuilder.Append("this ");
				}
			}
			else if (flag2)
			{
				flag2 = false;
			}
			else
			{
				stringBuilder.Append(", ");
			}
			if (parameterInfo.IsOut)
			{
				stringBuilder.Append("out ");
			}
			else if (parameterInfo.ParameterType.IsByRef)
			{
				stringBuilder.Append("ref ");
			}
			if (IsParamArray(parameterInfo))
			{
				stringBuilder.Append("params ");
			}
			if (!callable)
			{
				stringBuilder.Append(parameterInfo.ParameterType.TypeName());
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(parameterInfo.Name);
			if (parameterInfo.IsOptional)
			{
				stringBuilder.Append(" = " + (parameterInfo.DefaultValue ?? "null"));
			}
		}
		stringBuilder.Append(")");
		Type[] genericArguments = method.GetGenericArguments();
		foreach (Type type in genericArguments)
		{
			List<string> list = new List<string>();
			Type[] genericParameterConstraints = type.GetGenericParameterConstraints();
			foreach (Type type2 in genericParameterConstraints)
			{
				list.Add(type2.TypeName());
			}
			GenericParameterAttributes genericParameterAttributes = type.GenericParameterAttributes;
			if (genericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
			{
				list.Add("class");
			}
			if (genericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
			{
				list.Add("struct");
			}
			if (genericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
			{
				list.Add("new()");
			}
			if (list.Count > 0)
			{
				stringBuilder.Append(" where " + type.TypeName() + ": " + string.Join(", ", list));
			}
		}
		return stringBuilder.ToString();
	}

	public static string TypeName(this Type type)
	{
		Type underlyingType = Nullable.GetUnderlyingType(type);
		if (underlyingType != null)
		{
			return underlyingType.TypeName() + "?";
		}
		if (!type.IsGenericType)
		{
			if (type.IsArray)
			{
				return type.GetElementType().TypeName() + "[]";
			}
			switch (type.Name)
			{
			case "String":
				return "string";
			case "Int16":
				return "short";
			case "UInt16":
				return "ushort";
			case "Int32":
				return "int";
			case "UInt32":
				return "uint";
			case "Int64":
				return "long";
			case "UInt64":
				return "ulong";
			case "Decimal":
				return "decimal";
			case "Double":
				return "double";
			case "Object":
				return "object";
			case "Void":
				return "void";
			case "Boolean":
				return "bool";
			default:
				if (!string.IsNullOrWhiteSpace(type.FullName))
				{
					return type.FullName;
				}
				return type.Name;
			}
		}
		StringBuilder stringBuilder = new StringBuilder(type.Name.Substring(0, type.Name.IndexOf('`')));
		stringBuilder.Append('<');
		bool flag = true;
		Type[] genericArguments = type.GetGenericArguments();
		foreach (Type type2 in genericArguments)
		{
			if (!flag)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(type2.TypeName());
			flag = false;
		}
		stringBuilder.Append('>');
		return stringBuilder.ToString();
	}

	private static void BuildReturnSignature(StringBuilder sigBuilder, PropertyInfo prop, MethodInfo method, bool fullNamespace = true, bool callable = false)
	{
		bool flag = true;
		if (!callable)
		{
			sigBuilder.Append(Visibility(method) + " ");
			if (method.IsStatic)
			{
				sigBuilder.Append("static ");
			}
			sigBuilder.Append((prop?.PropertyType ?? method.ReturnType).TypeName());
			sigBuilder.Append(' ');
		}
		if (fullNamespace)
		{
			sigBuilder.Append(method.DeclaringType.TypeName()).Append(".");
		}
		sigBuilder.Append(prop?.Name ?? method.Name);
		if (!method.IsGenericMethod)
		{
			return;
		}
		sigBuilder.Append("<");
		Type[] genericArguments = method.GetGenericArguments();
		foreach (Type type in genericArguments)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				sigBuilder.Append(", ");
			}
			sigBuilder.Append(type.TypeName());
		}
		sigBuilder.Append(">");
	}

	private static string Visibility(MethodInfo method)
	{
		if (method.IsPublic)
		{
			return "public";
		}
		if (method.IsPrivate)
		{
			return "private";
		}
		if (method.IsAssembly)
		{
			return "internal";
		}
		if (method.IsFamily)
		{
			return "protected";
		}
		throw new Exception("I wasn't able to parse the visibility of this method.");
	}

	private static MethodInfo LeastRestrictiveVisibility(MethodInfo member1, MethodInfo member2)
	{
		if (member1 != null && member2 == null)
		{
			return member1;
		}
		if (member2 != null && member1 == null)
		{
			return member2;
		}
		int num = VisibilityValue(member1);
		int num2 = VisibilityValue(member2);
		if (num < num2)
		{
			return member1;
		}
		return member2;
	}

	private static int VisibilityValue(MethodInfo method)
	{
		if (method.IsPublic)
		{
			return 1;
		}
		if (method.IsFamily)
		{
			return 2;
		}
		if (method.IsAssembly)
		{
			return 3;
		}
		if (method.IsPrivate)
		{
			return 4;
		}
		throw new Exception("I wasn't able to parse the visibility of this method.");
	}

	private static bool IsParamArray(ParameterInfo info)
	{
		return info.GetCustomAttribute(typeof(ParamArrayAttribute), inherit: true) != null;
	}
}

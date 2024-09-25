using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Reflection;

public static class ReflectionExtension
{
	public static Assembly[] GetAssembliesSafe(this AppDomain appDomain)
	{
		try
		{
			return appDomain.GetAssemblies();
		}
		catch (Exception)
		{
		}
		return Array.Empty<Assembly>();
	}

	public static Assembly[] GetAssembliesSafeNonStandard(this AppDomain appDomain)
	{
		try
		{
			return (from x in appDomain.GetAssemblies()
				where !x.IsDynamic && !x.FullName.ToLower().StartsWith("mscorlib") && !x.FullName.ToLower().StartsWith("system") && !x.FullName.ToLower().StartsWith("nunit") && !x.FullName.ToLower().StartsWith("unityengine") && !x.FullName.ToLower().StartsWith("unityeditor")
				select x).ToArray();
		}
		catch (Exception)
		{
		}
		return Array.Empty<Assembly>();
	}

	public static Type GetTypeSafe(this Assembly assembly, string name)
	{
		try
		{
			return assembly.GetType(name);
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static Type[] GetTypesSafe(this Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (Exception)
		{
		}
		return Array.Empty<Type>();
	}

	public static MethodInfo[] GetMethodsSafe(this Type type, BindingFlags flags)
	{
		try
		{
			return type.GetMethods(flags);
		}
		catch (Exception)
		{
		}
		return Array.Empty<MethodInfo>();
	}

	public static PropertyInfo[] GetPropertiesSafe(this Type type, BindingFlags flags)
	{
		try
		{
			return type.GetProperties(flags);
		}
		catch (Exception)
		{
		}
		return Array.Empty<PropertyInfo>();
	}

	public static MemberInfo[] GetMembersSafe(this Type type, BindingFlags flags)
	{
		try
		{
			return type.GetMembers(flags);
		}
		catch (Exception)
		{
		}
		return Array.Empty<MemberInfo>();
	}

	public static IEnumerable<T> GetCustomAttributesSafe<T>(this MemberInfo element, bool inherit) where T : Attribute
	{
		try
		{
			return element.GetCustomAttributes<T>(inherit);
		}
		catch (Exception)
		{
		}
		return Array.Empty<T>();
	}

	public static IEnumerable<T> GetTypeInNonStandardAssemblies<T>(this AppDomain domain, string typeName) where T : Type
	{
		Assembly[] assembliesSafeNonStandard = domain.GetAssembliesSafeNonStandard();
		for (int i = 0; i < assembliesSafeNonStandard.Length; i++)
		{
			Type typeSafe = assembliesSafeNonStandard[i].GetTypeSafe(typeName);
			if (typeSafe != null)
			{
				yield return typeSafe as T;
			}
		}
	}

	public static IEnumerable<TFieldValue> GetTypeFieldInNonStandardAssemblies<TFieldValue>(this AppDomain domain, string typeName, string fieldName)
	{
		foreach (Type typeInNonStandardAssembly in domain.GetTypeInNonStandardAssemblies<Type>(typeName))
		{
			FieldInfo field = typeInNonStandardAssembly.GetField(fieldName);
			if (field == null)
			{
				throw new Exception($"Type {typeInNonStandardAssembly} in assembly {typeInNonStandardAssembly.Assembly.FullName} does not contain field {fieldName}");
			}
			yield return (TFieldValue)field.GetValue(null);
		}
	}
}

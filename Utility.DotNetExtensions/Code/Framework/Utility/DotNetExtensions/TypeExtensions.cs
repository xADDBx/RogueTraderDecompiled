using System;

namespace Code.Framework.Utility.DotNetExtensions;

public static class TypeExtensions
{
	public static bool IsArrayOfType(this Type type, Type elementType)
	{
		if (!type.IsArray)
		{
			return false;
		}
		Type elementType2 = type.GetElementType();
		if (elementType2 == elementType)
		{
			return true;
		}
		if ((object)elementType2 != null && elementType2.IsGenericType)
		{
			return elementType2.GetGenericTypeDefinition() == elementType;
		}
		return false;
	}
}

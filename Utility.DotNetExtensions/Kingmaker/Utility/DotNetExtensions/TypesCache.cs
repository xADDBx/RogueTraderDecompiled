using System;

namespace Kingmaker.Utility.DotNetExtensions;

public static class TypesCache
{
	public static string GetTypeName(Type type)
	{
		return type.FullName;
	}
}

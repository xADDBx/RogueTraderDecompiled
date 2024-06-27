using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public static class Utils
{
	public static void Swap<T>(ref T lhs, ref T rhs)
	{
		T val = lhs;
		lhs = rhs;
		rhs = val;
	}

	public static void EditorSafeDestroy(Object obj)
	{
		Object.Destroy(obj);
	}

	public static bool IsNullOrEmpty(this string _this)
	{
		return string.IsNullOrEmpty(_this);
	}

	public static string EmptyToNull(this string @this)
	{
		if (!(@this != ""))
		{
			return null;
		}
		return @this;
	}
}

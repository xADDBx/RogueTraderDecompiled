using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace StateHasher.Core;

[Preserve]
public static class RecursiveReferences
{
	private class EqualityComparer : IEqualityComparer<object>
	{
		bool IEqualityComparer<object>.Equals(object x, object y)
		{
			return x == y;
		}

		int IEqualityComparer<object>.GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private static readonly Dictionary<object, int> __refs = new Dictionary<object, int>(new EqualityComparer());

	private static int __refIndex = 0;

	public static void Reset()
	{
		__refs.Clear();
		__refIndex = 0;
	}

	public static bool TryGetValue(object obj, out int index)
	{
		return __refs.TryGetValue(obj, out index);
	}

	public static void Add(object obj)
	{
		__refs.Add(obj, __refIndex++);
	}
}

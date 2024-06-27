using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace StateHasher.Core.Hashers;

public static class EnumHasher<T> where T : struct, Enum
{
	public static Hash128 GetHash128(ref T obj)
	{
		Hash128 result = default(Hash128);
		int val = UnsafeUtility.EnumToInt(obj);
		result.Append(val);
		return result;
	}
}

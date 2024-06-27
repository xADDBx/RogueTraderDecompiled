using UnityEngine;

namespace StateHasher.Core.Hashers;

public static class UnmanagedHasher<T> where T : unmanaged
{
	public static Hash128 GetHash128(ref T obj)
	{
		Hash128 result = default(Hash128);
		result.Append(ref obj);
		return result;
	}
}

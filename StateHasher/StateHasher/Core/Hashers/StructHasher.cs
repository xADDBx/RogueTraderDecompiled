using Unity.Profiling;
using UnityEngine;

namespace StateHasher.Core.Hashers;

[IgnoredByDeepProfiler]
public static class StructHasher<T> where T : struct, IHashable
{
	public static Hash128 GetHash128(ref T obj)
	{
		return obj.GetHash128();
	}
}

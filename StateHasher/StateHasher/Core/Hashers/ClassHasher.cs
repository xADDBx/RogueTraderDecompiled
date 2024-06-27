using Unity.Profiling;
using UnityEngine;

namespace StateHasher.Core.Hashers;

[IgnoredByDeepProfiler]
public static class ClassHasher<T> where T : class, IHashable
{
	public static Hash128 GetHash128(T obj)
	{
		if (obj == null)
		{
			return default(Hash128);
		}
		if (RecursiveReferences.TryGetValue(obj, out var index))
		{
			Hash128 result = default(Hash128);
			result.Append(ref index);
			return result;
		}
		RecursiveReferences.Add(obj);
		return obj.GetHash128();
	}
}

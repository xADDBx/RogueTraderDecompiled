using UnityEngine;

namespace StateHasher.Core.Hashers;

public static class DummyHasher<T>
{
	public static Hash128 GetHash128(ref T obj)
	{
		return default(Hash128);
	}
}

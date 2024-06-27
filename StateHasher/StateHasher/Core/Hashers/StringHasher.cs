using UnityEngine;

namespace StateHasher.Core.Hashers;

public static class StringHasher
{
	public static Hash128 GetHash128(string obj)
	{
		return Hash128.Compute(obj);
	}
}

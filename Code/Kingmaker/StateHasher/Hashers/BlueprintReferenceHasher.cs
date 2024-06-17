using Kingmaker.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.StateHasher.Hashers;

internal static class BlueprintReferenceHasher
{
	[HasherFor(Type = typeof(BlueprintReferenceBase))]
	public static Hash128 GetHash128(BlueprintReferenceBase obj)
	{
		Hash128 result = default(Hash128);
		if (obj == null)
		{
			return result;
		}
		result.Append(obj.Guid);
		return result;
	}
}

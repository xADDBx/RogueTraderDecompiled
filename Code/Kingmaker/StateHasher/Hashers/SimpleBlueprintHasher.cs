using Kingmaker.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.StateHasher.Hashers;

internal static class SimpleBlueprintHasher
{
	[HasherFor(Type = typeof(SimpleBlueprint))]
	public static Hash128 GetHash128(SimpleBlueprint obj)
	{
		Hash128 result = default(Hash128);
		if (obj == null)
		{
			return result;
		}
		result.Append(obj.AssetGuid);
		return result;
	}
}

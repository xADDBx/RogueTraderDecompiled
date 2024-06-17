using Kingmaker.View.Roaming;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.StateHasher.Hashers;

public static class IRoamingPointHasher
{
	[HasherFor(Type = typeof(IRoamingPoint))]
	public static Hash128 GetHash128(IRoamingPoint obj)
	{
		return (obj as IHashable)?.GetHash128() ?? default(Hash128);
	}
}

using Kingmaker.ElementsSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.StateHasher.Hashers;

public static class MechanicEntityEvaluatorHasher
{
	[HasherFor(Type = typeof(MechanicEntityEvaluator))]
	public static Hash128 GetHash128(MechanicEntityEvaluator obj)
	{
		return default(Hash128);
	}
}

using Kingmaker.EntitySystem.Entities;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.StateHasher.Hashers;

public static class UnitReferenceHasher
{
	[HasherFor(Type = typeof(UnitReference))]
	public static Hash128 GetHash128(ref UnitReference obj)
	{
		return StringHasher.GetHash128(obj.Id);
	}
}

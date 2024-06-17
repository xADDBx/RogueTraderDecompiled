using Kingmaker.EntitySystem.Entities.Base;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.StateHasher.Hashers;

public static class EntityRefHasher
{
	[HasherFor(Type = typeof(EntityRef))]
	public static Hash128 GetHash128(ref EntityRef obj)
	{
		return StringHasher.GetHash128(obj.Id);
	}
}
public static class EntityRefHasher<T> where T : Entity
{
	[HasherFor(Type = typeof(EntityRef<>))]
	public static Hash128 GetHash128(ref EntityRef<T> obj)
	{
		return StringHasher.GetHash128(obj.Id);
	}
}

using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class SpringAttackEntry : IHashable
{
	[JsonProperty]
	public Vector3 OldPosition;

	[JsonProperty]
	public Vector3 NewPosition;

	[JsonProperty]
	public int Index;

	[JsonProperty]
	public EntityRef<AreaEffectEntity> AreaMark;

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref OldPosition);
		result.Append(ref NewPosition);
		result.Append(ref Index);
		EntityRef<AreaEffectEntity> obj = AreaMark;
		Hash128 val = StructHasher<EntityRef<AreaEffectEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		return result;
	}
}

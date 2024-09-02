using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class OathOfVengeanceEntry : IHashable
{
	[JsonProperty]
	public EntityRef<UnitEntity> Ally { get; set; }

	[JsonProperty]
	public EntityRef<UnitEntity> Enemy { get; set; }

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<UnitEntity> obj = Ally;
		Hash128 val = StructHasher<EntityRef<UnitEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		EntityRef<UnitEntity> obj2 = Enemy;
		Hash128 val2 = StructHasher<EntityRef<UnitEntity>>.GetHash128(ref obj2);
		result.Append(ref val2);
		return result;
	}
}

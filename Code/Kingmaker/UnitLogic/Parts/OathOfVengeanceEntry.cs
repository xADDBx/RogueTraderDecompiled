using Kingmaker.EntitySystem.Entities;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class OathOfVengeanceEntry : IHashable
{
	[JsonProperty]
	public UnitEntity Ally { get; set; }

	[JsonProperty]
	public UnitEntity Enemy { get; set; }

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<UnitEntity>.GetHash128(Ally);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<UnitEntity>.GetHash128(Enemy);
		result.Append(ref val2);
		return result;
	}
}

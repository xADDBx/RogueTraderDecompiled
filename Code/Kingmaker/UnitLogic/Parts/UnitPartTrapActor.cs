using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.MapObjects.Traps;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartTrapActor : BaseUnitPart, IHashable
{
	[JsonProperty]
	private EntityRef<TrapObjectData> m_TrapRef;

	public TrapObjectData Trap => m_TrapRef;

	public void Setup(TrapObjectData trap)
	{
		m_TrapRef = trap;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<TrapObjectData> obj = m_TrapRef;
		Hash128 val2 = StructHasher<EntityRef<TrapObjectData>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}
}

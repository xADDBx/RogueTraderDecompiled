using System;
using Kingmaker.AI.Learning.Collections;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AI.Learning;

[Serializable]
public class CollectedUnitData : IHashable
{
	[JsonProperty]
	private readonly EntityRef<BaseUnitEntity> m_UnitRef;

	[JsonProperty]
	public AttackDataCollection AttackDataCollection = new AttackDataCollection();

	public BaseUnitEntity Unit => m_UnitRef.Entity;

	public CollectedUnitData(BaseUnitEntity unit)
	{
		m_UnitRef = unit;
	}

	[JsonConstructor]
	private CollectedUnitData()
	{
	}

	public void Clear()
	{
		AttackDataCollection.Clear();
	}

	public override string ToString()
	{
		return "CollectedData: " + Unit.Blueprint.name;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<BaseUnitEntity> obj = m_UnitRef;
		Hash128 val = StructHasher<EntityRef<BaseUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		Hash128 val2 = AttackDataCollection.Hasher.GetHash128(AttackDataCollection);
		result.Append(ref val2);
		return result;
	}
}

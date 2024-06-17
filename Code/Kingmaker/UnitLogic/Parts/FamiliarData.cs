using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class FamiliarData : IHashable
{
	[JsonProperty]
	private EntityRef<AbstractUnitEntity> m_Unit;

	[JsonProperty]
	public EntityFactSource Source { get; private set; }

	[CanBeNull]
	public AbstractUnitEntity Unit
	{
		get
		{
			return m_Unit;
		}
		private set
		{
			m_Unit = value;
		}
	}

	public FamiliarData(AbstractUnitEntity unit, EntityFactSource source)
	{
		Unit = unit;
		Source = source;
	}

	[JsonConstructor]
	private FamiliarData()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<AbstractUnitEntity> obj = m_Unit;
		Hash128 val = StructHasher<EntityRef<AbstractUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<EntityFactSource>.GetHash128(Source);
		result.Append(ref val2);
		return result;
	}
}

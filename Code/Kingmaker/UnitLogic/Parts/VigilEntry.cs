using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.Pathfinding;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class VigilEntry : IHashable
{
	[JsonProperty]
	private Vector3 m_OldPosition;

	[JsonProperty]
	private bool m_IsOldPositionSet;

	[JsonProperty]
	private EntityFactRef<Buff> m_BuffRef;

	[JsonProperty(PropertyName = "Buff")]
	private Buff m_Buff;

	[JsonProperty]
	public int OldDamage { get; set; }

	[JsonProperty]
	public BlueprintAbility TeleportAbility { get; set; }

	public Buff Buff
	{
		get
		{
			return m_BuffRef.Fact;
		}
		set
		{
			m_BuffRef = value;
		}
	}

	public CustomGridNodeBase OldPosition
	{
		[CanBeNull]
		get
		{
			if (!m_IsOldPositionSet)
			{
				return null;
			}
			return m_OldPosition.GetNearestNodeXZUnwalkable();
		}
		set
		{
			m_OldPosition = value.Vector3Position;
			m_IsOldPositionSet = true;
		}
	}

	public void OnPostLoad()
	{
		if (m_Buff != null)
		{
			m_BuffRef = new EntityFactRef<Buff>(m_Buff);
			m_Buff = null;
			PFLog.Default.Log($"Convert Buff property to ref. Buff={Buff}, buff owner={m_BuffRef.Entity}");
		}
		if (!m_IsOldPositionSet && m_BuffRef.Entity != null)
		{
			m_OldPosition = m_BuffRef.Entity.Position;
			m_IsOldPositionSet = true;
			PFLog.Default.Log($"Convert OldPosition property to Vector3. Defaulting to buff owner position {m_OldPosition}. Buff owner={m_BuffRef.Entity}");
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_OldPosition);
		result.Append(ref m_IsOldPositionSet);
		EntityFactRef<Buff> obj = m_BuffRef;
		Hash128 val = StructHasher<EntityFactRef<Buff>>.GetHash128(ref obj);
		result.Append(ref val);
		int val2 = OldDamage;
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(TeleportAbility);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<Buff>.GetHash128(m_Buff);
		result.Append(ref val4);
		return result;
	}
}

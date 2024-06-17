using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

public class ShadowDisbelieveData : IHashable
{
	[Flags]
	public enum State : byte
	{
		Disbelieved = 1,
		ChanceChecked = 2,
		AffectedByChance = 4
	}

	[JsonProperty]
	private State m_State;

	[JsonProperty]
	public EntityRef<MechanicEntity> Unit { get; set; }

	public bool IsDisbelieved
	{
		get
		{
			return (m_State & State.Disbelieved) != 0;
		}
		set
		{
			m_State = (value ? (m_State | State.Disbelieved) : (m_State & ~State.Disbelieved));
		}
	}

	public bool IsChanceChecked
	{
		get
		{
			return (m_State & State.ChanceChecked) != 0;
		}
		set
		{
			m_State = (value ? (m_State | State.ChanceChecked) : (m_State & ~State.ChanceChecked));
		}
	}

	public bool IsAffectedByChance
	{
		get
		{
			return (m_State & State.AffectedByChance) != 0;
		}
		set
		{
			m_State = (value ? (m_State | State.AffectedByChance) : (m_State & ~State.AffectedByChance));
		}
	}

	public ShadowDisbelieveData Clone()
	{
		return new ShadowDisbelieveData
		{
			Unit = Unit,
			m_State = m_State
		};
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<MechanicEntity> obj = Unit;
		Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		result.Append(ref m_State);
		return result;
	}
}

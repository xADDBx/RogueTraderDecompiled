using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class PartUnitStealth : BaseUnitPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitStealth>, IEntityPartOwner
	{
		PartUnitStealth Stealth { get; }
	}

	[JsonProperty]
	private readonly List<UnitReference> m_SpottedBy = new List<UnitReference>();

	[JsonProperty]
	public bool Active { get; set; }

	[JsonProperty]
	public bool WantActivate { get; set; }

	[JsonProperty]
	public int CachedRoll { get; set; }

	[JsonProperty]
	public bool FullSpeed { get; set; }

	[JsonProperty]
	public bool ShouldExitStealth { get; set; }

	[JsonProperty]
	public TimeSpan LastNearEnemyTime { get; set; }

	[JsonProperty]
	public float NearEnemyPenalty { get; set; }

	[JsonProperty]
	public bool ForceEnterStealth { get; set; }

	[JsonProperty]
	public bool BecameInvisibleThisFrame { get; set; }

	[JsonProperty]
	public bool InAmbush { get; set; }

	[JsonProperty]
	public float AmbushJoinCombatDistance { get; set; }

	[JsonProperty]
	public bool AmbushTake20 { get; set; }

	public IEnumerable<BaseUnitEntity> SpottedBy
	{
		get
		{
			int i = 0;
			while (i < m_SpottedBy.Count)
			{
				UnitReference unitReference = m_SpottedBy[i];
				if (unitReference.Entity != null && unitReference.Entity.ToBaseUnitEntity().LifeState.IsConscious)
				{
					yield return unitReference.Entity.ToBaseUnitEntity();
				}
				int num = i + 1;
				i = num;
			}
		}
	}

	public bool AddSpottedBy(BaseUnitEntity unit)
	{
		if (!m_SpottedBy.Contains(unit.FromBaseUnitEntity()))
		{
			m_SpottedBy.Add(unit.FromBaseUnitEntity());
			return true;
		}
		return false;
	}

	public bool IsSpottedBy(UnitGroup group)
	{
		for (int i = 0; i < m_SpottedBy.Count; i++)
		{
			IAbstractUnitEntity entity = m_SpottedBy[i].Entity;
			if (entity != null && entity.ToBaseUnitEntity().LifeState.IsConscious && entity.ToBaseUnitEntity().CombatGroup.Group == group)
			{
				return true;
			}
		}
		return false;
	}

	public void Clear()
	{
		m_SpottedBy.Clear();
		CachedRoll = 0;
		FullSpeed = false;
		ShouldExitStealth = false;
	}

	public void ForceExitStealth()
	{
		if (Active)
		{
			WantActivate = false;
			Clear();
			Active = false;
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitStealthHandler>)delegate(IUnitStealthHandler h)
			{
				h.HandleUnitSwitchStealthCondition(inStealth: false);
			}, isCheckRuntime: true);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = Active;
		result.Append(ref val2);
		bool val3 = WantActivate;
		result.Append(ref val3);
		int val4 = CachedRoll;
		result.Append(ref val4);
		bool val5 = FullSpeed;
		result.Append(ref val5);
		bool val6 = ShouldExitStealth;
		result.Append(ref val6);
		TimeSpan val7 = LastNearEnemyTime;
		result.Append(ref val7);
		float val8 = NearEnemyPenalty;
		result.Append(ref val8);
		bool val9 = ForceEnterStealth;
		result.Append(ref val9);
		bool val10 = BecameInvisibleThisFrame;
		result.Append(ref val10);
		bool val11 = InAmbush;
		result.Append(ref val11);
		float val12 = AmbushJoinCombatDistance;
		result.Append(ref val12);
		bool val13 = AmbushTake20;
		result.Append(ref val13);
		List<UnitReference> spottedBy = m_SpottedBy;
		if (spottedBy != null)
		{
			for (int i = 0; i < spottedBy.Count; i++)
			{
				UnitReference obj = spottedBy[i];
				Hash128 val14 = UnitReferenceHasher.GetHash128(ref obj);
				result.Append(ref val14);
			}
		}
		return result;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Groups;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[JsonObject]
public class UnitAttackFactions : IEnumerable<BlueprintFaction>, IEnumerable, IHashable
{
	private BaseUnitEntity m_Owner;

	[JsonProperty]
	private readonly HashSet<BlueprintFaction> m_Factions = new HashSet<BlueprintFaction>();

	public bool IsPlayerEnemy { get; private set; }

	[JsonConstructor]
	public UnitAttackFactions(BaseUnitEntity owner)
	{
		m_Owner = owner;
	}

	public HashSet<BlueprintFaction>.Enumerator GetEnumerator()
	{
		return m_Factions.GetEnumerator();
	}

	IEnumerator<BlueprintFaction> IEnumerable<BlueprintFaction>.GetEnumerator()
	{
		return m_Factions.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_Factions.GetEnumerator();
	}

	public void Add(BlueprintFaction faction)
	{
		m_Factions.Add(faction);
		UpdateIsPlayerEnemy();
	}

	public void Remove(BlueprintFaction faction)
	{
		m_Factions.Remove(faction);
		UpdateIsPlayerEnemy();
	}

	public void UnionWith(IEnumerable<BlueprintFaction> faction)
	{
		m_Factions.UnionWith(faction);
		UpdateIsPlayerEnemy();
	}

	public void Match(IEnumerable<BlueprintFaction> faction)
	{
		m_Factions.Clear();
		UnionWith(faction);
	}

	public bool Contains(BlueprintFaction faction)
	{
		return m_Factions.Contains(faction);
	}

	public void Clear()
	{
		m_Factions.Clear();
		UpdateIsPlayerEnemy();
	}

	private void UpdateIsPlayerEnemy()
	{
		IsPlayerEnemy = m_Owner.Faction.EnemyForEveryone || m_Factions.Contains(BlueprintRoot.Instance.PlayerFaction);
		if (m_Owner.GetOptional<PartCombatGroup>()?.Owner != null)
		{
			m_Owner.GetOptional<PartCombatGroup>()?.UpdateAttackFactionsCache();
		}
		if (m_Owner.IsInitialized)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)m_Owner, (Action<IUnitChangeAttackFactionsHandler>)delegate(IUnitChangeAttackFactionsHandler h)
			{
				h.HandleUnitChangeAttackFactions(m_Owner);
			}, isCheckRuntime: true);
		}
	}

	public void PrePostLoad(BaseUnitEntity owner)
	{
		m_Owner = owner;
		UpdateIsPlayerEnemy();
	}

	public void PostLoad()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		HashSet<BlueprintFaction> factions = m_Factions;
		if (factions != null)
		{
			int num = 0;
			foreach (BlueprintFaction item in factions)
			{
				num ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		return result;
	}
}

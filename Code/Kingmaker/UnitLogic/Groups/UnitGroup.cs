using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Groups;

public sealed class UnitGroup : ICombatGroup, IDisposable, IComparable<UnitGroup>, IHashable
{
	private readonly List<UnitReference> m_Units = new List<UnitReference>();

	private readonly MultiSet<BlueprintFaction> m_Factions = new MultiSet<BlueprintFaction>();

	private readonly List<BlueprintFaction> m_AttackFactions = new List<BlueprintFaction>();

	private bool m_IsEnemyForEveryone;

	[JsonProperty]
	public string Id { get; private set; }

	[JsonProperty]
	public UnitGroupMemory Memory { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool IsPlayerParty { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool Disposed { get; private set; }

	[NotNull]
	public CountableFlag IsInCombat { get; } = new CountableFlag();


	public float LeaveCombatTimer { get; set; }

	public bool IsFollowingUnitInCombat { get; set; }

	public ReadonlyList<BlueprintFaction> AttackFactions => m_AttackFactions;

	public bool IsFake => Id == null;

	public int Count => m_Units.Count;

	public bool IsInFogOfWar => All((BaseUnitEntity u) => u.IsInFogOfWar);

	public ReadonlyList<UnitReference> Units => m_Units;

	public bool IsExtra => Id == "<optimized-units>";

	[CanBeNull]
	public BaseUnitEntity this[int index] => m_Units[index].Entity.ToBaseUnitEntity();

	public UnitGroup(string id)
	{
		Id = id;
		Memory = new UnitGroupMemory(id);
		IsPlayerParty = Id != null && Id.Equals("<directly-controllable-unit>");
	}

	[JsonConstructor]
	private UnitGroup()
	{
	}

	public bool Empty()
	{
		return m_Units.Count <= 0;
	}

	public UnitGroupView FindGroupView()
	{
		return Game.Instance.State.LoadedAreaState.AllEntityData.OfType<UnitGroupView.UnitGroupData>().FirstOrDefault((UnitGroupView.UnitGroupData d) => d.UniqueId == Id)?.View as UnitGroupView;
	}

	public void Add(BaseUnitEntity unit)
	{
		if (!IsFake && unit.CombatGroup.Id != Id)
		{
			PFLog.Default.Error($"Adding unit to wrong group: {unit}");
			return;
		}
		if (m_Units.Contains(unit.FromBaseUnitEntity()))
		{
			PFLog.Default.Error($"Group already contains unit: {unit}");
			return;
		}
		if (Disposed)
		{
			PFLog.Default.Error($"Adding unit to disposed group: {unit}");
			return;
		}
		m_Units.Add(unit.FromBaseUnitEntity());
		m_Units.Sort();
		if (unit.IsInCombat)
		{
			IsInCombat.Retain();
		}
		m_Factions.Add(unit.Faction.Blueprint);
		UpdateAttackFactionsCache();
	}

	public void Remove(BaseUnitEntity unit)
	{
		if (!Disposed)
		{
			m_Units.Remove(unit.FromBaseUnitEntity());
			if (unit.IsInCombat)
			{
				IsInCombat.Release();
			}
			m_Factions.Remove(unit.Faction.Blueprint);
			UpdateAttackFactionsCache();
		}
	}

	private static bool IsEnemy(UnitGroup g1, UnitGroup g2)
	{
		using (ProfileScope.New("UnitGroup.IsEnemy"))
		{
			if (g1.IsExtra || g2.IsExtra)
			{
				return false;
			}
			if (g1 == g2)
			{
				return false;
			}
			if (g1.m_IsEnemyForEveryone || g2.m_IsEnemyForEveryone)
			{
				return true;
			}
			if (g2.IsPlayerParty || g2.m_AttackFactions == null)
			{
				return false;
			}
			for (int i = 0; i < g2.m_AttackFactions.Count; i++)
			{
				BlueprintFaction blueprintFaction = g2.m_AttackFactions[i];
				if (blueprintFaction.AlwaysEnemy)
				{
					return true;
				}
				if (g1.m_Factions.Contains(blueprintFaction))
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsEnemy(UnitGroup group)
	{
		if (!IsEnemy(this, group))
		{
			return IsEnemy(group, this);
		}
		return true;
	}

	public bool IsEnemy(MechanicEntity entity)
	{
		UnitGroup unitGroup = entity.GetCombatGroupOptional()?.Group;
		if (unitGroup != null)
		{
			return IsEnemy(unitGroup);
		}
		return false;
	}

	public bool Any(Func<BaseUnitEntity, bool> pred)
	{
		for (int i = 0; i < m_Units.Count; i++)
		{
			IAbstractUnitEntity entity = m_Units[i].Entity;
			if (entity != null && pred(entity.ToBaseUnitEntity()))
			{
				return true;
			}
		}
		return false;
	}

	public bool All(Func<BaseUnitEntity, bool> pred)
	{
		try
		{
			for (int i = 0; i < m_Units.Count; i++)
			{
				IAbstractUnitEntity entity = m_Units[i].Entity;
				if (entity != null && !pred(entity.ToBaseUnitEntity()))
				{
					return false;
				}
			}
			return true;
		}
		finally
		{
		}
	}

	public IEnumerable<T> Select<T>(Func<BaseUnitEntity, T> select)
	{
		for (int i = 0; i < m_Units.Count; i++)
		{
			IAbstractUnitEntity entity = m_Units[i].Entity;
			if (entity != null)
			{
				yield return select(entity.ToBaseUnitEntity());
			}
		}
	}

	public void ForEach(Action<BaseUnitEntity> action)
	{
		for (int i = 0; i < m_Units.Count; i++)
		{
			IAbstractUnitEntity entity = m_Units[i].Entity;
			if (entity != null)
			{
				action(entity.ToBaseUnitEntity());
			}
		}
	}

	public bool HasLOS(BaseUnitEntity unit)
	{
		for (int i = 0; i < m_Units.Count; i++)
		{
			BaseUnitEntity baseUnitEntity = m_Units[i].Entity.ToBaseUnitEntity();
			if (baseUnitEntity != null && baseUnitEntity.IsInGame && !baseUnitEntity.State.IsHelpless && baseUnitEntity.Vision.HasLOS(unit))
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateAttackFactionsCache()
	{
		m_IsEnemyForEveryone = false;
		m_AttackFactions.Clear();
		if (IsExtra)
		{
			return;
		}
		foreach (UnitReference unit in m_Units)
		{
			BaseUnitEntity baseUnitEntity = unit.Entity.ToBaseUnitEntity();
			if (baseUnitEntity == null)
			{
				LogChannel.Default.Error("UnitGroup.UpdateAttackFactionsCache: can't dereference " + unit.Id);
				continue;
			}
			if (baseUnitEntity.Faction.EnemyForEveryone)
			{
				m_IsEnemyForEveryone = true;
			}
			foreach (BlueprintFaction attackFaction in baseUnitEntity.Faction.AttackFactions)
			{
				if (!m_AttackFactions.HasItem(attackFaction))
				{
					m_AttackFactions.Add(attackFaction);
				}
			}
		}
		Memory.ClearEnemies();
	}

	public void ResetFactionSet()
	{
		m_Factions.Clear();
		foreach (UnitReference unit in m_Units)
		{
			if (unit.Entity != null)
			{
				m_Factions.Add(unit.Entity.ToBaseUnitEntity().Faction.Blueprint);
			}
		}
	}

	public void ExtractNearUnits(HashSet<BaseUnitEntity> result)
	{
		UnitGroupEnumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			foreach (BaseUnitEntity item2 in enumerator.Current.Vision.CanBeInRange)
			{
				if (item2 != null)
				{
					BaseUnitEntity item = item2;
					result.Add(item);
				}
			}
		}
	}

	public bool HasEnemyInCombat()
	{
		return Game.Instance.UnitGroups.Any((UnitGroup other) => other != this && (bool)other.IsInCombat && other.IsEnemy(this));
	}

	public void Dispose()
	{
		m_Units.Clear();
		Memory.Clear();
		Disposed = true;
	}

	public UnitGroupEnumerator GetEnumerator()
	{
		return new UnitGroupEnumerator(m_Units);
	}

	public override string ToString()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append("UnitGroup[");
		bool flag = true;
		foreach (UnitReference unit in m_Units)
		{
			if (!flag)
			{
				builder.Append(", ");
			}
			flag = false;
			string value = ((unit.Entity != null) ? unit.Entity.ToBaseUnitEntity().Blueprint.name : "<missing>");
			builder.Append(value);
		}
		builder.Append("]");
		return builder.ToString();
	}

	int IComparable<UnitGroup>.CompareTo(UnitGroup other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		return string.Compare(Id, other.Id, StringComparison.Ordinal);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(Id);
		Hash128 val = ClassHasher<UnitGroupMemory>.GetHash128(Memory);
		result.Append(ref val);
		bool val2 = IsPlayerParty;
		result.Append(ref val2);
		bool val3 = Disposed;
		result.Append(ref val3);
		return result;
	}
}

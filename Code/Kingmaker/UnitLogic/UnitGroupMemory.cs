using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class UnitGroupMemory : IHashable
{
	public class UnitInfo : IHashable
	{
		[JsonProperty(IsReference = true)]
		private readonly UnitReference m_UnitRef;

		[JsonProperty]
		public TimeSpan LastDetectTime;

		public bool Visible;

		public BaseUnitEntity Unit => m_UnitRef.Entity.ToBaseUnitEntity();

		public UnitReference UnitReference => m_UnitRef;

		[JsonConstructor]
		public UnitInfo(UnitReference unit)
		{
			m_UnitRef = unit;
		}

		public override int GetHashCode()
		{
			return m_UnitRef.GetHashCode();
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			UnitReference obj = m_UnitRef;
			Hash128 val = UnitReferenceHasher.GetHash128(ref obj);
			result.Append(ref val);
			result.Append(ref LastDetectTime);
			return result;
		}
	}

	private readonly SortedDictionary<string, UnitInfo> m_Units = new SortedDictionary<string, UnitInfo>();

	private readonly List<UnitInfo> m_UnitsList = new List<UnitInfo>();

	private bool m_UnitsListValid;

	private readonly List<UnitInfo> m_EnemiesList = new List<UnitInfo>();

	private bool m_EnemiesListValid;

	[JsonProperty]
	public readonly string GroupId;

	private UnitGroup m_Group;

	[JsonProperty]
	[UsedImplicitly]
	private List<UnitInfo> PersistentUnits
	{
		get
		{
			return m_Units.Select((KeyValuePair<string, UnitInfo> pair) => pair.Value).ToList();
		}
		set
		{
			m_Units.Clear();
			foreach (UnitInfo item in value)
			{
				m_Units.Add(item.UnitReference.Id, item);
			}
		}
	}

	public UnitGroup Group => m_Group ?? (m_Group = Game.Instance.UnitGroupsController.GetGroup(GroupId));

	public List<UnitInfo> UnitsList
	{
		get
		{
			if (!m_UnitsListValid)
			{
				m_UnitsList.Clear();
				foreach (KeyValuePair<string, UnitInfo> unit2 in m_Units)
				{
					BaseUnitEntity unit = unit2.Value.Unit;
					if (unit != null && unit.IsInGame)
					{
						m_UnitsList.Add(unit2.Value);
					}
				}
				m_UnitsListValid = true;
			}
			return m_UnitsList;
		}
	}

	public List<UnitInfo> Enemies
	{
		get
		{
			if (!m_EnemiesListValid)
			{
				m_EnemiesList.Clear();
				foreach (UnitInfo value in m_Units.Values)
				{
					BaseUnitEntity unit = value.Unit;
					if (unit != null && unit.IsInGame && Group.IsEnemy(unit))
					{
						m_EnemiesList.Add(value);
					}
				}
				m_EnemiesListValid = true;
			}
			return m_EnemiesList;
		}
	}

	[JsonConstructor]
	public UnitGroupMemory(string groupId)
	{
		GroupId = groupId;
	}

	public UnitInfo Add(BaseUnitEntity unit)
	{
		m_EnemiesListValid = false;
		m_UnitsListValid = false;
		if (!m_Units.TryGetValue(unit.UniqueId, out var value))
		{
			value = new UnitInfo(unit.FromBaseUnitEntity());
			m_Units.Add(unit.UniqueId, value);
		}
		value.LastDetectTime = Game.Instance.TimeController.GameTime;
		return value;
	}

	public void Remove(BaseUnitEntity unit)
	{
		m_EnemiesListValid = false;
		m_UnitsListValid = false;
		m_Units.Remove(unit.UniqueId);
	}

	[CanBeNull]
	public UnitInfo Find(BaseUnitEntity unit)
	{
		m_Units.TryGetValue(unit.UniqueId, out var value);
		return value;
	}

	public void Clear()
	{
		m_UnitsListValid = false;
		ClearEnemies();
		m_UnitsList.Clear();
		m_Units.Clear();
	}

	public void ClearEnemies()
	{
		m_EnemiesListValid = false;
		m_EnemiesList.Clear();
	}

	private bool Contains(MechanicEntity unit, bool visibleOnly)
	{
		try
		{
			UnitInfo unitInfo = m_Units.Get(unit.UniqueId);
			return unitInfo != null && (!visibleOnly || unitInfo.Visible);
		}
		finally
		{
		}
	}

	public bool Contains(MechanicEntity unit)
	{
		return Contains(unit, visibleOnly: false);
	}

	public bool ContainsVisible(MechanicEntity unit)
	{
		return Contains(unit, visibleOnly: true);
	}

	public void Cleanup()
	{
		List<UnitInfo> list = TempList.Get<UnitInfo>();
		foreach (KeyValuePair<string, UnitInfo> unit2 in m_Units)
		{
			UnitInfo value = unit2.Value;
			BaseUnitEntity unit = value.Unit;
			if (unit == null || !unit.IsInGame)
			{
				list.Add(value);
			}
		}
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				m_Units.Remove(list[i].UnitReference.Id);
			}
			list.Clear();
		}
		m_EnemiesListValid = false;
		m_UnitsListValid = false;
	}

	public bool HasPlayerCharacterInMemory()
	{
		foreach (UnitInfo units in UnitsList)
		{
			if (units.Unit.Faction.IsPlayer)
			{
				return true;
			}
		}
		return false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<UnitInfo> persistentUnits = PersistentUnits;
		if (persistentUnits != null)
		{
			for (int i = 0; i < persistentUnits.Count; i++)
			{
				Hash128 val = ClassHasher<UnitInfo>.GetHash128(persistentUnits[i]);
				result.Append(ref val);
			}
		}
		result.Append(GroupId);
		return result;
	}
}

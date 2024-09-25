using System;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.View.Roaming;
using Newtonsoft.Json;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartRoaming : EntityPart<AbstractUnitEntity>, IHashable
{
	[CanBeNull]
	[JsonProperty]
	[HasherCustom(Type = typeof(IRoamingPointHasher))]
	private IRoamingPoint m_PersistentNextPoint;

	[JsonProperty]
	private string m_PersistentNextPointId = "";

	[JsonProperty]
	private bool m_Disabled;

	[CanBeNull]
	private IRoamingPoint m_NextPoint;

	[CanBeNull]
	public IRoamingPoint NextPoint
	{
		get
		{
			return m_NextPoint;
		}
		set
		{
			m_NextPoint = value;
			if (value is RoamingWaypointData roamingWaypointData)
			{
				m_PersistentNextPointId = roamingWaypointData.UniqueId;
				m_PersistentNextPoint = null;
			}
			else
			{
				m_PersistentNextPointId = "";
				m_PersistentNextPoint = value;
			}
		}
	}

	[JsonProperty(IsReference = false)]
	public Vector3 OriginalPoint { get; set; }

	[JsonProperty]
	public bool ReverseDirection { get; set; }

	[CanBeNull]
	[JsonProperty]
	public CutscenePlayerData IdleCutscene { get; set; }

	[JsonProperty]
	public TimeSpan IdleEndTime { get; set; }

	[CanBeNull]
	public RoamingUnitSettings Settings { get; set; }

	public Vector3? CachedTargetPosition { get; set; }

	public ABPath PathInProcess { get; set; }

	public bool Disabled
	{
		get
		{
			if (!m_Disabled)
			{
				return base.Owner.GetOptional<UnitPartFollowUnit>() != null;
			}
			return true;
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		IRoamingPoint nextPoint;
		if (string.IsNullOrEmpty(m_PersistentNextPointId))
		{
			nextPoint = m_PersistentNextPoint;
		}
		else
		{
			IRoamingPoint entity = EntityService.Instance.GetEntity<RoamingWaypointData>(m_PersistentNextPointId);
			nextPoint = entity;
		}
		m_NextPoint = nextPoint;
	}

	public void SetEnabled(bool enabled)
	{
		m_Disabled = !enabled;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = IRoamingPointHasher.GetHash128(m_PersistentNextPoint);
		result.Append(ref val2);
		result.Append(m_PersistentNextPointId);
		result.Append(ref m_Disabled);
		Vector3 val3 = OriginalPoint;
		result.Append(ref val3);
		bool val4 = ReverseDirection;
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<CutscenePlayerData>.GetHash128(IdleCutscene);
		result.Append(ref val5);
		TimeSpan val6 = IdleEndTime;
		result.Append(ref val6);
		return result;
	}
}

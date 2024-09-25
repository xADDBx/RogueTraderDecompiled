using System.Collections.Generic;
using Kingmaker.Controllers.FogOfWar.LineOfSight;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class UnitSightCache
{
	private struct CacheEntry
	{
		public readonly Vector3 Position;

		public readonly bool HasLOS;

		public CacheEntry(Vector3 position, bool hasLOS)
		{
			Position = position;
			HasLOS = hasLOS;
		}
	}

	private const float MaxPositionError = 0.5f;

	private const float MaxPositionError2 = 0.25f;

	private readonly Dictionary<string, CacheEntry> m_Cache = new Dictionary<string, CacheEntry>();

	private readonly BaseUnitEntity m_Owner;

	private bool m_HasPosition;

	private Vector3 m_LastSamplePosition;

	private float m_LastVisionRadius;

	private float m_LastGeometryUpdateTime;

	public UnitSightCache(BaseUnitEntity owner)
	{
		m_Owner = owner;
	}

	public bool GetLOS(BaseUnitEntity other)
	{
		if (m_Cache.TryGetValue(other.UniqueId, out var value) && GeometryUtils.SqrDistance2D(value.Position, other.Position) < 0.25f)
		{
			return value.HasLOS;
		}
		if (!m_HasPosition)
		{
			m_HasPosition = true;
			m_LastSamplePosition = m_Owner.Position;
		}
		value = new CacheEntry(other.Position, m_Owner.Vision.HasLOS(other.Position + LosCalculations.EyeShift, m_LastSamplePosition));
		m_Cache[other.UniqueId] = value;
		return value.HasLOS;
	}

	public void ClearIfNeeded()
	{
		if (GeometryUtils.SqrDistance2D(m_Owner.Position, m_LastSamplePosition) >= 0.25f)
		{
			Clear();
		}
		else if (LineOfSightGeometry.Instance.LastUpdateTime > m_LastGeometryUpdateTime)
		{
			Clear();
		}
		else if (m_Owner.Vision.RangeMeters != m_LastVisionRadius)
		{
			Clear();
		}
	}

	private void Clear()
	{
		m_Cache.Clear();
		m_LastSamplePosition = m_Owner.Position;
		m_HasPosition = true;
		m_LastVisionRadius = m_Owner.Vision.RangeMeters;
		m_LastGeometryUpdateTime = LineOfSightGeometry.Instance.LastUpdateTime;
	}
}

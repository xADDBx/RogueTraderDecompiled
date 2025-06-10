using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartStrategistKeystoneRearAbilityFlipPattern : BaseUnitPart, IHashable
{
	public enum FlipStates
	{
		FlipNone,
		FlipVertical,
		FlipHorizontal
	}

	[JsonIgnore]
	private FlipStates m_FlipState = FlipStates.FlipHorizontal;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private Dictionary<string, FlipStates> m_AreaEffectEntityIds = new Dictionary<string, FlipStates>();

	[JsonIgnore]
	public FlipStates FlipPattern
	{
		get
		{
			return m_FlipState;
		}
		set
		{
			m_FlipState = value;
		}
	}

	public void Flip()
	{
		m_FlipState = ((m_FlipState == FlipStates.FlipHorizontal) ? FlipStates.FlipVertical : FlipStates.FlipHorizontal);
	}

	public Vector3 GetOverrideDirection()
	{
		return GetOverrideDirection(m_FlipState);
	}

	public Vector3 GetOverrideDirection(string areaEffectUniqueId, Vector3 @default)
	{
		if (!m_AreaEffectEntityIds.TryGetValue(areaEffectUniqueId, out var value))
		{
			return @default;
		}
		return GetOverrideDirection(value);
	}

	private Vector3 GetOverrideDirection(FlipStates flipState)
	{
		if (flipState != FlipStates.FlipVertical)
		{
			_ = 2;
			return new Vector3(1f, 0f, 1f);
		}
		return new Vector3(1f, 0f, 0f);
	}

	public void Reset()
	{
		m_FlipState = FlipStates.FlipHorizontal;
	}

	public void AddAreaEffect(AreaEffectEntity areaEffectEntity)
	{
		if (areaEffectEntity != null && !m_AreaEffectEntityIds.ContainsKey(areaEffectEntity.UniqueId) && areaEffectEntity.Context?.SourceAbility != null && areaEffectEntity.Context.SourceAbility.HasLogic<IsFlipZoneAbility>())
		{
			m_AreaEffectEntityIds.Add(areaEffectEntity.UniqueId, m_FlipState);
		}
	}

	public void RemoveAreaEffect(AreaEffectEntity areaEffectEntity)
	{
		if (areaEffectEntity != null)
		{
			m_AreaEffectEntityIds.Remove(areaEffectEntity.UniqueId);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<string, FlipStates> areaEffectEntityIds = m_AreaEffectEntityIds;
		if (areaEffectEntityIds != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<string, FlipStates> item in areaEffectEntityIds)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = StringHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				FlipStates obj = item.Value;
				Hash128 val4 = UnmanagedHasher<FlipStates>.GetHash128(ref obj);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		return result;
	}
}

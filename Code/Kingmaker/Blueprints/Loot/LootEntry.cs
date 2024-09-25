using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Networking.Serialization;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

[Serializable]
public class LootEntry : IHashable
{
	[SerializeField]
	[GameStateInclude]
	private BlueprintItemReference m_Item;

	[SerializeField]
	[JsonProperty]
	public int Diversity;

	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	public int Count = 1;

	[NonSerialized]
	public bool Identify;

	[SerializeField]
	private int m_ReputationPointsToUnlock;

	[SerializeField]
	private long? m_ProfitFactorCostOverride;

	[JsonProperty]
	[GameStateIgnore]
	public BlueprintItem Item
	{
		get
		{
			return m_Item?.Get();
		}
		set
		{
			m_Item = value.ToReference<BlueprintItemReference>();
		}
	}

	public long? ProfitFactorCostOverride
	{
		get
		{
			return m_ProfitFactorCostOverride;
		}
		set
		{
			m_ProfitFactorCostOverride = value;
		}
	}

	public int ReputationPointsToUnlock
	{
		get
		{
			return m_ReputationPointsToUnlock;
		}
		set
		{
			m_ReputationPointsToUnlock = value;
		}
	}

	public float CargoVolumePercent => Item ? (Item.CargoVolumePercent * Count) : 0;

	public float ProfitFactorCost
	{
		get
		{
			if ((bool)Item)
			{
				return Item.ProfitFactorCost * (float)Count;
			}
			return 0f;
		}
	}

	public bool IsDuplicate(LootEntry other)
	{
		if (other == null)
		{
			return false;
		}
		if (object.Equals(Item, other.Item) && Count == other.Count && ReputationPointsToUnlock == other.ReputationPointsToUnlock && ProfitFactorCostOverride == other?.ProfitFactorCostOverride && Diversity == other.Diversity && Mathf.Abs(CargoVolumePercent - other.CargoVolumePercent) < 1.06f)
		{
			return this != other;
		}
		return false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(m_Item);
		result.Append(ref val);
		result.Append(ref Diversity);
		result.Append(ref Count);
		return result;
	}
}

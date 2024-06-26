using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[Serializable]
public class VendorLootItem : IEquatable<VendorLootItem>, IComparable<VendorLootItem>, IHashable
{
	[JsonProperty]
	public BlueprintItem Item { get; }

	[JsonProperty]
	public int Diversity { get; }

	[JsonProperty]
	public int Count { get; private set; }

	[JsonProperty]
	public int ReputationToUnlock { get; }

	[JsonProperty]
	public int ProfitFactorCosts { get; }

	[JsonConstructor]
	private VendorLootItem()
	{
	}

	public VendorLootItem(LootItemsPackFixed pack)
		: this(pack.Item.Item, pack.Item.Diversity, pack.Count, pack.ReputationPointsToUnlock, pack.Item.ProfitFactorCostOverride.GetValueOrDefault())
	{
	}

	public VendorLootItem(BlueprintItem item, int diversity, int count, int reputationToUnlock, int profitFactorCosts)
	{
		Item = item;
		Diversity = diversity;
		Count = count;
		ReputationToUnlock = reputationToUnlock;
		ProfitFactorCosts = profitFactorCosts;
	}

	public void UpdateCount(int newCount)
	{
		Count += newCount;
	}

	public bool Equals(VendorLootItem other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (object.Equals(Item, other.Item) && Count == other.Count && ReputationToUnlock == other.ReputationToUnlock && ProfitFactorCosts == other.ProfitFactorCosts)
		{
			return Diversity == other.Diversity;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((VendorLootItem)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Item, ReputationToUnlock, ProfitFactorCosts, Diversity);
	}

	public int CompareTo(VendorLootItem other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		int num = Count.CompareTo(other.Count);
		if (num != 0)
		{
			return num;
		}
		int num2 = ReputationToUnlock.CompareTo(other.ReputationToUnlock);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = Diversity.CompareTo(other.Diversity);
		if (num3 != 0)
		{
			return num3;
		}
		return ProfitFactorCosts.CompareTo(other.ProfitFactorCosts);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Item);
		result.Append(ref val);
		int val2 = Diversity;
		result.Append(ref val2);
		int val3 = Count;
		result.Append(ref val3);
		int val4 = ReputationToUnlock;
		result.Append(ref val4);
		int val5 = ProfitFactorCosts;
		result.Append(ref val5);
		return result;
	}
}

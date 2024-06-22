using System;
using Kingmaker.Blueprints.Items;
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
	public int Count { get; private set; }

	[JsonProperty]
	public int ReputationToUnlock { get; }

	[JsonProperty]
	public int ProfitFactorCosts { get; }

	public VendorLootItem(BlueprintItem item, int count, int reputationToUnlock, int profitFactorCosts)
	{
		Item = item;
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
		if (object.Equals(Item, other.Item) && Count == other.Count && ReputationToUnlock == other.ReputationToUnlock)
		{
			return ProfitFactorCosts == other.ProfitFactorCosts;
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
		return HashCode.Combine(Item, ReputationToUnlock, ProfitFactorCosts);
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
		return ProfitFactorCosts.CompareTo(other.ProfitFactorCosts);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Item);
		result.Append(ref val);
		int val2 = Count;
		result.Append(ref val2);
		int val3 = ReputationToUnlock;
		result.Append(ref val3);
		int val4 = ProfitFactorCosts;
		result.Append(ref val4);
		return result;
	}
}

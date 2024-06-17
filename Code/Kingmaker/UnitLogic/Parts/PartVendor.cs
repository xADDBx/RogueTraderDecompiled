using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Controllers;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartVendor : PartItemsCollection, IHashable
{
	[NotNull]
	[JsonProperty]
	private List<BlueprintUnitLoot> m_OwnLoot = new List<BlueprintUnitLoot>();

	[NotNull]
	[JsonProperty]
	private Dictionary<BlueprintItem, int> m_OwnKnownItems = new Dictionary<BlueprintItem, int>();

	[JsonProperty]
	private BlueprintSharedVendorTable m_SharedInventory;

	[JsonProperty]
	private BlueprintVendorFaction.Reference m_VendorFactionReference;

	[JsonProperty]
	private Dictionary<BlueprintItem, int> m_ReputationToUnlock = new Dictionary<BlueprintItem, int>();

	[JsonProperty]
	private Dictionary<BlueprintItem, long> m_OverrideProfitFactorCosts = new Dictionary<BlueprintItem, long>();

	public FactionType FactionType => Faction.FactionType;

	public BlueprintVendorFaction Faction => m_VendorFactionReference.Get();

	public bool AutoIdentifyPlayersInventory => m_SharedInventory?.AutoIdentifyAllItems ?? false;

	public int GetCurrentFactionReputationPoints()
	{
		return ReputationHelper.GetCurrentReputationPoints(FactionType);
	}

	public bool IsLockedByReputation(BlueprintItem item)
	{
		return GetReputationToUnlock(item) > ReputationHelper.GetCurrentReputationPoints(FactionType);
	}

	public IReadOnlyDictionary<BlueprintItem, int> GetReputationToUnlock()
	{
		if (m_SharedInventory == null)
		{
			return m_ReputationToUnlock;
		}
		return Game.Instance.Player.SharedVendorTables.GetReputationToUnlock(m_SharedInventory);
	}

	public IReadOnlyDictionary<BlueprintItem, long> GetOverridePrices()
	{
		if (m_SharedInventory == null)
		{
			return m_OverrideProfitFactorCosts;
		}
		return Game.Instance.Player.SharedVendorTables.GetOverridePrices(m_SharedInventory);
	}

	public int GetReputationToUnlock(BlueprintItem item)
	{
		if (!GetReputationToUnlock().TryGetValue(item, out var value))
		{
			return 0;
		}
		return value;
	}

	public float GetProfitFactorCost(BlueprintItem item)
	{
		long value;
		float num = (GetOverridePrices().TryGetValue(item, out value) ? ((float)value) : item.ProfitFactorCost);
		FactionType factionType = FactionType;
		Game.Instance.Player.ProfitFactor.VendorDiscounts.TryGetValue(factionType, out var value2);
		return Mathf.Max(0f, num - (float)Mathf.Abs(value2));
	}

	public float GetBaseProfitFactorCost(BlueprintItem item)
	{
		if (!GetOverridePrices().TryGetValue(item, out var value))
		{
			return item.ProfitFactorCost;
		}
		return value;
	}

	private void CalculateSharedTableCosts()
	{
		if (m_SharedInventory == null)
		{
			SharedVendorTables.CalculateCosts(m_OwnLoot, ref m_ReputationToUnlock, ref m_OverrideProfitFactorCosts);
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (base.Collection != null && m_SharedInventory == null)
		{
			Dictionary<BlueprintItem, int> fixedItems = SharedVendorTables.GetFixedItems(m_OwnLoot);
			foreach (KeyValuePair<BlueprintItem, int> item in SharedVendorTables.GetLootDifference(m_OwnKnownItems, fixedItems))
			{
				BlueprintItem key = item.Key;
				int value = item.Value;
				if (value > 0)
				{
					base.Collection.Add(key, value);
				}
				else
				{
					base.Collection.Remove(key, -value);
				}
			}
			m_OwnKnownItems = fixedItems;
		}
		CalculateSharedTableCosts();
	}

	protected override ItemsCollection SetupInternal(ItemsCollection currentCollection)
	{
		if (m_SharedInventory != null)
		{
			return Game.Instance.Player.SharedVendorTables.GetCollection(m_SharedInventory);
		}
		return base.Collection ?? new ItemsCollection(base.ConcreteOwner)
		{
			ForceStackable = true,
			IsVendorTable = true
		};
	}

	protected override void OnCollectionChanged()
	{
		base.OnCollectionChanged();
		CalculateSharedTableCosts();
	}

	public void SetSharedInventory(BlueprintSharedVendorTable loot)
	{
		m_SharedInventory = loot;
		m_OwnKnownItems.Clear();
		m_OwnLoot.Clear();
		Setup();
	}

	public void SetVendorFaction(BlueprintVendorFaction vendorFaction)
	{
		m_VendorFactionReference = vendorFaction.ToReference<BlueprintVendorFaction.Reference>();
	}

	public void AddLoot(BlueprintUnitLoot loot)
	{
		if (loot == null)
		{
			return;
		}
		if (m_SharedInventory != null)
		{
			Game.Instance.Player.SharedVendorTables.AddLoot(m_SharedInventory, loot);
			return;
		}
		foreach (LootEntry item in loot.GenerateItems())
		{
			base.Collection.Add(item.Item, item.Count);
		}
		m_OwnLoot.Add(loot);
		m_OwnKnownItems = SharedVendorTables.GetFixedItems(m_OwnLoot);
		CalculateSharedTableCosts();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<BlueprintUnitLoot> ownLoot = m_OwnLoot;
		if (ownLoot != null)
		{
			for (int i = 0; i < ownLoot.Count; i++)
			{
				Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(ownLoot[i]);
				result.Append(ref val2);
			}
		}
		Dictionary<BlueprintItem, int> ownKnownItems = m_OwnKnownItems;
		if (ownKnownItems != null)
		{
			int val3 = 0;
			foreach (KeyValuePair<BlueprintItem, int> item in ownKnownItems)
			{
				Hash128 hash = default(Hash128);
				Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val4);
				int obj = item.Value;
				Hash128 val5 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val5);
				val3 ^= hash.GetHashCode();
			}
			result.Append(ref val3);
		}
		Hash128 val6 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_SharedInventory);
		result.Append(ref val6);
		Hash128 val7 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(m_VendorFactionReference);
		result.Append(ref val7);
		Dictionary<BlueprintItem, int> reputationToUnlock = m_ReputationToUnlock;
		if (reputationToUnlock != null)
		{
			int val8 = 0;
			foreach (KeyValuePair<BlueprintItem, int> item2 in reputationToUnlock)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val9 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2.Key);
				hash2.Append(ref val9);
				int obj2 = item2.Value;
				Hash128 val10 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash2.Append(ref val10);
				val8 ^= hash2.GetHashCode();
			}
			result.Append(ref val8);
		}
		Dictionary<BlueprintItem, long> overrideProfitFactorCosts = m_OverrideProfitFactorCosts;
		if (overrideProfitFactorCosts != null)
		{
			int val11 = 0;
			foreach (KeyValuePair<BlueprintItem, long> item3 in overrideProfitFactorCosts)
			{
				Hash128 hash3 = default(Hash128);
				Hash128 val12 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item3.Key);
				hash3.Append(ref val12);
				long obj3 = item3.Value;
				Hash128 val13 = UnmanagedHasher<long>.GetHash128(ref obj3);
				hash3.Append(ref val13);
				val11 ^= hash3.GetHashCode();
			}
			result.Append(ref val11);
		}
		return result;
	}
}

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

	[NotNull]
	[JsonProperty]
	private HashSet<VendorLootItem> m_OwnKnownLootItems = new HashSet<VendorLootItem>(new VendorLootItemEqualityComparer());

	[JsonProperty]
	private Dictionary<ItemEntity, VendorLootItem> m_ItemEntityToVendorLootItem = new Dictionary<ItemEntity, VendorLootItem>();

	[JsonProperty]
	private BlueprintSharedVendorTable m_SharedInventory;

	[JsonProperty]
	private BlueprintVendorFaction.Reference m_VendorFactionReference;

	[JsonProperty]
	private Dictionary<BlueprintItem, int> m_ReputationToUnlock = new Dictionary<BlueprintItem, int>();

	[JsonProperty]
	private Dictionary<BlueprintItem, int> m_OverrideProfitFactorCosts = new Dictionary<BlueprintItem, int>();

	public FactionType FactionType => Faction.FactionType;

	public BlueprintVendorFaction Faction => m_VendorFactionReference.Get();

	public bool AutoIdentifyPlayersInventory => m_SharedInventory?.AutoIdentifyAllItems ?? false;

	private IReadOnlyDictionary<ItemEntity, VendorLootItem> GetItemEntityToVendorLootItemPairs()
	{
		if (m_SharedInventory == null)
		{
			return m_ItemEntityToVendorLootItem;
		}
		return Game.Instance.Player.SharedVendorTables.GetItemEntityToVendorLootItemPairs(m_SharedInventory);
	}

	public int GetCurrentFactionReputationPoints()
	{
		return ReputationHelper.GetCurrentReputationPoints(FactionType);
	}

	public bool IsLockedByReputation(ItemEntity item)
	{
		return GetReputationToUnlock(item) > ReputationHelper.GetCurrentReputationPoints(FactionType);
	}

	[CanBeNull]
	public VendorLootItem GetVendorLootItem(ItemEntity item)
	{
		if (GetItemEntityToVendorLootItemPairs().TryGetValue(item, out var value))
		{
			return value;
		}
		PFLog.Default.Error("PartVendor: cannot find ItemEntity " + item.Name + " in VendorLootItems");
		return null;
	}

	public int GetReputationToUnlock(ItemEntity item)
	{
		return GetVendorLootItem(item)?.ReputationToUnlock ?? 0;
	}

	public float GetProfitFactorCost(ItemEntity item)
	{
		VendorLootItem value;
		float num = (GetItemEntityToVendorLootItemPairs().TryGetValue(item, out value) ? ((float)value.ProfitFactorCosts) : item.ProfitFactorCost);
		FactionType factionType = FactionType;
		Game.Instance.Player.ProfitFactor.VendorDiscounts.TryGetValue(factionType, out var value2);
		return Mathf.Max(0f, num - (float)Mathf.Abs(value2));
	}

	public float GetBaseProfitFactorCost(ItemEntity item)
	{
		if (!GetItemEntityToVendorLootItemPairs().TryGetValue(item, out var value))
		{
			return item.ProfitFactorCost;
		}
		return value.ProfitFactorCosts;
	}

	private void CalculateSharedTableCosts()
	{
		if (m_SharedInventory == null)
		{
			m_OwnKnownLootItems = SharedVendorTables.GetFixedItems(m_OwnLoot);
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (base.Collection != null && m_SharedInventory == null)
		{
			UpdateKnownLootItems(m_OwnLoot, m_OwnKnownLootItems, base.Collection);
		}
	}

	public static HashSet<VendorLootItem> UpdateKnownLootItems(List<BlueprintUnitLoot> ownLoot, HashSet<VendorLootItem> ownKnownLootItems, ItemsCollection collection)
	{
		HashSet<VendorLootItem> fixedItems = SharedVendorTables.GetFixedItems(ownLoot);
		List<LootEntry> list = new List<LootEntry>();
		foreach (KeyValuePair<VendorLootItem, int> item2 in SharedVendorTables.GetLootDifference(ownKnownLootItems, fixedItems, list))
		{
			BlueprintItem item = item2.Key.Item;
			int value = item2.Value;
			if (value > 0)
			{
				collection.Add(item, value);
			}
			else
			{
				collection.Remove(item, -value);
			}
		}
		foreach (LootEntry item3 in list)
		{
			collection.Add(item3.Item, item3.Count);
		}
		return fixedItems;
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
		m_OwnKnownLootItems.Clear();
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
		m_OwnKnownLootItems = SharedVendorTables.GetFixedItems(m_OwnLoot);
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
		HashSet<VendorLootItem> ownKnownLootItems = m_OwnKnownLootItems;
		if (ownKnownLootItems != null)
		{
			int num = 0;
			foreach (VendorLootItem item2 in ownKnownLootItems)
			{
				num ^= ClassHasher<VendorLootItem>.GetHash128(item2).GetHashCode();
			}
			result.Append(num);
		}
		Dictionary<ItemEntity, VendorLootItem> itemEntityToVendorLootItem = m_ItemEntityToVendorLootItem;
		if (itemEntityToVendorLootItem != null)
		{
			int val6 = 0;
			foreach (KeyValuePair<ItemEntity, VendorLootItem> item3 in itemEntityToVendorLootItem)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val7 = ClassHasher<ItemEntity>.GetHash128(item3.Key);
				hash2.Append(ref val7);
				Hash128 val8 = ClassHasher<VendorLootItem>.GetHash128(item3.Value);
				hash2.Append(ref val8);
				val6 ^= hash2.GetHashCode();
			}
			result.Append(ref val6);
		}
		Hash128 val9 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_SharedInventory);
		result.Append(ref val9);
		Hash128 val10 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(m_VendorFactionReference);
		result.Append(ref val10);
		Dictionary<BlueprintItem, int> reputationToUnlock = m_ReputationToUnlock;
		if (reputationToUnlock != null)
		{
			int val11 = 0;
			foreach (KeyValuePair<BlueprintItem, int> item4 in reputationToUnlock)
			{
				Hash128 hash3 = default(Hash128);
				Hash128 val12 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item4.Key);
				hash3.Append(ref val12);
				int obj2 = item4.Value;
				Hash128 val13 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash3.Append(ref val13);
				val11 ^= hash3.GetHashCode();
			}
			result.Append(ref val11);
		}
		Dictionary<BlueprintItem, int> overrideProfitFactorCosts = m_OverrideProfitFactorCosts;
		if (overrideProfitFactorCosts != null)
		{
			int val14 = 0;
			foreach (KeyValuePair<BlueprintItem, int> item5 in overrideProfitFactorCosts)
			{
				Hash128 hash4 = default(Hash128);
				Hash128 val15 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item5.Key);
				hash4.Append(ref val15);
				int obj3 = item5.Value;
				Hash128 val16 = UnmanagedHasher<int>.GetHash128(ref obj3);
				hash4.Append(ref val16);
				val14 ^= hash4.GetHashCode();
			}
			result.Append(ref val14);
		}
		return result;
	}
}

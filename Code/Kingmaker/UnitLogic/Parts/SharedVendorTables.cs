using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.DLC;
using Kingmaker.Items;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class SharedVendorTables : IHashable
{
	public class TableData : IHashable
	{
		[JsonProperty]
		public BlueprintSharedVendorTable Table;

		[JsonProperty]
		public readonly List<LootEntry> Entries = new List<LootEntry>();

		[JsonProperty]
		public readonly List<BlueprintUnitLoot> Loot = new List<BlueprintUnitLoot>();

		[JsonProperty]
		public Dictionary<BlueprintItem, int> KnownItems;

		[JsonProperty]
		public HashSet<VendorLootItem> KnownLootItems = new HashSet<VendorLootItem>(new VendorLootItemEqualityComparer());

		[JsonProperty]
		public Dictionary<ItemEntity, VendorLootItem> ItemEntityToVendorLootItem = new Dictionary<ItemEntity, VendorLootItem>();

		[JsonProperty]
		public Dictionary<BlueprintItem, int> ReputationToUnlock = new Dictionary<BlueprintItem, int>();

		[JsonProperty]
		public Dictionary<BlueprintItem, int> OverrideProfitFactorCosts = new Dictionary<BlueprintItem, int>();

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Table);
			result.Append(ref val);
			List<LootEntry> entries = Entries;
			if (entries != null)
			{
				for (int i = 0; i < entries.Count; i++)
				{
					Hash128 val2 = ClassHasher<LootEntry>.GetHash128(entries[i]);
					result.Append(ref val2);
				}
			}
			List<BlueprintUnitLoot> loot = Loot;
			if (loot != null)
			{
				for (int j = 0; j < loot.Count; j++)
				{
					Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(loot[j]);
					result.Append(ref val3);
				}
			}
			Dictionary<BlueprintItem, int> knownItems = KnownItems;
			if (knownItems != null)
			{
				int val4 = 0;
				foreach (KeyValuePair<BlueprintItem, int> item in knownItems)
				{
					Hash128 hash = default(Hash128);
					Hash128 val5 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
					hash.Append(ref val5);
					int obj = item.Value;
					Hash128 val6 = UnmanagedHasher<int>.GetHash128(ref obj);
					hash.Append(ref val6);
					val4 ^= hash.GetHashCode();
				}
				result.Append(ref val4);
			}
			HashSet<VendorLootItem> knownLootItems = KnownLootItems;
			if (knownLootItems != null)
			{
				int num = 0;
				foreach (VendorLootItem item2 in knownLootItems)
				{
					num ^= ClassHasher<VendorLootItem>.GetHash128(item2).GetHashCode();
				}
				result.Append(num);
			}
			Dictionary<ItemEntity, VendorLootItem> itemEntityToVendorLootItem = ItemEntityToVendorLootItem;
			if (itemEntityToVendorLootItem != null)
			{
				int val7 = 0;
				foreach (KeyValuePair<ItemEntity, VendorLootItem> item3 in itemEntityToVendorLootItem)
				{
					Hash128 hash2 = default(Hash128);
					Hash128 val8 = ClassHasher<ItemEntity>.GetHash128(item3.Key);
					hash2.Append(ref val8);
					Hash128 val9 = ClassHasher<VendorLootItem>.GetHash128(item3.Value);
					hash2.Append(ref val9);
					val7 ^= hash2.GetHashCode();
				}
				result.Append(ref val7);
			}
			Dictionary<BlueprintItem, int> reputationToUnlock = ReputationToUnlock;
			if (reputationToUnlock != null)
			{
				int val10 = 0;
				foreach (KeyValuePair<BlueprintItem, int> item4 in reputationToUnlock)
				{
					Hash128 hash3 = default(Hash128);
					Hash128 val11 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item4.Key);
					hash3.Append(ref val11);
					int obj2 = item4.Value;
					Hash128 val12 = UnmanagedHasher<int>.GetHash128(ref obj2);
					hash3.Append(ref val12);
					val10 ^= hash3.GetHashCode();
				}
				result.Append(ref val10);
			}
			Dictionary<BlueprintItem, int> overrideProfitFactorCosts = OverrideProfitFactorCosts;
			if (overrideProfitFactorCosts != null)
			{
				int val13 = 0;
				foreach (KeyValuePair<BlueprintItem, int> item5 in overrideProfitFactorCosts)
				{
					Hash128 hash4 = default(Hash128);
					Hash128 val14 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item5.Key);
					hash4.Append(ref val14);
					int obj3 = item5.Value;
					Hash128 val15 = UnmanagedHasher<int>.GetHash128(ref obj3);
					hash4.Append(ref val15);
					val13 ^= hash4.GetHashCode();
				}
				result.Append(ref val13);
			}
			return result;
		}
	}

	private readonly Dictionary<BlueprintSharedVendorTable, ItemsCollection> m_Dictionary = new Dictionary<BlueprintSharedVendorTable, ItemsCollection>();

	[JsonProperty]
	private readonly List<TableData> m_PersistentTables = new List<TableData>();

	public void PostLoad()
	{
		foreach (TableData persistentTable in m_PersistentTables)
		{
			TryConvertToVendorLootItem(persistentTable.KnownLootItems, persistentTable.KnownItems, persistentTable.ReputationToUnlock, persistentTable.OverrideProfitFactorCosts);
		}
	}

	public void Subscribe()
	{
		foreach (ItemsCollection value in m_Dictionary.Values)
		{
			value.Subscribe();
		}
	}

	public void Unsubscribe()
	{
		foreach (ItemsCollection value in m_Dictionary.Values)
		{
			value.Unsubscribe();
		}
	}

	public void PreSave()
	{
		foreach (KeyValuePair<BlueprintSharedVendorTable, ItemsCollection> pair in m_Dictionary)
		{
			TableData tableData = m_PersistentTables.SingleOrDefault((TableData t) => t.Table == pair.Key);
			if (tableData == null)
			{
				PFLog.Default.Warning($"{pair.Key} shared table was not persisted. skipping");
				continue;
			}
			tableData.Entries.Clear();
			tableData.Entries.AddRange(pair.Value.Items.Select((ItemEntity i) => new LootEntry
			{
				Item = i.Blueprint,
				Count = i.Count
			}));
		}
	}

	public void AddLoot(BlueprintSharedVendorTable blueprint, BlueprintUnitLoot loot)
	{
		ItemsCollection collection = GetCollection(blueprint);
		List<LootEntry> list = loot.GenerateItems();
		foreach (LootEntry item in list)
		{
			collection.Add(item.Item, item.Count);
		}
		TableData tableData = m_PersistentTables.SingleOrDefault((TableData t) => t.Table == blueprint);
		if (tableData != null)
		{
			tableData.Loot.Add(loot);
			tableData.KnownLootItems = GetFixedItems(tableData.Loot);
			tableData.Entries.AddRange(list);
			MakeItemEntityToVendorLootItemPairs(collection.Items, tableData.KnownLootItems, tableData.ItemEntityToVendorLootItem);
		}
	}

	public TableData GetTable(BlueprintSharedVendorTable blueprint)
	{
		return m_PersistentTables.SingleOrDefault((TableData t) => t.Table == blueprint);
	}

	public ItemsCollection GetCollection(BlueprintSharedVendorTable blueprint)
	{
		if (!blueprint)
		{
			PFLog.Default.Error("SharedVendorTables: blueprint is null");
			return null;
		}
		if (!m_Dictionary.TryGetValue(blueprint, out var value))
		{
			value = new ItemsCollection(Game.Instance.Player)
			{
				ForceStackable = true,
				IsVendorTable = true
			};
			TableData tableData = m_PersistentTables.SingleOrDefault((TableData t) => t.Table == blueprint);
			if (tableData == null)
			{
				tableData = new TableData
				{
					Table = blueprint
				};
				tableData.Loot.Add(blueprint);
				tableData.Entries.AddRange(blueprint.GenerateItems());
				tableData.KnownLootItems = GetFixedItems(tableData.Loot);
				m_PersistentTables.Add(tableData);
			}
			HashSet<VendorLootItem> fixedItems = GetFixedItems(tableData.Loot);
			if (ShouldFixEntries(tableData.KnownLootItems))
			{
				tableData.Entries.Clear();
				tableData.Entries.AddRange(blueprint.GenerateItems());
			}
			List<LootEntry> entries = tableData.Entries;
			foreach (KeyValuePair<VendorLootItem, int> item in GetLootDifference(tableData.KnownLootItems, fixedItems, entries))
			{
				UpdateLootItems(entries, item.Key, item.Value);
			}
			TryCleanItemsRemovedFromBp(fixedItems, entries);
			tableData.KnownLootItems = fixedItems;
			foreach (LootEntry entry in entries)
			{
				if (entry.Item == null)
				{
					LogChannel.Default.Error($"UnitPartVendor.GetTable: entry.Item is null ({blueprint})");
					continue;
				}
				VendorLootItem vendorLootItem = fixedItems.FindOrDefault((VendorLootItem i) => i.Item == entry.Item && i.Count < entry.Count);
				if (vendorLootItem != null)
				{
					entry.Count = vendorLootItem.Count;
				}
				if (entry.Count > 0)
				{
					value.Add(entry.Item, entry.Count, null, noAutoMerge: true);
				}
			}
			MakeItemEntityToVendorLootItemPairs(value.Items, tableData.KnownLootItems, tableData.ItemEntityToVendorLootItem);
			m_Dictionary[blueprint] = value;
		}
		return value;
	}

	private static bool ShouldFixEntries(HashSet<VendorLootItem> savedKnownLootItems)
	{
		return !savedKnownLootItems.HasItem((VendorLootItem i) => i.Item != null);
	}

	private static void MakeItemEntityToVendorLootItemPairs(ReadonlyList<ItemEntity> items, HashSet<VendorLootItem> vendorLootItems, Dictionary<ItemEntity, VendorLootItem> itemEntityToVendorLootItem)
	{
		itemEntityToVendorLootItem.Clear();
		List<VendorLootItem> list = TempList.Get<VendorLootItem>();
		foreach (ItemEntity item in items)
		{
			foreach (VendorLootItem vendorLootItem in vendorLootItems)
			{
				if (!list.Contains(vendorLootItem) && vendorLootItem.Item == item.Blueprint && vendorLootItem.Count == item.Count)
				{
					itemEntityToVendorLootItem.Add(item, vendorLootItem);
					list.Add(vendorLootItem);
					break;
				}
			}
		}
	}

	public IReadOnlyDictionary<ItemEntity, VendorLootItem> GetItemEntityToVendorLootItemPairs(BlueprintSharedVendorTable blueprint)
	{
		return GetTable(blueprint).ItemEntityToVendorLootItem;
	}

	[NotNull]
	public static HashSet<VendorLootItem> GetFixedItems(List<BlueprintUnitLoot> loot)
	{
		HashSet<VendorLootItem> hashSet = new HashSet<VendorLootItem>();
		foreach (BlueprintUnitLoot item in loot)
		{
			foreach (LootItemsPackFixed component in item.GetComponents<LootItemsPackFixed>())
			{
				if (component.DlcCondition)
				{
					BlueprintDlcReward blueprintDlcReward = component.DlcReward?.Get();
					if (blueprintDlcReward != null && !blueprintDlcReward.IsActive)
					{
						continue;
					}
				}
				BlueprintItem blueprint = component.Item.Item;
				int cost = component.Item.ProfitFactorCostOverride.GetValueOrDefault();
				if (blueprint != null)
				{
					IEnumerable<VendorLootItem> source = hashSet.Where((VendorLootItem i) => i.Item == blueprint && i.ProfitFactorCosts == cost);
					if (source.Empty())
					{
						hashSet.Add(new VendorLootItem(blueprint, component.Count, component.ReputationPointsToUnlock, cost));
					}
					else
					{
						source.First().UpdateCount(component.Count);
					}
				}
			}
		}
		return hashSet;
	}

	public static bool TryConvertToVendorLootItem(HashSet<VendorLootItem> vendorItems, Dictionary<BlueprintItem, int> oldKnownItems, Dictionary<BlueprintItem, int> oldReputationToUnlock, Dictionary<BlueprintItem, int> overrideProfitFactorCosts)
	{
		if (oldKnownItems == null || oldKnownItems.Empty())
		{
			return false;
		}
		foreach (KeyValuePair<BlueprintItem, int> item in oldKnownItems)
		{
			if (!vendorItems.HasItem((VendorLootItem i) => i.Item == item.Key && i.Count == item.Value))
			{
				oldReputationToUnlock.TryGetValue(item.Key, out var value);
				overrideProfitFactorCosts.TryGetValue(item.Key, out var value2);
				VendorLootItem item2 = new VendorLootItem(item.Key, item.Value, value, value2);
				vendorItems.Add(item2);
			}
		}
		return true;
	}

	public static Dictionary<VendorLootItem, int> GetLootDifference(HashSet<VendorLootItem> oldKnownItems, HashSet<VendorLootItem> newKnownItems, List<LootEntry> entry)
	{
		Dictionary<VendorLootItem, int> dictionary = new Dictionary<VendorLootItem, int>();
		HashSet<VendorLootItem> hashSet = new HashSet<VendorLootItem>();
		hashSet.AddRange(newKnownItems);
		IEnumerable<VendorLootItem> enumerable = oldKnownItems.Where((VendorLootItem i) => i.Item != null);
		if (enumerable.Empty())
		{
			return dictionary;
		}
		foreach (VendorLootItem oldItem in enumerable)
		{
			if (!hashSet.HasItem((VendorLootItem i) => i.Equals(oldItem)))
			{
				hashSet.Add(oldItem);
			}
		}
		foreach (VendorLootItem item in hashSet)
		{
			VendorLootItem oldItem = oldKnownItems.FirstOrDefault((VendorLootItem i) => i.Item == item.Item && i.ProfitFactorCosts == item.ProfitFactorCosts && i.ReputationToUnlock == item.ReputationToUnlock);
			VendorLootItem vendorLootItem = newKnownItems.FirstOrDefault((VendorLootItem i) => i.Item == item.Item && i.ProfitFactorCosts == item.ProfitFactorCosts && i.ReputationToUnlock == item.ReputationToUnlock);
			if (oldItem == null && vendorLootItem != null)
			{
				LootEntry item2 = new LootEntry
				{
					Item = item.Item,
					Count = item.Count
				};
				entry.Add(item2);
			}
			if (oldItem != null && vendorLootItem == null)
			{
				entry.Remove((LootEntry i) => i.Item == oldItem.Item);
			}
			if (oldItem != null && vendorLootItem != null && vendorLootItem.Count != oldItem.Count)
			{
				dictionary[vendorLootItem] = vendorLootItem.Count - oldItem.Count;
			}
		}
		return dictionary;
	}

	private static void TryCleanItemsRemovedFromBp(HashSet<VendorLootItem> newKnownItems, List<LootEntry> entry)
	{
		List<LootEntry> list = TempList.Get<LootEntry>();
		foreach (LootEntry lootEntry in entry)
		{
			if (!newKnownItems.HasItem((VendorLootItem i) => i.Item == lootEntry.Item))
			{
				list.Add(lootEntry);
			}
		}
		foreach (LootEntry item in list)
		{
			entry.Remove(item);
		}
	}

	public static void UpdateLootItems(List<LootEntry> entries, VendorLootItem item, int difference)
	{
		foreach (LootEntry entry in entries)
		{
			if (difference == 0)
			{
				return;
			}
			if (entry.Item == item.Item)
			{
				if (entry.Count + difference >= 0)
				{
					entry.Count += difference;
					difference = 0;
				}
				else
				{
					difference += entry.Count;
					entry.Count = 0;
				}
			}
		}
		if (difference > 0)
		{
			LootEntry item2 = new LootEntry
			{
				Item = item.Item,
				Count = difference
			};
			entries.Add(item2);
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<TableData> persistentTables = m_PersistentTables;
		if (persistentTables != null)
		{
			for (int i = 0; i < persistentTables.Count; i++)
			{
				Hash128 val = ClassHasher<TableData>.GetHash128(persistentTables[i]);
				result.Append(ref val);
			}
		}
		return result;
	}
}

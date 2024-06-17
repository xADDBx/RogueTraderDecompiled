using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Items;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class SharedVendorTables : IHashable
{
	internal class TableData : IHashable
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
		public Dictionary<BlueprintItem, int> ReputationToUnlock = new Dictionary<BlueprintItem, int>();

		[JsonProperty]
		public Dictionary<BlueprintItem, long> OverrideProfitFactorCosts = new Dictionary<BlueprintItem, long>();

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
			Dictionary<BlueprintItem, int> reputationToUnlock = ReputationToUnlock;
			if (reputationToUnlock != null)
			{
				int val7 = 0;
				foreach (KeyValuePair<BlueprintItem, int> item2 in reputationToUnlock)
				{
					Hash128 hash2 = default(Hash128);
					Hash128 val8 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2.Key);
					hash2.Append(ref val8);
					int obj2 = item2.Value;
					Hash128 val9 = UnmanagedHasher<int>.GetHash128(ref obj2);
					hash2.Append(ref val9);
					val7 ^= hash2.GetHashCode();
				}
				result.Append(ref val7);
			}
			Dictionary<BlueprintItem, long> overrideProfitFactorCosts = OverrideProfitFactorCosts;
			if (overrideProfitFactorCosts != null)
			{
				int val10 = 0;
				foreach (KeyValuePair<BlueprintItem, long> item3 in overrideProfitFactorCosts)
				{
					Hash128 hash3 = default(Hash128);
					Hash128 val11 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item3.Key);
					hash3.Append(ref val11);
					long obj3 = item3.Value;
					Hash128 val12 = UnmanagedHasher<long>.GetHash128(ref obj3);
					hash3.Append(ref val12);
					val10 ^= hash3.GetHashCode();
				}
				result.Append(ref val10);
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
			CalculateCosts(persistentTable.Loot, ref persistentTable.ReputationToUnlock, ref persistentTable.OverrideProfitFactorCosts);
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
		foreach (LootEntry item in loot.GenerateItems())
		{
			collection.Add(item.Item, item.Count);
		}
		TableData tableData = m_PersistentTables.SingleOrDefault((TableData t) => t.Table == blueprint);
		if (tableData != null)
		{
			tableData.Loot.Add(loot);
			tableData.KnownItems = GetFixedItems(tableData.Loot);
			CalculateCosts(tableData.Loot, ref tableData.ReputationToUnlock, ref tableData.OverrideProfitFactorCosts);
		}
	}

	private TableData GetTable(BlueprintSharedVendorTable blueprint)
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
				tableData.KnownItems = GetFixedItems(tableData.Loot);
				m_PersistentTables.Add(tableData);
			}
			Dictionary<BlueprintItem, int> fixedItems = GetFixedItems(tableData.Loot);
			List<LootEntry> entries = tableData.Entries;
			foreach (KeyValuePair<BlueprintItem, int> item in GetLootDifference(tableData.KnownItems, fixedItems))
			{
				UpdateLootItems(entries, item.Key, item.Value);
			}
			tableData.KnownItems = fixedItems;
			CalculateCosts(tableData.Loot, ref tableData.ReputationToUnlock, ref tableData.OverrideProfitFactorCosts);
			foreach (LootEntry item2 in entries)
			{
				if (item2.Item == null)
				{
					LogChannel.Default.Error($"UnitPartVendor.GetTable: entry.Item is null ({blueprint})");
				}
				else if (item2.Count > 0)
				{
					value.Add(item2.Item, item2.Count);
				}
			}
			m_Dictionary[blueprint] = value;
		}
		return value;
	}

	public IReadOnlyDictionary<BlueprintItem, int> GetReputationToUnlock(BlueprintSharedVendorTable blueprint)
	{
		return GetTable(blueprint).ReputationToUnlock;
	}

	public IReadOnlyDictionary<BlueprintItem, long> GetOverridePrices(BlueprintSharedVendorTable blueprint)
	{
		return GetTable(blueprint).OverrideProfitFactorCosts;
	}

	public static void CalculateCosts(IReadOnlyCollection<BlueprintUnitLoot> loot, ref Dictionary<BlueprintItem, int> reputationToUnlock, ref Dictionary<BlueprintItem, long> overrideProfitFactorCosts)
	{
		reputationToUnlock.Clear();
		overrideProfitFactorCosts.Clear();
		if (loot == null)
		{
			return;
		}
		foreach (BlueprintUnitLoot item2 in loot)
		{
			foreach (BlueprintLootComponent component in item2.GetComponents<BlueprintLootComponent>())
			{
				foreach (LootItem item3 in component.GetPossibleLoot())
				{
					BlueprintItem item = item3.Item;
					if (item != null)
					{
						reputationToUnlock[item] = Math.Max(item2.ReputationPointsToUnlock, component.ReputationPointsToUnlock);
						if (item3.ProfitFactorCostOverride.HasValue)
						{
							overrideProfitFactorCosts[item] = item3.ProfitFactorCostOverride.Value;
						}
					}
				}
			}
		}
	}

	[NotNull]
	public static Dictionary<BlueprintItem, int> GetFixedItems(List<BlueprintUnitLoot> loot)
	{
		Dictionary<BlueprintItem, int> dictionary = new Dictionary<BlueprintItem, int>();
		foreach (BlueprintUnitLoot item2 in loot)
		{
			foreach (LootItemsPackFixed component in item2.GetComponents<LootItemsPackFixed>())
			{
				BlueprintItem item = component.Item.Item;
				if (item != null)
				{
					int num = dictionary.Get(item, 0);
					dictionary[item] = num + component.Count;
				}
			}
		}
		return dictionary;
	}

	public static Dictionary<BlueprintItem, int> GetLootDifference(Dictionary<BlueprintItem, int> oldKnownItems, Dictionary<BlueprintItem, int> newKnownItems)
	{
		HashSet<BlueprintItem> hashSet = new HashSet<BlueprintItem>();
		if (oldKnownItems != null)
		{
			hashSet.AddRange(oldKnownItems.Keys);
		}
		if (newKnownItems != null)
		{
			hashSet.AddRange(newKnownItems.Keys);
		}
		Dictionary<BlueprintItem, int> dictionary = new Dictionary<BlueprintItem, int>();
		if (oldKnownItems != null && newKnownItems != null)
		{
			foreach (BlueprintItem item in hashSet)
			{
				int num = oldKnownItems.Get(item, 0);
				int num2 = newKnownItems.Get(item, 0);
				if (num != num2)
				{
					dictionary[item] = num2 - num;
				}
			}
		}
		return dictionary;
	}

	public static void UpdateLootItems(List<LootEntry> entries, BlueprintItem item, int difference)
	{
		foreach (LootEntry entry in entries)
		{
			if (difference == 0)
			{
				return;
			}
			if (entry.Item == item)
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
				Item = item,
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

using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[ClassInfoBox("If there are duplicated items from the list, we replace them with other ones. If all items already present, remove duplicates.")]
[TypeId("0cc48d89e2226fe4a9da4a2d43ae1f8f")]
public class EnsureUniqueItems : PlayerUpgraderOnlyAction
{
	private class Record
	{
		public ItemEntity Item;

		public ItemsCollection Collection;
	}

	[SerializeField]
	private BlueprintItemReference[] m_Items;

	public override string GetCaption()
	{
		return "Ensure that player has only unique items from list (in inventory and stashes)";
	}

	protected override void RunActionOverride()
	{
		BlueprintItem[] itemsBlueprints = GetItemsBlueprints(m_Items);
		List<Record>[] records = PrepareRecords(itemsBlueprints.Length);
		AddItemsFromCollection(records, itemsBlueprints, Game.Instance.Player.Inventory);
		AddItemsFromCollection(records, itemsBlueprints, Game.Instance.Player.SharedStash);
		ReplaceDuplicatesWithUniqueItems(itemsBlueprints, records);
		RemoveDuplicates(itemsBlueprints, records);
	}

	private static void ReplaceDuplicatesWithUniqueItems(BlueprintItem[] blueprints, List<Record>[] records)
	{
		for (int i = 0; i < blueprints.Length; i++)
		{
			int index;
			while (records[i].Count > 1 && TryFindEmptyIndex(records, out index))
			{
				Record record = records[i][1];
				records[i].RemoveAt(1);
				ItemEntity item = record.Item;
				ItemsCollection collection = record.Collection;
				ItemEntity itemEntity = blueprints[index].CreateEntity();
				collection.Remove(item);
				collection.Add(itemEntity);
				records[index].Add(new Record
				{
					Item = itemEntity,
					Collection = collection
				});
			}
		}
	}

	private static void RemoveDuplicates(BlueprintItem[] blueprints, List<Record>[] records)
	{
		for (int i = 0; i < blueprints.Length; i++)
		{
			if (records[i].Count >= 2)
			{
				for (int j = 1; j < records[i].Count; j++)
				{
					Record record = records[i][j];
					record.Collection.Remove(record.Item);
				}
			}
		}
	}

	private static bool TryFindEmptyIndex(List<Record>[] records, out int index)
	{
		for (int i = 0; i < records.Length; i++)
		{
			if (records[i].Count <= 0)
			{
				index = i;
				return true;
			}
		}
		index = -1;
		return false;
	}

	private static BlueprintItem[] GetItemsBlueprints(BlueprintItemReference[] blueprintReferences)
	{
		BlueprintItem[] array = new BlueprintItem[blueprintReferences.Length];
		for (int i = 0; i < blueprintReferences.Length; i++)
		{
			array[i] = blueprintReferences[i].Get();
		}
		return array;
	}

	private static List<Record>[] PrepareRecords(int length)
	{
		List<Record>[] array = new List<Record>[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = new List<Record>();
		}
		return array;
	}

	private static void AddItemsFromCollection(List<Record>[] records, BlueprintItem[] blueprints, ItemsCollection collection)
	{
		foreach (ItemEntity item in collection)
		{
			if (blueprints.Contains(item.Blueprint))
			{
				int num = blueprints.IndexOf(item.Blueprint);
				records[num].Add(new Record
				{
					Item = item,
					Collection = collection
				});
			}
		}
	}
}

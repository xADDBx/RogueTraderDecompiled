using System;
using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Blueprints.Workarounds;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Blueprints.Loot;

[TypeId("5f66ee2522e41c842be4bfe85a2a61ac")]
public class TrashLootSettings : BlueprintScriptableObject, IBlueprintScanner
{
	[Serializable]
	public class TypeSettings
	{
		public ItemsItemOrigin Type;

		public List<BlueprintItemReference> Items = new List<BlueprintItemReference>();
	}

	[Serializable]
	public class TypeChance
	{
		[Serializable]
		public class ItemData
		{
			public int Weight;

			public ItemsItemOrigin Type;

			public ItemQuality ItemQuality;
		}

		public LootSetting Setting;

		public ItemData[] Types = new ItemData[0];
	}

	[Serializable]
	public class CargoVolume
	{
		public CargoVolumeAmount CargoVolumeAmount;

		public int Percent;
	}

	private static TrashLootSettings s_MainInstance;

	public CargoVolume[] CargoVolumeTable = new CargoVolume[0];

	public List<TypeSettings> Types = new List<TypeSettings>();

	public TypeChance[] Table = new TypeChance[0];

	public static TrashLootSettings Main
	{
		get
		{
			if (!s_MainInstance)
			{
				return s_MainInstance = TrashLootSettingsInstanceHelper.GetSettingsInstance<TrashLootSettings>();
			}
			return s_MainInstance;
		}
	}

	public void Fill(BlueprintLoot target)
	{
		if (target.Type != 0)
		{
			PFLog.Default.Error($"Invalid loot type {target.Type}");
			return;
		}
		int cvp = (from i in CargoVolumeTable
			where i.CargoVolumeAmount == target.CargoVolumeAmount
			select i.Percent).FirstOrDefault();
		target.Items = RandomizeLoot(target.Setting, cvp);
	}

	public void Fill(BlueprintPointOfInterestLoot target)
	{
		int cvp = (from i in CargoVolumeTable
			where i.CargoVolumeAmount == target.CargoVolumeAmount
			select i.Percent).FirstOrDefault();
		target.FillExplorationLoot(RandomizeLoot(target.Setting, cvp));
	}

	private LootEntry[] RandomizeLoot(LootSetting setting, int cvp, [CanBeNull] Func<int, int, int> randomNumberGenerator = null)
	{
		if (cvp <= 0)
		{
			PFLog.Default.Error("CargoVolume is not configured");
			return new LootEntry[0];
		}
		int slots = 1 + cvp / 10 + ((cvp % 10 != 0) ? 1 : 0);
		return RandomizeLoot(setting, cvp, slots, randomNumberGenerator);
	}

	private LootEntry[] RandomizeLoot(LootSetting setting, int volume, int slots, [CanBeNull] Func<int, int, int> randomNumberGenerator = null)
	{
		randomNumberGenerator = randomNumberGenerator ?? new Func<int, int, int>(PFStatefulRandom.Blueprints.Range);
		TypeChance.ItemData[] array = Table.FirstItem((TypeChance i) => i.Setting == setting)?.Types;
		if (array == null)
		{
			return new LootEntry[0];
		}
		List<LootEntry> list = new List<LootEntry>();
		int num = volume;
		while (slots-- > 0)
		{
			BlueprintItem blueprintItem = null;
			int num2 = 5;
			while (blueprintItem == null && num2-- > 0)
			{
				TypeChance.ItemData itemData = Randomize(array, null, randomNumberGenerator);
				if (itemData != null && TryGetRandomItem(randomNumberGenerator, itemData, num, out var item))
				{
					blueprintItem = item;
				}
			}
			if (blueprintItem == null)
			{
				break;
			}
			LootEntry lootEntry = new LootEntry
			{
				Item = blueprintItem,
				Count = 1
			};
			list.Add(lootEntry);
			num -= blueprintItem.CargoVolumePercent;
			while ((float)num - lootEntry.CargoVolumePercent > 0f)
			{
				lootEntry.Count++;
				num -= blueprintItem.CargoVolumePercent;
			}
		}
		if (num > 0)
		{
			PFLog.Default.Error($"Small space of cargo volume left: {num}");
		}
		return list.ToArray();
	}

	private bool TryGetRandomItem(Func<int, int, int> randomNumberGenerator, TypeChance.ItemData type, int maxVolume, out BlueprintItem item)
	{
		item = null;
		List<BlueprintItemReference> list = Types.FirstItem((TypeSettings t) => t.Type == type.Type)?.Items;
		if (list == null)
		{
			PFLog.Default.Error($"Has no items for type {type}");
			return false;
		}
		item = (from i in list
			select i.Get() into it
			where it.CargoVolumePercent <= maxVolume && type.ItemQuality.HasFlag(it.GetItemQuality())
			select it).Random(PFStatefulRandom.Blueprints, randomNumberGenerator);
		return true;
	}

	private static TypeChance.ItemData Randomize(TypeChance.ItemData[] types, ItemsItemOrigin? except, [NotNull] Func<int, int, int> randomFromRange)
	{
		if (types.Length == 0)
		{
			PFLog.Default.Error("Trash loot settings are not configured");
			return null;
		}
		int arg = types.Aggregate(0, (int s, TypeChance.ItemData i) => s + i.Weight);
		int num = randomFromRange(0, arg);
		int num2 = 0;
		foreach (TypeChance.ItemData itemData in types)
		{
			num2 += itemData.Weight;
			if (num2 > num && except != itemData.Type)
			{
				return itemData;
			}
		}
		return types[^1];
	}

	[BlueprintButton(Name = "Scan trash loot items")]
	public void ScanTrashLootItems()
	{
	}

	public void Scan()
	{
	}
}

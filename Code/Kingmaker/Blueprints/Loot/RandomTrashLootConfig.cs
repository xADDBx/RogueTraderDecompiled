using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

public class RandomTrashLootConfig : ScriptableObject
{
	public List<LootItem> TrashLoot;

	public void FillLootComponent(List<LootEntry> list, float approxPrice)
	{
		while (approxPrice > 0f)
		{
			LootItem lootItem = TrashLoot.Where((LootItem i) => i.GetProfitFactorPrice() < approxPrice).Random(PFStatefulRandom.Blueprints);
			if (lootItem == null)
			{
				break;
			}
			lootItem.AddItemTo(list, 1, 0);
			approxPrice -= lootItem.GetProfitFactorPrice();
		}
	}
}

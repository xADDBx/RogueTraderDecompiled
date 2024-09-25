using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

[TypeId("a7a24a2cb7f69674cbbbbb84578ba9fa")]
public class LootRandomItem : BlueprintLootComponent
{
	[SerializeField]
	private LootItemAndWeight[] m_Items;

	public IEnumerable<LootItemAndWeight> Items => m_Items;

	public override void AddItemsTo(List<LootEntry> targetList)
	{
		int maxExclusive = m_Items.EmptyIfNull().Aggregate(0, (int acc, LootItemAndWeight i) => acc + i.Weight);
		int num = PFStatefulRandom.Blueprints.Range(0, maxExclusive);
		int num2 = 0;
		LootItemAndWeight lootItemAndWeight = null;
		LootItemAndWeight[] array = m_Items.EmptyIfNull();
		foreach (LootItemAndWeight lootItemAndWeight2 in array)
		{
			num2 += lootItemAndWeight2.Weight;
			if (num2 > num)
			{
				lootItemAndWeight = lootItemAndWeight2;
				break;
			}
		}
		lootItemAndWeight?.Item.AddItemTo(targetList, 1, base.ReputationPointsToUnlock);
	}

	public override float GetAverageProfitFactorPrice()
	{
		int num = m_Items.Sum((LootItemAndWeight i) => i.Weight);
		if (num <= 0)
		{
			return 0f;
		}
		return m_Items.Sum((LootItemAndWeight i) => i.Item.GetProfitFactorPrice() * (float)i.Weight) / (float)num;
	}

	public override IReadOnlyCollection<LootItem> GetPossibleLoot()
	{
		return m_Items.Select((LootItemAndWeight x) => x.Item).ToList();
	}
}

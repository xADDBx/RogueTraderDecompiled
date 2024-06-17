using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

[TypeId("9a9cba603f85c634690eb67962fdf792")]
public class LootItemsPackFixed : BlueprintLootComponent
{
	[SerializeField]
	private LootItem m_Item;

	[SerializeField]
	private int m_Count = 1;

	public LootItem Item => m_Item;

	public int Count => m_Count;

	public override void AddItemsTo(List<LootEntry> targetList)
	{
		m_Item.AddItemTo(targetList, m_Count, base.ReputationPointsToUnlock);
	}

	public override float GetAverageProfitFactorPrice()
	{
		return (float)m_Count * m_Item.GetProfitFactorPrice();
	}

	public override IReadOnlyCollection<LootItem> GetPossibleLoot()
	{
		return new List<LootItem> { m_Item };
	}
}

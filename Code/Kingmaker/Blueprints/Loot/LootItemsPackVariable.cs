using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

[TypeId("272ec78313893854b9338b9d34908ced")]
public class LootItemsPackVariable : BlueprintLootComponent
{
	[SerializeField]
	private LootItem m_Item;

	[SerializeField]
	private int m_CountFrom = 1;

	[SerializeField]
	private int m_CountTo = 2;

	public LootItem Item => m_Item;

	public int CountFrom => m_CountFrom;

	public int CountTo => m_CountTo;

	public override void AddItemsTo(List<LootEntry> targetList)
	{
		int count = PFStatefulRandom.Blueprints.Range(m_CountFrom, m_CountTo + 1);
		m_Item.AddItemTo(targetList, count, base.ReputationPointsToUnlock);
	}

	public override float GetAverageProfitFactorPrice()
	{
		return m_Item.GetProfitFactorPrice() * (float)(m_CountFrom + m_CountTo) / 2f;
	}

	public override IReadOnlyCollection<LootItem> GetPossibleLoot()
	{
		return new List<LootItem> { m_Item };
	}
}

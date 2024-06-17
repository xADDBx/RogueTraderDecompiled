using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Loot;

[TypeId("2fb8fe48c9208c245a98402d50657143")]
public class BlueprintUnitLoot : BlueprintScriptableObject
{
	[Serializable]
	public class Dummy
	{
	}

	[SerializeField]
	private int m_ReputationPointsToUnlock;

	[SerializeField]
	private Dummy m_Dummy;

	public int ReputationPointsToUnlock => m_ReputationPointsToUnlock;

	public void AddItemsTo(List<LootEntry> targetList)
	{
		base.ComponentsArray.OfType<BlueprintLootComponent>().ForEach(delegate(BlueprintLootComponent c)
		{
			c.AddItemsTo(targetList);
		});
	}

	public List<LootEntry> GenerateItems()
	{
		List<LootEntry> list = new List<LootEntry>();
		AddItemsTo(list);
		return list;
	}

	public float GetAverageProfitFactorPrice()
	{
		return base.ComponentsArray.OfType<BlueprintLootComponent>().Sum((BlueprintLootComponent c) => c.GetAverageProfitFactorPrice());
	}
}

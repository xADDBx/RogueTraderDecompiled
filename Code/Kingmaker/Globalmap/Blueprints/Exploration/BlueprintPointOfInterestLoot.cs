using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Enums;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[TypeId("9f5179678cb74615a98b6222787ab6ea")]
public class BlueprintPointOfInterestLoot : BlueprintPointOfInterest
{
	public CargoVolumeAmount CargoVolumeAmount = CargoVolumeAmount.FullCrate;

	public LootSetting Setting;

	[SerializeField]
	private List<LootEntry> m_ExplorationLoot = new List<LootEntry>();

	public List<LootEntry> ExplorationLoot => m_ExplorationLoot;

	public float CargoVolumePercent => ExplorationLoot.Sum((LootEntry e) => e.CargoVolumePercent);

	public void FillExplorationLoot(LootEntry[] loot)
	{
		ExplorationLoot.Clear();
		ExplorationLoot.AddRange(loot);
	}
}

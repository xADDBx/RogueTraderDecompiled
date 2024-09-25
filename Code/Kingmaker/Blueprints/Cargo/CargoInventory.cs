using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using UnityEngine;

namespace Kingmaker.Blueprints.Cargo;

[AllowedOn(typeof(BlueprintCargo))]
[TypeId("c5faca67a25d4e168d57a98ccbac5203")]
public class CargoInventory : BlueprintComponent
{
	[Serializable]
	public class Inventory
	{
		[SerializeField]
		private List<LootEntry> m_Loot;

		public IEnumerable<LootEntry> Loot => m_Loot;
	}

	[SerializeField]
	private int m_UnusableVolumePercent;

	[SerializeField]
	private Inventory m_Inventory;

	public int UnusableVolumePercent => m_UnusableVolumePercent;

	public IEnumerable<LootEntry> Loot => m_Inventory.Loot;
}

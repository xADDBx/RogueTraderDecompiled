using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawner))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("91a03409306240bea3fb42e764c1c2d4")]
public class SpawnerLootSettings : EntityPartComponent<SpawnerLootSettings.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public new SpawnerLootSettings Source => (SpawnerLootSettings)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			PartInventory inventoryOptional = unit.GetInventoryOptional();
			if (inventoryOptional == null)
			{
				return;
			}
			foreach (LootEntry item in Source.AddLoot)
			{
				inventoryOptional.Add(item.Item, item.Count);
			}
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
		}

		public void OnDispose(AbstractUnitEntity unit)
		{
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[SerializeField]
	private List<LootEntry> m_AddLoot = new List<LootEntry>();

	public IEnumerable<LootEntry> AddLoot => m_AddLoot;
}

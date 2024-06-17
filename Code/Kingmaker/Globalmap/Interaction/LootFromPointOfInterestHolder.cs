using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Loot;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

public class LootFromPointOfInterestHolder : ILootable, IHashable
{
	[JsonProperty]
	public BasePointOfInterest Point;

	[JsonProperty]
	private ItemsCollection m_Items;

	[JsonProperty]
	public StarSystemObjectEntity Owner;

	public string Name => Owner.Name;

	public string Description => null;

	public BaseUnitEntity OwnerEntity => null;

	public ItemsCollection Items => m_Items;

	public List<BlueprintCargoReference> Cargo => null;

	public Func<ItemEntity, bool> CanInsertItem => (ItemEntity _) => false;

	public LootFromPointOfInterestHolder()
	{
	}

	public LootFromPointOfInterestHolder(List<LootEntry> lootCollection, StarSystemObjectEntity entity, BasePointOfInterest pointOfInterest = null)
	{
		Point = pointOfInterest;
		Owner = entity;
		m_Items = new ItemsCollection(entity);
		foreach (LootEntry item in lootCollection)
		{
			m_Items.Add(item.Item, item.Count);
		}
	}

	public void Dispose()
	{
		m_Items.Dispose();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<BasePointOfInterest>.GetHash128(Point);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<ItemsCollection>.GetHash128(m_Items);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<StarSystemObjectEntity>.GetHash128(Owner);
		result.Append(ref val3);
		return result;
	}
}

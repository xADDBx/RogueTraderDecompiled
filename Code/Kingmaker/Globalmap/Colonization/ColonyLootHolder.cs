using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization;

public class ColonyLootHolder : ILootable, IHashable
{
	[JsonProperty]
	private ItemsCollection m_Items;

	[JsonProperty]
	private Colony m_Colony;

	[JsonProperty]
	private List<BlueprintCargoReference> m_Cargo = new List<BlueprintCargoReference>();

	public string Name => m_Colony.Blueprint.Name;

	public string Description => null;

	public BaseUnitEntity OwnerEntity => null;

	public ItemsCollection Items => m_Items;

	public List<BlueprintCargoReference> Cargo => m_Cargo;

	public Func<ItemEntity, bool> CanInsertItem => (ItemEntity _) => false;

	public ColonyLootHolder(JsonConstructorMark _)
	{
	}

	public ColonyLootHolder()
	{
	}

	public ColonyLootHolder(Colony colony)
	{
		m_Colony = colony;
		m_Items = new ItemsCollection(null);
	}

	public void AddItem(BlueprintItem item, int count)
	{
		m_Items.Add(item, count);
	}

	public void AddCargo(BlueprintCargoReference cargo)
	{
		m_Cargo.Add(cargo);
	}

	public void RewardItems()
	{
		foreach (ItemEntity item in m_Items)
		{
			if (item.Collection == null)
			{
				ItemsCollection itemsCollection = (item.Collection = m_Items);
			}
		}
		Player player = Game.Instance.Player;
		for (int num = m_Items.Items.Count - 1; num >= 0; num--)
		{
			m_Items.Transfer(m_Items.Items[num], player.Inventory);
		}
		foreach (BlueprintCargoReference item2 in m_Cargo)
		{
			player.CargoState.Create(item2);
		}
		m_Cargo.Clear();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<ItemsCollection>.GetHash128(m_Items);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<Colony>.GetHash128(m_Colony);
		result.Append(ref val2);
		List<BlueprintCargoReference> cargo = m_Cargo;
		if (cargo != null)
		{
			for (int i = 0; i < cargo.Count; i++)
			{
				Hash128 val3 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(cargo[i]);
				result.Append(ref val3);
			}
		}
		return result;
	}
}

using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Cargo;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[Serializable]
[TypeId("a7d262780a094221a6a2b49dfb152c4a")]
public class RemoveOrReplaceItem : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_RemoveItem;

	[SerializeField]
	private BlueprintItemReference m_ReplaceItem;

	[NotNull]
	public BlueprintItem RemoveItem => m_RemoveItem;

	[CanBeNull]
	public BlueprintItem ReplaceItem => m_ReplaceItem;

	public override string GetCaption()
	{
		if (ReplaceItem == null)
		{
			return $"Remove item {RemoveItem}";
		}
		return $"Replace item {RemoveItem} with {ReplaceItem}";
	}

	protected override void RunActionOverride()
	{
		int num = 0;
		foreach (UnitEntity item2 in Game.Instance.Player.AllCrossSceneUnits.OfType<UnitEntity>())
		{
			foreach (ItemSlot equipmentSlot in item2.Body.EquipmentSlots)
			{
				if (equipmentSlot.MaybeItem?.Blueprint != RemoveItem)
				{
					continue;
				}
				equipmentSlot.RemoveItem(autoMerge: true, force: true);
				if (ReplaceItem == null)
				{
					continue;
				}
				ItemEntity item = ReplaceItem.CreateEntity();
				if (equipmentSlot.IsItemSupported(item))
				{
					using (ContextData<ItemSlot.IgnoreLock>.Request())
					{
						equipmentSlot.InsertItem(item, force: true);
					}
					num++;
				}
			}
		}
		ItemsCollection inventory = Game.Instance.Player.Inventory;
		int num2 = inventory.RemoveAll(RemoveItem);
		TryAddReplacement(inventory, ReplaceItem, num2 - num);
		ItemsCollection sharedStash = Game.Instance.Player.SharedStash;
		num2 = sharedStash.RemoveAll(RemoveItem);
		TryAddReplacement(sharedStash, ReplaceItem, num2);
		foreach (CargoEntity cargoEntity in Game.Instance.Player.CargoState.CargoEntities)
		{
			num2 = cargoEntity.Inventory.Collection.RemoveAll(RemoveItem);
			TryAddReplacement(cargoEntity.Inventory.Collection, ReplaceItem, num2);
		}
	}

	private void TryAddReplacement(ItemsCollection collection, [CanBeNull] BlueprintItem replacement, int count)
	{
		if (replacement != null && count >= 1)
		{
			collection.Add(replacement, count);
		}
	}
}

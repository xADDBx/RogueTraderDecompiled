using System.Collections.Generic;
using System.Linq;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Utility.GameConst;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class LootObjectVM : VirtualListElementVMBase
{
	public readonly LootObjectType Type;

	public readonly string DisplayName;

	public readonly string Description;

	public readonly Sprite DisplayIcon;

	public CargoDropZoneVM CargoDropZoneVM;

	public InventoryDropZoneVM InventoryDropZoneVM;

	public SlotsGroupVM<ItemSlotVM> SlotsGroup;

	public IEnumerable<ItemEntity> ItemEntities => SlotsGroup.Items;

	public IEnumerable<ItemEntity> LootableItems => ItemEntities.Where((ItemEntity k) => k.IsLootable);

	public bool HasLootableItems => LootableItems.Any();

	public IEnumerable<ItemEntity> ItemsToCargo => ItemEntities.Where(CargoHelper.CanTransferToCargo);

	public bool HasItemsToCargo => ItemsToCargo.Any();

	public LootObjectVM(LootObjectType type, string displayName, string description, Sprite icon, ItemsCollection itemsCollection, IEnumerable<ItemEntity> items, LootContextVM.LootWindowMode mode)
	{
		Type = type;
		DisplayName = displayName;
		Description = description;
		DisplayIcon = icon;
		if (mode == LootContextVM.LootWindowMode.PlayerChest)
		{
			AddDisposable(SlotsGroup = new ItemSlotsGroupVM(itemsCollection, items, 6, 48, ItemsFilterType.NoFilter, ItemsSorterType.NameUp, showSlotHoldItemsInSlots: false, ItemSlotsGroupType.Loot));
		}
		else if (Game.Instance.IsControllerMouse)
		{
			AddDisposable(SlotsGroup = new ItemSlotsGroupVM(itemsCollection, items, UIConsts.MinLootSlotsInRow, UIConsts.MinLootSlotsInSingleObj, ItemsFilterType.NoFilter, ItemsSorterType.DateUp, showSlotHoldItemsInSlots: true, ItemSlotsGroupType.Loot));
		}
		else
		{
			AddDisposable(SlotsGroup = new ItemSlotsGroupVM(itemsCollection, items, UIConsts.MinLootSlotsInRow, UIConsts.MinLootSlotsInSingleObj, ItemsFilterType.NoFilter, ItemsSorterType.DateUp, showSlotHoldItemsInSlots: true, ItemSlotsGroupType.Loot));
		}
		switch (type)
		{
		case LootObjectType.Normal:
			AddDisposable(InventoryDropZoneVM = new InventoryDropZoneVM(null));
			break;
		case LootObjectType.Trash:
			AddDisposable(CargoDropZoneVM = new CargoDropZoneVM(null));
			break;
		}
	}

	protected override void DisposeImplementation()
	{
	}

	public void SetNewItems(IEnumerable<ItemEntity> items)
	{
		SlotsGroup.SetNewItems(items);
	}

	public bool ContainsSlot(ItemSlotVM slot)
	{
		return SlotsGroup.VisibleCollection.Contains(slot);
	}
}

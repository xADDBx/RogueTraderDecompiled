using System;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Augmentations;

public class AugmentationsInventoryStashVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IPartyEncumbranceHandler, ISubscriber, ISetInventorySorterHandler, IForceSortHandler, IInsertItemHandler, IUnequipItemHandler, IInventoryHandler, IDropItemHandler, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, IEquipItemAutomaticallyHandler
{
	public readonly SlotsGroupVM<ItemSlotVM> ItemSlotsGroup;

	public readonly SlotsGroupVM<InsertableLootSlotVM> InsertableSlotsGroup;

	public readonly IReadOnlyReactiveProperty<BaseUnitEntity> Unit;

	public readonly ItemsFilterVM ItemsFilter;

	public readonly EncumbranceVM EncumbranceVM;

	public readonly IReactiveProperty<long> Money = new ReactiveProperty<long>();

	public readonly InventoryDropZoneVM DropZoneVM;

	public ReactiveCommand UpdateMetallizationBuffTooltip = new ReactiveCommand();

	private ItemsCollection ItemsCollection => Game.Instance.Player.Inventory;

	public ReactiveProperty<ItemsSorterType> CurrentSorter => ItemsFilter.CurrentSorter;

	public ReactiveProperty<ItemsFilterType> CurrentFilter => ItemsFilter.CurrentFilter;

	public ItemSlotVM FirstEmptySlot => ItemSlotsGroup.VisibleCollection.FirstOrDefault((ItemSlotVM x) => !x.HasItem);

	public AugmentationsInventoryStashVM(bool inventory, Func<ItemEntity, bool> canInsertItem = null, IReadOnlyReactiveProperty<BaseUnitEntity> unit = null)
	{
		Unit = unit;
		if (canInsertItem == null)
		{
			AddDisposable(ItemSlotsGroup = new ItemSlotsGroupVM(ItemsCollection, inventory ? 6 : 9, inventory ? 120 : 81, sorter: Game.Instance.Player.UISettings.InventorySorter, filter: Game.Instance.Player.UISettings.InventoryFilter, showUnavailableItems: Game.Instance.Player.UISettings.ShowUnavailableItems, showSlotHoldItemsInSlots: false, type: ItemSlotsGroupType.Inventory));
			AddDisposable(ItemsFilter = new ItemsFilterVM(ItemSlotsGroup));
			AddDisposable(ItemSlotsGroup.CollectionChangedCommand.Subscribe(delegate
			{
				UpdateValues();
			}));
		}
		else
		{
			AddDisposable(InsertableSlotsGroup = new InsertableLootSlotsGroupVM(ItemsCollection, canInsertItem, 9, 81, sorter: Game.Instance.Player.UISettings.InventorySorter, filter: Game.Instance.Player.UISettings.InventoryFilter, showUnavailableItems: Game.Instance.Player.UISettings.ShowUnavailableItems));
			AddDisposable(ItemsFilter = new ItemsFilterVM(InsertableSlotsGroup));
			AddDisposable(InsertableSlotsGroup.CollectionChangedCommand.Subscribe(delegate
			{
				UpdateValues();
			}));
		}
		AddDisposable(EncumbranceVM = new EncumbranceVM(EncumbranceHelper.GetPartyCarryingCapacity()));
		UpdateValues();
		AddDisposable(CurrentSorter.Subscribe(delegate(ItemsSorterType value)
		{
			if (Game.Instance.Player.UISettings.InventorySorter != value)
			{
				Game.Instance.GameCommandQueue.SetInventorySorter(value);
			}
		}));
		AddDisposable(ItemsFilter.ShowUnavailable.Subscribe(delegate(bool value)
		{
			Game.Instance.Player.UISettings.ShowUnavailableItems = value;
		}));
		AddDisposable(CurrentFilter.Subscribe(delegate(ItemsFilterType value)
		{
			Game.Instance.Player.UISettings.InventoryFilter = value;
		}));
		AddDisposable(DropZoneVM = new InventoryDropZoneVM(null));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void CollectionChanged()
	{
		ItemSlotsGroup?.UpdateVisibleCollection();
		InsertableSlotsGroup?.UpdateVisibleCollection();
	}

	public void ResetFilter()
	{
		ItemsFilter.ResetFilter();
	}

	private void UpdateValues()
	{
		UpdateCapacity();
	}

	private void UpdateCapacity()
	{
		EncumbranceVM.SetCapacity(EncumbranceHelper.GetPartyCarryingCapacity());
	}

	public void ChangePartyEncumbrance(Encumbrance prevEncumbrance)
	{
		UpdateCapacity();
	}

	void ISetInventorySorterHandler.HandleSetInventorySorter(ItemsSorterType sorterType)
	{
		if (CurrentSorter.Value != sorterType)
		{
			CurrentSorter.Value = sorterType;
		}
	}

	public void HandleForceSort()
	{
		ItemSlotsGroup.SortItems();
	}

	public void HandleInsertItem(ItemSlot slot)
	{
		CollectionChanged();
	}

	public void HandleUnequipItem()
	{
		CollectionChanged();
	}

	public void Refresh()
	{
		CollectionChanged();
		foreach (ItemSlotVM item in ItemSlotsGroup.VisibleCollection)
		{
			item.UpdateTooltips(force: true);
		}
	}

	public void TryEquip(ItemSlotVM slot)
	{
	}

	public void TryDrop(ItemSlotVM slot)
	{
	}

	public void TryMoveToCargo(ItemSlotVM slot, bool immediately = false)
	{
	}

	public void TryMoveToInventory(ItemSlotVM slot, bool immediately = false)
	{
	}

	public void HandleDropItem(ItemEntity item, bool isSplit)
	{
		Refresh();
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		Refresh();
	}

	public void HandleEquipItemAutomatically(ItemEntity item)
	{
		Refresh();
	}
}

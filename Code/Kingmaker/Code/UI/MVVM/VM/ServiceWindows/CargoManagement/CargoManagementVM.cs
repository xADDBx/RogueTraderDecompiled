using System;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement;

public class CargoManagementVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INewSlotsHandler, ISubscriber, IMoveItemHandler, IInventoryHandler
{
	public ReactiveProperty<BaseUnitEntity> Unit;

	public readonly ShipNameAndPortraitVM ShipNameAndPortraitVM;

	public readonly InventoryStashVM StashVM;

	public readonly InventoryCargoVM InventoryCargoVM;

	public CargoManagementVM()
	{
		Unit = Game.Instance.SelectionCharacter.SelectedUnitInUI;
		AddDisposable(Unit.Subscribe(delegate
		{
			OnUnitChanged();
		}));
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ShipNameAndPortraitVM = new ShipNameAndPortraitVM(Unit));
		AddDisposable(StashVM = new InventoryStashVM(inventory: true));
		AddDisposable(InventoryCargoVM = new InventoryCargoVM(InventoryCargoViewType.Inventory));
	}

	protected override void DisposeImplementation()
	{
	}

	private void OnUnitChanged()
	{
		if (Unit.Value != null)
		{
			StashVM?.CollectionChanged();
		}
	}

	void INewSlotsHandler.HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
	}

	void INewSlotsHandler.HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
		InventoryHelper.TryMoveSlotInInventory(from, to);
	}

	void INewSlotsHandler.HandleTrySplitSlot(ItemSlotVM slot)
	{
		InventoryHelper.TrySplitSlot(slot, isLoot: false);
	}

	void IMoveItemHandler.HandleMoveItem(bool isEquip)
	{
		StashVM.CollectionChanged();
	}

	void IInventoryHandler.Refresh()
	{
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
	}

	void IInventoryHandler.TryDrop(ItemSlotVM slot)
	{
	}

	void IInventoryHandler.TryMoveToCargo(ItemSlotVM slot, bool immediately)
	{
	}

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM slot, bool immediately)
	{
		InventoryHelper.TryMoveSlotInInventory(slot, StashVM.FirstEmptySlot);
	}
}

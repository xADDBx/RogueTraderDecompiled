using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INewSlotsHandler, ISubscriber, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, IVendorDealHandler, IMoveItemHandler, IInventoryHandler, ICargoStateChangedHandler
{
	public enum VendorViewType
	{
		BuyView,
		SellView
	}

	public readonly ReactiveProperty<VendorWindowsTab> ActiveTab = new ReactiveProperty<VendorWindowsTab>();

	public readonly InventoryStashVM StashVM;

	public readonly InventoryCargoVM InventoryCargoVM;

	public readonly LensSelectorVM Selector;

	public VendorTabNavigationVM VendorTabNavigationVM;

	public VendorTradePartVM VendorTradePartVM;

	public VendorReputationPartVM VendorReputationPartVM;

	public ReactiveProperty<bool> HasItemsToBuy = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<VendorViewType> MiddleView = new ReactiveProperty<VendorViewType>(VendorViewType.BuyView);

	public ReactiveCommand CargoCollectionChange = new ReactiveCommand();

	private VendorLogic Vendor => VendorHelper.Vendor;

	public VendorVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Vendor);
		});
		AddDisposable(StashVM = new InventoryStashVM(inventory: true));
		AddDisposable(InventoryCargoVM = new InventoryCargoVM(InventoryCargoViewType.Vendor, null, null, fromPointOfInterest: false, fromVendor: true));
		AddDisposable(VendorTabNavigationVM = new VendorTabNavigationVM());
		AddDisposable(VendorTabNavigationVM.ActiveTab.Subscribe(delegate(VendorWindowsTab val)
		{
			SelectWindow(val);
		}));
		VendorTabNavigationVM.NeedHideReputationCompletely.Value = Vendor.NeedHideReputationCompletely;
		AddDisposable(Selector = new LensSelectorVM());
		TryCreateDropZone();
	}

	protected override void DisposeImplementation()
	{
		VendorTradePartVM?.Dispose();
		VendorReputationPartVM?.Dispose();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Vendor);
		});
	}

	private void SelectWindow(VendorWindowsTab tab)
	{
		switch (tab)
		{
		case VendorWindowsTab.Trade:
			VendorTradePartVM?.Dispose();
			VendorReputationPartVM?.Dispose();
			VendorTradePartVM = new VendorTradePartVM();
			CheckHasItemsToBuy();
			break;
		case VendorWindowsTab.Reputation:
			VendorTradePartVM?.Dispose();
			VendorReputationPartVM?.Dispose();
			VendorReputationPartVM = new VendorReputationPartVM(InventoryCargoVM);
			CheckHasItemsToBuy();
			break;
		}
		ActiveTab.Value = tab;
	}

	private void TryCreateDropZone()
	{
		InventoryCargoVM.TryCreateDropZone();
	}

	private void UpdatePlayerSide()
	{
		StashVM.CollectionChanged();
	}

	public void Close()
	{
		if ((Game.Instance.LoadedAreaState?.Settings.CapitalPartyMode).Value && !UINetUtility.IsControlMainCharacterWithWarning())
		{
			return;
		}
		if (Vendor.IsChanged)
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.Vendor.BeforeClose, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				if (button == DialogMessageBoxBase.BoxButton.Yes)
				{
					Game.Instance.GameCommandQueue.EndTrading();
				}
			});
		}
		else
		{
			Game.Instance.GameCommandQueue.EndTrading();
		}
	}

	void INewSlotsHandler.HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
	}

	void INewSlotsHandler.HandleTrySplitSlot(ItemSlotVM slot)
	{
		InventoryHelper.TrySplitSlot(slot, isLoot: false);
	}

	void INewSlotsHandler.HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
		if (from.IsInVendor)
		{
			from.VendorTryMove(split: false);
		}
		else
		{
			InventoryHelper.TryMoveSlotInInventory(from, to);
		}
	}

	void IVendorDealHandler.HandleVendorDeal()
	{
		UpdatePlayerSide();
	}

	void IVendorDealHandler.HandleCancelVendorDeal()
	{
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

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM from, bool immediately)
	{
		InventoryHelper.TryMoveSlotInInventory(from, StashVM.FirstEmptySlot);
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		UpdatePlayerSide();
	}

	public void SetMiddlePart(VendorViewType screen)
	{
		MiddleView.Value = screen;
	}

	void IMoveItemHandler.HandleMoveItem(bool isEquip)
	{
		StashVM.CollectionChanged();
	}

	public void HandleCreateNewCargo(CargoEntity entity)
	{
		if (ActiveTab.Value == VendorWindowsTab.Reputation)
		{
			CargoCollectionChange?.Execute();
		}
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(UIStrings.Instance.CargoTexts.CargoCreated.Text, addToLog: false, WarningNotificationFormat.Attention);
		});
	}

	public void HandleRemoveCargo(CargoEntity entity, bool fromMassSell)
	{
		if (ActiveTab.Value == VendorWindowsTab.Reputation)
		{
			CargoCollectionChange?.Execute();
		}
	}

	public void HandleAddItemToCargo(ItemEntity item, ItemsCollection from, CargoEntity to, int oldIndex)
	{
	}

	public void HandleRemoveItemFromCargo(ItemEntity item, CargoEntity from)
	{
	}

	public void BuyAllAvailable()
	{
		VendorHelper.VendorTryBuyAllAvailable(CheckHasItemsToBuy);
	}

	public void CheckHasItemsToBuy()
	{
		HasItemsToBuy.Value = VendorHelper.HasItemsToBuy();
	}
}

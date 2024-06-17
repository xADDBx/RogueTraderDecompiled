using System;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public class InventoryStashVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IPartyEncumbranceHandler, ISubscriber, ISetInventorySorterHandler
{
	public readonly SlotsGroupVM<ItemSlotVM> ItemSlotsGroup;

	public readonly SlotsGroupVM<InsertableLootSlotVM> InsertableSlotsGroup;

	public readonly ItemsFilterVM ItemsFilter;

	public readonly EncumbranceVM EncumbranceVM;

	public IReactiveProperty<long> Money = new ReactiveProperty<long>();

	public InventoryDropZoneVM DropZoneVM;

	private ItemsCollection ItemsCollection => Game.Instance.Player.Inventory;

	public ReactiveProperty<ItemsSorterType> CurrentSorter => ItemsFilter.CurrentSorter;

	public ReactiveProperty<ItemsFilterType> CurrentFilter => ItemsFilter.CurrentFilter;

	public ItemSlotVM FirstEmptySlot => ItemSlotsGroup.VisibleCollection.FirstOrDefault((ItemSlotVM x) => !x.HasItem);

	public InventoryStashVM(bool inventory, Func<ItemEntity, bool> canInsertItem = null)
	{
		if (canInsertItem == null)
		{
			AddDisposable(ItemSlotsGroup = new ItemSlotsGroupVM(ItemsCollection, inventory ? 6 : 9, inventory ? 120 : 81, sorter: Game.Instance.Player.UISettings.InventorySorter, filter: Game.Instance.Player.UISettings.InventoryFilter, showSlotHoldItemsInSlots: false, type: ItemSlotsGroupType.Inventory));
			AddDisposable(ItemsFilter = new ItemsFilterVM(ItemSlotsGroup));
			AddDisposable(ItemSlotsGroup.CollectionChangedCommand.Subscribe(delegate
			{
				UpdateValues();
			}));
		}
		else
		{
			AddDisposable(InsertableSlotsGroup = new InsertableLootSlotsGroupVM(ItemsCollection, canInsertItem, 9, 81, sorter: Game.Instance.Player.UISettings.InventorySorter, filter: Game.Instance.Player.UISettings.InventoryFilter));
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
		AddDisposable(CurrentFilter.Subscribe(delegate(ItemsFilterType value)
		{
			Game.Instance.Player.UISettings.InventoryFilter = value;
		}));
		AddDisposable(DropZoneVM = new InventoryDropZoneVM(null));
		AddDisposable(EventBus.Subscribe(this));
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
		UpdateGoldCoins();
	}

	private void UpdateCapacity()
	{
		EncumbranceVM.SetCapacity(EncumbranceHelper.GetPartyCarryingCapacity());
	}

	private void UpdateGoldCoins()
	{
		Money.Value = Game.Instance.Player.Money;
	}

	public void ChangePartyEncumbrance(Encumbrance prevEncumbrance)
	{
		UpdateCapacity();
	}

	protected override void DisposeImplementation()
	{
	}

	void ISetInventorySorterHandler.HandleSetInventorySorter(ItemsSorterType sorterType)
	{
		if (CurrentSorter.Value != sorterType)
		{
			CurrentSorter.Value = sorterType;
		}
	}

	public void OnWeaponSetChanged()
	{
		ItemSlotsGroup.VisibleCollection.ForEach(delegate(ItemSlotVM s)
		{
			s.UpdateTooltips(force: true);
		});
	}
}

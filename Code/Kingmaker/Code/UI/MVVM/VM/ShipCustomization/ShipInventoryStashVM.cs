using System;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using Warhammer.SpaceCombat;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Kingmaker.Code.UI.MVVM.VM.ShipCustomization;

public class ShipInventoryStashVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISetInventorySorterHandler, ISubscriber, IInsertItemHandler, IUnequipItemHandler, IUpgradeSystemComponentHandler, ISubscriber<StarshipEntity>, IScrapChangedHandler, IInventoryHandler, IDropItemHandler, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, IEquipItemAutomaticallyHandler
{
	public readonly SlotsGroupVM<ItemSlotVM> ItemSlotsGroup;

	public readonly SlotsGroupVM<InsertableLootSlotVM> InsertableSlotsGroup;

	public readonly ItemsFilterVM ItemsFilter;

	public readonly EncumbranceVM EncumbranceVM;

	public IReactiveProperty<long> Scrap = new ReactiveProperty<long>();

	private ItemsCollection ItemsCollection => Game.Instance.Player.PlayerShip.Inventory.Collection;

	public ReactiveProperty<ItemsSorterType> CurrentSorter => ItemsFilter.CurrentSorter;

	public ReactiveProperty<ItemsFilterType> CurrentFilter => ItemsFilter.CurrentFilter;

	public ItemSlotVM FirstEmptySlot => ItemSlotsGroup.VisibleCollection.FirstOrDefault((ItemSlotVM x) => !x.HasItem);

	public TooltipBaseTemplate EncumbranceTooltip => new TooltipTemplateEncumbranceParty();

	public ShipInventoryStashVM(bool inventory, Func<ItemEntity, bool> canInsertItem = null)
	{
		AddDisposable(EncumbranceVM = new EncumbranceVM(EncumbranceHelper.GetPartyCarryingCapacity()));
		if (canInsertItem == null)
		{
			AddDisposable(ItemSlotsGroup = new ItemSlotsGroupVM(ItemsCollection, inventory ? 6 : 9, inventory ? 120 : 81, ItemsFilterType.ShipNoFilter, Game.Instance.Player.UISettings.InventorySorter, showUnavailableItems: true, showSlotHoldItemsInSlots: false, ItemSlotsGroupType.Inventory));
			AddDisposable(ItemsFilter = new ItemsFilterVM(ItemSlotsGroup));
			AddDisposable(ItemSlotsGroup.CollectionChangedCommand.Subscribe(delegate
			{
				UpdateValues();
			}));
		}
		else
		{
			AddDisposable(InsertableSlotsGroup = new InsertableLootSlotsGroupVM(ItemsCollection, canInsertItem, 9, 81, ItemsFilterType.ShipNoFilter, Game.Instance.Player.UISettings.InventorySorter));
			AddDisposable(ItemsFilter = new ItemsFilterVM(InsertableSlotsGroup));
			AddDisposable(InsertableSlotsGroup.CollectionChangedCommand.Subscribe(delegate
			{
				UpdateValues();
			}));
		}
		UpdateValues();
		AddDisposable(CurrentSorter.Subscribe(delegate(ItemsSorterType value)
		{
			if (Game.Instance.Player.UISettings.InventorySorter != value)
			{
				Game.Instance.GameCommandQueue.SetInventorySorter(value);
			}
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	public void CollectionChanged()
	{
		ItemSlotsGroup?.UpdateVisibleCollection();
		InsertableSlotsGroup?.UpdateVisibleCollection();
	}

	private void UpdateValues()
	{
		UpdateCapacity();
		UpdateScrap();
	}

	private void UpdateCapacity()
	{
		EncumbranceVM.SetCapacity(EncumbranceHelper.GetPartyCarryingCapacity());
	}

	private void UpdateScrap()
	{
		Scrap.Value = (int)Game.Instance.Player.Scrap;
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

	void IInsertItemHandler.HandleInsertItem(ItemSlot slot)
	{
		CollectionChanged();
	}

	void IUnequipItemHandler.HandleUnequipItem()
	{
		CollectionChanged();
	}

	void IUpgradeSystemComponentHandler.HandleSystemComponentUpgrade(SystemComponent.SystemComponentType componentType, SystemComponent.UpgradeResult result)
	{
		UpdateValues();
	}

	void IUpgradeSystemComponentHandler.HandleSystemComponentDowngrade(SystemComponent.SystemComponentType componentType, SystemComponent.DowngradeResult result)
	{
		UpdateValues();
	}

	void IScrapChangedHandler.HandleScrapGained(int scrap)
	{
	}

	void IScrapChangedHandler.HandleScrapSpend(int scrap)
	{
		UpdateValues();
	}

	public void Refresh()
	{
		CollectionChanged();
		foreach (ItemSlotVM item in ItemSlotsGroup.VisibleCollection)
		{
			item.UpdateTooltips(force: true);
		}
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
		InventoryHelper.TryEquip(slot, Game.Instance.Player.PlayerShip);
	}

	void IInventoryHandler.TryDrop(ItemSlotVM slot)
	{
		InventoryHelper.TryDrop(slot);
	}

	void IInventoryHandler.TryMoveToCargo(ItemSlotVM slot, bool immediately)
	{
		InventoryHelper.TryMoveToCargo(slot);
	}

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM slot, bool immediately)
	{
	}

	void IDropItemHandler.HandleDropItem(ItemEntity item, bool split)
	{
		Refresh();
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		Refresh();
	}

	void IEquipItemAutomaticallyHandler.HandleEquipItemAutomatically(ItemEntity item)
	{
		Refresh();
	}
}

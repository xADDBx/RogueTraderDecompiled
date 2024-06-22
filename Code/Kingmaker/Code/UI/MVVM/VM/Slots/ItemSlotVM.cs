using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public class ItemSlotVM : VirtualListElementVMBase, ICargoStateChangedHandler, ISubscriber, IInventoryChangedHandler, ISplitItemHandler, IEquipItemHandler, ISubscriber<IItemEntity>, IInventorySlotHoverHandler, ISubscriber<ItemEntity>, IInventorySlotPossibleTarget, IToCargoAutomaticallyChangedHandler
{
	public readonly ReactiveProperty<ItemEntity> Item = new ReactiveProperty<ItemEntity>(null);

	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>(null);

	public readonly ReactiveProperty<int> Count = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> UsableCount = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<double> CurrentCostPF = new ReactiveProperty<double>(0.0);

	public readonly ReactiveProperty<double> PriceWithoutDiscountPF = new ReactiveProperty<double>(0.0);

	public readonly ReactiveProperty<bool> IsNotable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> CanUse = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsTrash = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsUsable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<string> TypeName = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> DisplayName = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<float> Weight = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<ItemGrade> ItemGrade = new ReactiveProperty<ItemGrade>(Kingmaker.Code.UI.MVVM.VM.Slots.ItemGrade.Common);

	public readonly ReactiveProperty<ItemStatus> ItemStatus = new ReactiveProperty<ItemStatus>(Kingmaker.Code.UI.MVVM.VM.Slots.ItemStatus.None);

	public readonly ReactiveProperty<bool> PossibleTarget = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<List<TooltipBaseTemplate>> Tooltip = new ReactiveProperty<List<TooltipBaseTemplate>>();

	public readonly ReactiveProperty<List<ContextMenuCollectionEntity>> ContextMenu = new ReactiveProperty<List<ContextMenuCollectionEntity>>();

	public readonly ReactiveCommand ToCargoAutomaticallyChange = new ReactiveCommand();

	public readonly ReactiveCommand<ItemEntity> ItemChanged = new ReactiveCommand<ItemEntity>();

	private IDisposable m_UpdateDispatcher;

	private ItemEntity m_LastItem;

	private readonly bool m_CompareEnabled;

	protected bool IsPossibleHighlighted;

	protected bool IsPossibleHovered;

	public readonly ReactiveCommand NeedBlink = new ReactiveCommand();

	public int Index;

	public ItemEntity ItemEntity => Item.Value;

	public ItemEntityWeapon ItemWeapon => ItemEntity as ItemEntityWeapon;

	public ISlotsGroupVM Group { get; }

	public ItemSlotsGroupType SlotsGroupType => Group?.Type ?? ItemSlotsGroupType.Unknown;

	public ItemsCollection ParentCollection => Item.Value?.Collection;

	public bool HasItem => Item.Value != null;

	public bool CanEquip
	{
		get
		{
			if (HasItem && UIUtilityItem.CanEquipItem(Item.Value))
			{
				return IsInStash;
			}
			return false;
		}
	}

	public bool IsEquipPossible
	{
		get
		{
			if (HasItem && UIUtilityItem.IsEquipPossible(Item.Value))
			{
				return IsInStash;
			}
			return false;
		}
	}

	public bool IsPosibleSplit
	{
		get
		{
			if (HasItem)
			{
				return Item.Value.Count > 1;
			}
			return false;
		}
	}

	public bool IsInStash => ParentCollection == Game.Instance.Player.Inventory;

	public bool IsInVendor => ParentCollection.IsVendorTable;

	public bool IsLockedByRep => Game.Instance.Vendor.VendorInventory.IsLockedByReputation(ItemEntity);

	public bool IsLockedByCost => Game.Instance.Vendor.GetItemBuyPrice(ItemEntity) > Game.Instance.Player.ProfitFactor.Total;

	public bool HasDiscount
	{
		get
		{
			int value;
			if (Game.Instance.Vendor.VendorEntity != null)
			{
				return Game.Instance.Player.ProfitFactor.VendorDiscounts.TryGetValue(Game.Instance.Vendor.VendorFaction.FactionType, out value);
			}
			return false;
		}
	}

	public bool CanTransferToCargo => CargoHelper.CanTransferToCargo(Item?.Value);

	public bool CanTransferToInventory => CanTransferFromCargo(Item?.Value);

	public virtual bool CanTransfer
	{
		get
		{
			if ((!IsInStash || !CanTransferToCargo) && (SlotsGroupType != ItemSlotsGroupType.Cargo || !CanTransferToInventory))
			{
				if (SlotsGroupType == ItemSlotsGroupType.Loot)
				{
					if (CanTransferToInventory)
					{
						return CanTransferToCargo;
					}
					return false;
				}
				return false;
			}
			return true;
		}
	}

	protected ItemSlotVM()
	{
		AddDisposable(Item.Subscribe(ItemChangedHandler));
		AddDisposable(Game.Instance.SelectionCharacter.SelectedUnitInUI.Subscribe(delegate(BaseUnitEntity unit)
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				OnUnitChanged(unit);
			}, 1);
		}));
		AddDisposable(CanUse.CombineLatest(IsTrash, (bool canUse, bool isTrash) => new { canUse, isTrash }).Subscribe(value =>
		{
			if (!value.canUse)
			{
				ItemStatus.Value = Kingmaker.Code.UI.MVVM.VM.Slots.ItemStatus.Unsuitable;
			}
			else if (value.isTrash)
			{
				ItemStatus.Value = Kingmaker.Code.UI.MVVM.VM.Slots.ItemStatus.Uncollectable;
			}
			else
			{
				ItemStatus.Value = Kingmaker.Code.UI.MVVM.VM.Slots.ItemStatus.None;
			}
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	public ItemSlotVM(ItemEntity item, int index, ISlotsGroupVM group = null, bool compareEnabled = true)
		: this()
	{
		m_LastItem = item;
		m_CompareEnabled = compareEnabled;
		Item.Value = item;
		Index = index;
		Group = group;
	}

	protected override void DisposeImplementation()
	{
		Item.Value = null;
		m_UpdateDispatcher?.Dispose();
	}

	private void OnUnitChanged(BaseUnitEntity unit)
	{
		CanUse.Value = GetCanUse();
		IsUsable.Value = GetIsUsable();
		UpdateTooltips(force: true);
	}

	protected virtual void ItemChangedHandler(ItemEntity item)
	{
		Icon.Value = GetIcon();
		CurrentCostPF.Value = ((item != null) ? Game.Instance.Vendor.GetItemBuyPrice(item) : 0f);
		if (HasDiscount)
		{
			PriceWithoutDiscountPF.Value = ((item != null) ? Game.Instance.Vendor.GetItemBaseBuyPrice(ItemEntity) : 0f);
		}
		IsNotable.Value = item?.Blueprint.IsNotable ?? false;
		Count.Value = item?.Count ?? 0;
		UsableCount.Value = ((item != null && !(item.Blueprint is BlueprintStarshipItem)) ? item.Charges : 0);
		ItemGrade.Value = GetItemGrade();
		CanUse.Value = GetCanUse();
		IsUsable.Value = GetIsUsable();
		IsTrash.Value = item != null && CargoHelper.IsTrashItem(item);
		TypeName.Value = ((item != null) ? UIUtilityItem.GetItemType(item) : string.Empty);
		DisplayName.Value = item?.Name ?? string.Empty;
		Weight.Value = item?.Blueprint.Weight ?? 0f;
		Tooltip.Value = GetTooltips();
		if (m_LastItem != item)
		{
			ItemChanged.Execute(m_LastItem);
			m_LastItem = item;
		}
		if (item != null)
		{
			m_UpdateDispatcher?.Dispose();
			AddDisposable(m_UpdateDispatcher = ObservableExtensions.Subscribe(MainThreadDispatcher.FrequentUpdateAsObservable(), delegate
			{
				if (!LoadingProcess.Instance.IsLoadingInProcess)
				{
					Count.Value = GetItemsCount();
				}
			}));
		}
		else
		{
			m_UpdateDispatcher?.Dispose();
		}
	}

	public void UpdateTooltips(bool force = false)
	{
		Tooltip.Value = GetTooltips(force);
	}

	private List<TooltipBaseTemplate> GetTooltips(bool force = false)
	{
		List<TooltipBaseTemplate> list = new List<TooltipBaseTemplate>();
		if (ItemEntity == null)
		{
			return list;
		}
		if (m_CompareEnabled && GetType() == typeof(ItemSlotVM) && !IsUsable.Value)
		{
			List<ItemSlot> list2 = ((ItemEntity.Origin != ItemsItemOrigin.ShipComponents) ? Game.Instance.SelectionCharacter.SelectedUnitInUI.Value?.Body.EquipmentSlots.Where(IsSlotAllowedToCompare).EmptyIfNull().ToList() : Game.Instance.Player.PlayerShip?.Hull.HullSlots.EquipmentSlots.Where(IsSlotAllowedToCompare).EmptyIfNull().ToList());
			if (list2 != null && list2.Count > 2)
			{
				list2 = list2.Where((ItemSlot i) => i.Active).ToList();
			}
			List<TooltipTemplateItem> list3 = new List<TooltipTemplateItem>();
			if (list2 != null && list2.Count >= 4)
			{
				for (int j = 0; j < list2.Count - 1; j += 2)
				{
					List<ItemEntity> fewItems = new List<ItemEntity>
					{
						list2[j]?.Item,
						list2[j + 1]?.Item
					};
					list3.Add(new TooltipTemplateItem(null, ItemEntity, force, replenishing: false, fewItems));
				}
				if (list2.Count % 2 != 0)
				{
					ItemEntity item = list2.Last()?.Item;
					list3.Add(new TooltipTemplateItem(item, ItemEntity, force));
				}
			}
			else
			{
				list3 = list2?.Select((ItemSlot i) => new TooltipTemplateItem(i?.Item, ItemEntity, force)).ToList();
			}
			if (list3 != null)
			{
				list.AddRange(list3);
			}
		}
		list.Add(new TooltipTemplateItem(ItemEntity, null, force));
		return list;
	}

	private bool IsSlotAllowedToCompare(ItemSlot slot)
	{
		if (!slot.HasItem || !slot.IsItemSupported(ItemEntity) || ItemEntity is ItemEntityUsable)
		{
			return false;
		}
		if (slot.Item is ItemEntityWeapon itemEntityWeapon && ItemEntity is ItemEntityWeapon itemEntityWeapon2)
		{
			return itemEntityWeapon.Blueprint.IsRanged == itemEntityWeapon2.Blueprint.IsRanged;
		}
		return true;
	}

	protected virtual Sprite GetIcon()
	{
		if (!HasItem)
		{
			return null;
		}
		return ItemEntity.Icon.Or(Game.Instance.BlueprintRoot.UIConfig.UIIcons.DefaultItemIcon);
	}

	private bool GetCanUse()
	{
		if (ItemEntity != null)
		{
			_ = ItemEntity.Blueprint.IsNotable;
		}
		else
			_ = 0;
		if (ItemEntity != null)
		{
			if (!UIUtilityItem.GetEquipPosibility(ItemEntity)[0] && ItemEntity.Blueprint.ItemType != ItemsItemType.Other && ItemEntity.Blueprint.ItemType != ItemsItemType.NonUsable && ItemEntity.Blueprint.ItemType != ItemsItemType.ColonyFoundation)
			{
				return ItemEntity.Blueprint.ItemType == ItemsItemType.ResourceMiner;
			}
			return true;
		}
		return false;
	}

	private bool GetIsUsable()
	{
		if (HasItem && Item.Value.IsUsableFromInventory)
		{
			return Item.Value.GetBestAvailableUser() != null;
		}
		return false;
	}

	protected virtual int GetItemsCount()
	{
		return Item.Value?.Count ?? 0;
	}

	private ItemGrade GetItemGrade()
	{
		if (ItemEntity == null)
		{
			return Kingmaker.Code.UI.MVVM.VM.Slots.ItemGrade.Common;
		}
		if (ItemEntity.Blueprint.IsNotable)
		{
			return Kingmaker.Code.UI.MVVM.VM.Slots.ItemGrade.Quest;
		}
		return ItemEntity.Blueprint.Rarity switch
		{
			BlueprintItem.ItemRarity.Trash => Kingmaker.Code.UI.MVVM.VM.Slots.ItemGrade.Trash, 
			BlueprintItem.ItemRarity.Lore => Kingmaker.Code.UI.MVVM.VM.Slots.ItemGrade.Lore, 
			BlueprintItem.ItemRarity.Pattern => Kingmaker.Code.UI.MVVM.VM.Slots.ItemGrade.Pattern, 
			BlueprintItem.ItemRarity.Quest => Kingmaker.Code.UI.MVVM.VM.Slots.ItemGrade.Quest, 
			BlueprintItem.ItemRarity.Unique => Kingmaker.Code.UI.MVVM.VM.Slots.ItemGrade.Unique, 
			_ => Kingmaker.Code.UI.MVVM.VM.Slots.ItemGrade.Common, 
		};
	}

	private void RemoveItem()
	{
		Item.Value = null;
	}

	private bool CanTransferFromCargo(ItemEntity item)
	{
		if (CargoHelper.IsItemInCargo(item))
		{
			return CargoHelper.CanTransferFromCargo(item);
		}
		if (item != null)
		{
			return !CargoHelper.IsTrashItem(item);
		}
		return false;
	}

	private void UpdateItem()
	{
		Item.SetValueAndForceNotify(Item.Value);
	}

	public void SetItem(ItemEntity item)
	{
		Item.Value = item;
		item?.SetSlotIndex(Index);
	}

	public void VendorTryMove(bool split)
	{
		if (CanBuy())
		{
			VendorHelper.TryMove(ItemEntity, ParentCollection, split);
		}
	}

	public void AddToCargoAutomatically()
	{
		ItemEntity.SetToCargoAutomatically(!ItemEntity.ToCargoAutomatically);
	}

	void IToCargoAutomaticallyChangedHandler.HandleToCargoAutomaticallyChanged(BlueprintItem blueprintItem)
	{
		ToCargoAutomaticallyChange?.Execute();
	}

	public void VendorTryBuyAll()
	{
		if (CanBuy())
		{
			Game.Instance.GameCommandQueue.AddForBuyVendor(ItemEntity, ItemEntity.Count, makeDeal: true);
		}
	}

	public void Blink()
	{
		NeedBlink?.Execute();
	}

	private bool CanBuy()
	{
		if (!HasItem)
		{
			return false;
		}
		if (IsLockedByRep)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.Vendor.CantBuyItem.Text + ". " + UIStrings.Instance.Vendor.NotEnoughReputation.Text, addToLog: false, WarningNotificationFormat.Attention);
			});
			return false;
		}
		if (IsLockedByCost)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.Vendor.CantBuyItem.Text + ". " + UIStrings.Instance.Vendor.NotEnoughProfitFactor.Text, addToLog: false, WarningNotificationFormat.Attention);
			});
			return false;
		}
		return true;
	}

	public void ShowInfo()
	{
		TooltipHelper.ShowInfo(Tooltip.Value?.LastItem());
	}

	void ICargoStateChangedHandler.HandleCreateNewCargo(CargoEntity entity)
	{
	}

	void ICargoStateChangedHandler.HandleRemoveCargo(CargoEntity entity, bool fromMassSell)
	{
	}

	void ICargoStateChangedHandler.HandleAddItemToCargo(ItemEntity item, ItemsCollection from, CargoEntity to, int oldIndex)
	{
		if (Index == oldIndex && Group?.MechanicCollection == from && Item.Value == item)
		{
			RemoveItem();
			UpdateItem();
		}
	}

	void ICargoStateChangedHandler.HandleRemoveItemFromCargo(ItemEntity item, CargoEntity cargoEntity)
	{
	}

	void IInventoryChangedHandler.HandleSetItem(ItemEntity item, int oldItemIndex)
	{
		if (item.InventorySlotIndex == Index && item.Collection == Group?.MechanicCollection)
		{
			Item.Value = item;
			UpdateItem();
		}
		if (item.InventorySlotIndex != oldItemIndex && oldItemIndex == Index && item.Collection == Group?.MechanicCollection)
		{
			Item.Value = null;
			UpdateItem();
		}
	}

	void ISplitItemHandler.HandleBeforeSplitItem(ItemEntity item, ItemsCollection from, ItemsCollection to)
	{
		if (ItemEntity == item && from == to && Group != null)
		{
			Group.SorterType.Value = ItemsSorterType.NotSorted;
		}
	}

	void ISplitItemHandler.HandleAfterSplitItem(ItemEntity item)
	{
		if (ItemEntity == item)
		{
			Group?.UpdateVisibleCollection();
		}
	}

	void ISplitItemHandler.HandleSplitItem()
	{
	}

	void IInventoryChangedHandler.HandleUpdateItem(ItemEntity item, ItemsCollection collection, int index)
	{
		if (Index == index && Group?.MechanicCollection == collection)
		{
			Item.Value = item;
			UpdateItem();
		}
	}

	void IInventoryChangedHandler.HandleRemoveItem(ItemEntity item, ItemsCollection collection, int oldIndex)
	{
		if (Index == oldIndex && Item.Value == item && Group?.MechanicCollection == collection)
		{
			RemoveItem();
		}
	}

	void IEquipItemHandler.OnDidEquipped()
	{
		HandleItemEquip();
	}

	void IEquipItemHandler.OnWillUnequip()
	{
		HandleItemEquip();
	}

	private void HandleItemEquip()
	{
		if (EventInvokerExtensions.GetEntity<ItemEntity>() == ItemEntity)
		{
			UpdateTooltips(force: true);
		}
	}

	public void HandleHighlightStart(ItemSlot slot)
	{
		IsPossibleHighlighted = IsPossibleTarget(slot);
		UpdatePossibleTarget();
	}

	public void HandleHighlightStop()
	{
		IsPossibleHighlighted = false;
		UpdatePossibleTarget();
	}

	public void HandleHoverStart(ItemSlot slot, WeaponSlotType weaponSlotType)
	{
		IsPossibleHovered = IsPossibleTarget(slot, weaponSlotType);
		UpdatePossibleTarget();
	}

	public void HandleHoverStop()
	{
		IsPossibleHovered = false;
		UpdatePossibleTarget();
	}

	private bool IsPossibleTarget(ItemSlot slot, WeaponSlotType weaponSlotType = WeaponSlotType.None)
	{
		if (slot.CanInsertItem(Item.Value))
		{
			return CanHighLight(Item.Value, weaponSlotType);
		}
		return false;
	}

	private void UpdatePossibleTarget()
	{
		PossibleTarget.Value = IsPossibleHighlighted || IsPossibleHovered;
	}

	private bool CanHighLight(ItemEntity item, WeaponSlotType weaponSlotType = WeaponSlotType.None)
	{
		BlueprintStarshipWeapon blueprintStarshipWeapon = item.Blueprint as BlueprintStarshipWeapon;
		if (!blueprintStarshipWeapon)
		{
			return true;
		}
		return blueprintStarshipWeapon?.AllowedSlots.Contains(weaponSlotType) ?? false;
	}
}

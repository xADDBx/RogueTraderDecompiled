using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;

public class CargoSlotVM : VirtualListElementVMBase
{
	public readonly string Title;

	public readonly string Description;

	public string TypeLabel;

	public Sprite TypeIcon;

	public readonly ReactiveProperty<int> TotalFillValue = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> UnusableFillValue = new ReactiveProperty<int>();

	public readonly CargoEntity CargoEntity;

	public SlotsGroupVM<ItemSlotVM> ItemSlotsGroup;

	private ItemsFilterType m_FilterType;

	private ItemsSorterType m_SorterType;

	private string m_SearchString;

	public readonly TooltipBaseTemplate Tooltip;

	private readonly Action<CargoSlotVM> m_SelectAction;

	public ReactiveCommand OnValueUpdate = new ReactiveCommand();

	public ReactiveCommand OnCheck = new ReactiveCommand();

	public bool IsChecked;

	public InventoryCargoViewType m_CargoViewType;

	public ReactiveCommand NeedBlink = new ReactiveCommand();

	public ItemsFilterType FilterType
	{
		set
		{
			m_FilterType = value;
			if (ItemSlotsGroup != null)
			{
				ItemSlotsGroup.FilterType.Value = m_FilterType;
			}
		}
	}

	public ItemsSorterType SorterType
	{
		set
		{
			m_SorterType = value;
			if (ItemSlotsGroup != null)
			{
				ItemSlotsGroup.SorterType.Value = m_SorterType;
			}
		}
	}

	public string SearchString
	{
		set
		{
			m_SearchString = value;
			if (ItemSlotsGroup != null)
			{
				ItemSlotsGroup.SearchString.Value = m_SearchString;
			}
		}
	}

	public bool IsNew => CargoEntity?.IsNew ?? false;

	private ItemsCollection ItemsCollection => CargoEntity?.Inventory.Collection;

	public bool IsEmpty
	{
		get
		{
			if (CargoEntity != null)
			{
				CargoEntity cargoEntity = CargoEntity;
				if (cargoEntity == null)
				{
					return false;
				}
				PartInventory inventory = cargoEntity.Inventory;
				int? obj;
				if (inventory == null)
				{
					obj = null;
				}
				else
				{
					ItemsCollection collection = inventory.Collection;
					obj = ((collection != null) ? new int?(collection.Items.Count((ItemEntity i) => i.InventorySlotIndex >= 0)) : null);
				}
				return obj == 0;
			}
			return true;
		}
	}

	public bool CanAddToSell
	{
		get
		{
			if (!IsEmpty)
			{
				return VendorHelper.Vendor.CanAddToSell(CargoEntity);
			}
			return false;
		}
	}

	public bool CanVendorBuyCargo => VendorHelper.Vendor.CanVendorBuyCargo(CargoEntity);

	public bool CanRemoveFromSell => VendorHelper.Vendor.CanRemoveFromSell(CargoEntity);

	public bool CanCheck
	{
		get
		{
			if (TotalFillValue.Value >= 100)
			{
				if (!CanAddToSell)
				{
					return CanRemoveFromSell;
				}
				return true;
			}
			return false;
		}
	}

	public CargoSlotVM(Action<CargoSlotVM> selectAction, CargoEntity entity, InventoryCargoViewType type = InventoryCargoViewType.Inventory)
	{
		CargoEntity = entity;
		Title = entity?.Blueprint.Name ?? string.Empty;
		Description = entity?.Blueprint.Description ?? string.Empty;
		ItemsItemOrigin origin = entity?.Blueprint.OriginType ?? ItemsItemOrigin.None;
		TypeLabel = UIStrings.Instance.CargoTexts.GetLabelByOrigin(origin);
		TypeIcon = UIConfig.Instance.UIIcons.CargoIcons.GetIconByOrigin(origin);
		m_SelectAction = selectAction;
		m_CargoViewType = type;
		if (entity != null)
		{
			Tooltip = new TooltipTemplateCargo(this);
		}
		UpdateValues(needUpdateActive: true);
	}

	protected override void DisposeImplementation()
	{
	}

	public void CreateItemSlotsGroup()
	{
		if (ItemSlotsGroup != null)
		{
			return;
		}
		if (m_CargoViewType == InventoryCargoViewType.Vendor)
		{
			if (Game.Instance.IsControllerMouse)
			{
				AddDisposable(ItemSlotsGroup = new ItemSlotsGroupVM(ItemsCollection, 8, 16, ItemsFilterType.NoFilter, ItemsSorterType.NotSorted, showUnavailableItems: true, showSlotHoldItemsInSlots: true, ItemSlotsGroupType.Cargo, needMaximumLimit: true, 16));
			}
			else
			{
				AddDisposable(ItemSlotsGroup = new ItemSlotsGroupVM(ItemsCollection, 6, 12, ItemsFilterType.NoFilter, ItemsSorterType.NotSorted, showUnavailableItems: true, showSlotHoldItemsInSlots: true, ItemSlotsGroupType.Cargo, needMaximumLimit: true, 12));
			}
		}
		else if (Game.Instance.IsControllerMouse)
		{
			AddDisposable(ItemSlotsGroup = new ItemSlotsGroupVM(ItemsCollection, 6, 12, ItemsFilterType.NoFilter, ItemsSorterType.NotSorted, showUnavailableItems: true, showSlotHoldItemsInSlots: true, ItemSlotsGroupType.Cargo, needMaximumLimit: true, 12));
		}
		else
		{
			AddDisposable(ItemSlotsGroup = new ItemSlotsGroupVM(ItemsCollection, 5, 10, ItemsFilterType.NoFilter, ItemsSorterType.NotSorted, showUnavailableItems: true, showSlotHoldItemsInSlots: true, ItemSlotsGroupType.Cargo, needMaximumLimit: true, 10));
		}
		ItemSlotsGroup.SearchString.Value = m_SearchString;
		AddDisposable(UniRxExtensionMethods.Subscribe(ItemSlotsGroup.CollectionChangedCommand, delegate
		{
			UpdateValues(needUpdateActive: true);
		}));
		UpdateValues(needUpdateActive: false);
	}

	public void HandleClick()
	{
		m_SelectAction?.Invoke(this);
	}

	public void HandleCheck()
	{
		if (!CanCheck)
		{
			if (!CanVendorBuyCargo)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(UIStrings.Instance.Vendor.CantSelectCargoForSell.Text + ". " + UIStrings.Instance.Vendor.VendorDontTakeThisCargo.Text, addToLog: false, WarningNotificationFormat.Attention);
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(UIStrings.Instance.Vendor.CantSelectCargoForSell.Text + ". " + UIStrings.Instance.Vendor.CargoIsNotFull.Text, addToLog: false, WarningNotificationFormat.Attention);
				});
			}
		}
		else if (IsChecked)
		{
			RemoveCargoFromSell();
		}
		else
		{
			AddCargoToSell();
		}
	}

	public void AddCargoToSell()
	{
		Game.Instance.GameCommandQueue.AddCargoToSell(CargoEntity);
		IsChecked = true;
		OnCheck?.Execute();
	}

	public void RemoveCargoFromSell()
	{
		Game.Instance.GameCommandQueue.RemoveCargoFromSell(CargoEntity);
		IsChecked = false;
		OnCheck?.Execute();
	}

	public void UpdateValues(bool needUpdateActive)
	{
		TotalFillValue.Value = ((!IsEmpty) ? CargoEntity.FilledVolumePercent : 0);
		UnusableFillValue.Value = ((!IsEmpty) ? CargoEntity.UnusableVolumePercent : 0);
		if (needUpdateActive)
		{
			bool value = ItemSlotsGroup?.VisibleCollection?.Any((ItemSlotVM i) => i.HasItem) ?? (!IsEmpty);
			Active.Value = value;
			m_IsAvailable.Value = value;
		}
	}

	public void OnUpdateValues()
	{
		ItemSlotsGroup?.UpdateVisibleCollection();
		UpdateValues(needUpdateActive: true);
		OnValueUpdate?.Execute();
	}

	public void SetNotNewState()
	{
		CargoEntity.IsNew = false;
	}

	public void Blink()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			NeedBlink?.Execute();
		}, 3);
	}
}

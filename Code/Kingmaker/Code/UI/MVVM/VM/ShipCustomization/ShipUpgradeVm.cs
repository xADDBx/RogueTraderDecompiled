using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Kingmaker.Code.UI.MVVM.VM.ShipCustomization;

public class ShipUpgradeVm : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IShipCustomizationUIHandler, ISubscriber, INewSlotsHandler, IMoveItemHandler, IShipComponentItemHandler, IEquipSlotHandler, IVoidShipRotationHandler
{
	public ShipComponentSlotVM PlasmaDrives;

	public ShipComponentSlotVM VoidShieldGenerator;

	public ShipComponentSlotVM AugerArray;

	public ShipComponentSlotVM ArmorPlating;

	public readonly List<ShipComponentSlotVM> Arsenals = new List<ShipComponentSlotVM>();

	public readonly List<ShipComponentSlotVM> Weapons = new List<ShipComponentSlotVM>();

	public ShipUpgradeSlotVM InternalStructure;

	public ShipUpgradeSlotVM ProwRam;

	public readonly ShipInventoryStashVM ShipInventoryStashVM;

	private readonly BoolReactiveProperty m_IsLocked = new BoolReactiveProperty();

	private readonly AutoDisposingList<ShipComponentSlotVM> m_AllSlots = new AutoDisposingList<ShipComponentSlotVM>();

	public readonly ReactiveProperty<ShipItemSelectorWindowVM> ShipSelectorWindowVM = new ReactiveProperty<ShipItemSelectorWindowVM>();

	public readonly BoolReactiveProperty ChooseSlotMode = new BoolReactiveProperty();

	public InventorySlotConsoleView ItemToSlotView;

	public readonly BoolReactiveProperty CanSetDefaultPosition = new BoolReactiveProperty();

	public readonly PlayerShipType ShipType;

	public ShipUpgradeVm(bool isLocked)
	{
		m_IsLocked.Value = isLocked;
		AddDisposable(ShipInventoryStashVM = new ShipInventoryStashVM(inventory: true));
		AddDisposable(EventBus.Subscribe(this));
		ShipType = Game.Instance.Player.PlayerShip.Blueprint.ShipType;
		UpdateSlots();
	}

	protected override void DisposeImplementation()
	{
		DisposeSlotsList(Arsenals);
		DisposeSlotsList(Weapons);
		ShipSelectorWindowVM.Value?.Dispose();
	}

	private void DisposeSlotsList(List<ShipComponentSlotVM> slots)
	{
		foreach (ShipComponentSlotVM slot in slots)
		{
			slot.Dispose();
		}
		slots.Clear();
	}

	public void HandleChangeItem(ShipComponentSlotVM slot)
	{
		if (RootUIContext.Instance.CurrentServiceWindow == ServiceWindowsType.CharacterInfo)
		{
			return;
		}
		if (ChooseSlotMode.Value && ItemToSlotView != null)
		{
			slot.InsertItem(ItemToSlotView.Item);
			EventBus.RaiseEvent(delegate(IInventoryHandler h)
			{
				h.Refresh();
			});
			ChooseSlotMode.Value = false;
			return;
		}
		List<ShipComponentItemSlotVM> list = SetupSlots(Game.Instance.Player.Inventory, slot, ItemsSorterType.TypeUp, GetFilterType(slot.SlotType));
		if (list.Count > 0)
		{
			ShipSelectorWindowVM.Value?.Dispose();
			ShipSelectorWindowVM.Value = new ShipItemSelectorWindowVM(delegate(ShipComponentItemSlotVM x)
			{
				slot.InsertItem(x.Item);
			}, HideSelectionWindow, list, slot);
		}
		else
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.NothingToInsertInThisSlot.Text, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
	}

	private void HideSelectionWindow()
	{
		ShipSelectorWindowVM.Value?.Dispose();
		ShipSelectorWindowVM.Value = null;
	}

	private static List<ShipComponentItemSlotVM> SetupSlots(ItemsCollection itemsCollection, ShipComponentSlotVM slot, ItemsSorterType sorterType, ItemsFilterType filterType)
	{
		List<ItemEntity> list = ItemsFilter.ItemSorter(itemsCollection.ToList(), sorterType, filterType);
		list.RemoveAll((ItemEntity item) => item != null && !ItemsFilter.ShouldShowItem(item, filterType));
		list.RemoveAll((ItemEntity item) => item.HoldingSlot != null || !slot.ItemSlot.PossibleEquipItem(item));
		if (slot.HasItem)
		{
			list.Insert(0, slot.ItemEntity);
		}
		return list.Select((ItemEntity item) => new ShipComponentItemSlotVM(item)).ToList();
	}

	private void UpdateSlots()
	{
		Warhammer.SpaceCombat.StarshipLogic.Equipment.HullSlots hullSlots = Game.Instance.Player.PlayerShip.GetHull().HullSlots;
		m_AllSlots.Clear();
		AddDisposable(PlasmaDrives = new ShipComponentSlotVM(ShipComponentSlotType.PlasmaDrives, hullSlots.PlasmaDrives, -1, WeaponSlotType.None, m_IsLocked.Value));
		m_AllSlots.Add(PlasmaDrives);
		AddDisposable(VoidShieldGenerator = new ShipComponentSlotVM(ShipComponentSlotType.VoidShieldGenerator, hullSlots.VoidShieldGenerator, -1, WeaponSlotType.None, m_IsLocked.Value));
		m_AllSlots.Add(VoidShieldGenerator);
		AddDisposable(AugerArray = new ShipComponentSlotVM(ShipComponentSlotType.AugerArray, hullSlots.AugerArray, -1, WeaponSlotType.None, m_IsLocked.Value));
		m_AllSlots.Add(AugerArray);
		AddDisposable(ArmorPlating = new ShipComponentSlotVM(ShipComponentSlotType.ArmorPlating, hullSlots.ArmorPlating, -1, WeaponSlotType.None, m_IsLocked.Value));
		m_AllSlots.Add(ArmorPlating);
		for (int i = 0; i < hullSlots.Arsenals.Count; i++)
		{
			ShipComponentSlotVM item = new ShipComponentSlotVM(ShipComponentSlotType.Arsenal, hullSlots.Arsenals[i], i, WeaponSlotType.None, m_IsLocked.Value);
			Arsenals.Add(item);
			m_AllSlots.Add(item);
		}
		for (int j = 0; j < hullSlots.WeaponSlots.Count; j++)
		{
			switch (hullSlots.WeaponSlots[j].Type)
			{
			case WeaponSlotType.Prow:
				Weapons.Add(Weapons.Any((ShipComponentSlotVM x) => x.WeaponSlotType == WeaponSlotType.Prow) ? new ShipComponentSlotVM(ShipComponentSlotType.Prow1, hullSlots.WeaponSlots[j], j, WeaponSlotType.Prow, m_IsLocked.Value) : new ShipComponentSlotVM(ShipComponentSlotType.Prow2, hullSlots.WeaponSlots[j], j, WeaponSlotType.Prow, m_IsLocked.Value));
				m_AllSlots.Add(Weapons[j]);
				break;
			case WeaponSlotType.Port:
				Weapons.Add(new ShipComponentSlotVM(ShipComponentSlotType.Port, hullSlots.WeaponSlots[j], j, WeaponSlotType.Port, m_IsLocked.Value));
				m_AllSlots.Add(Weapons[j]);
				break;
			case WeaponSlotType.Starboard:
				Weapons.Add(new ShipComponentSlotVM(ShipComponentSlotType.Starboard, hullSlots.WeaponSlots[j], j, WeaponSlotType.Starboard, m_IsLocked.Value));
				m_AllSlots.Add(Weapons[j]);
				break;
			case WeaponSlotType.Dorsal:
				Weapons.Add(new ShipComponentSlotVM(ShipComponentSlotType.Dorsal, hullSlots.WeaponSlots[j], j, WeaponSlotType.Dorsal, m_IsLocked.Value));
				m_AllSlots.Add(Weapons[j]);
				break;
			}
		}
		InternalStructure?.Dispose();
		RemoveDisposable(InternalStructure);
		AddDisposable(InternalStructure = new ShipUpgradeSlotVM(m_IsLocked.Value));
		ProwRam?.Dispose();
		RemoveDisposable(ProwRam);
		AddDisposable(ProwRam = new ShipUpgradeSlotVM(m_IsLocked.Value));
	}

	public void HandleOpenShipCustomization()
	{
	}

	public void HandleCloseAllComponentsMenu()
	{
	}

	public void HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
	}

	public void HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
		if (m_IsLocked.Value)
		{
			return;
		}
		if (to is ShipComponentSlotVM shipComponentSlotVM)
		{
			BlueprintStarshipWeapon blueprintStarshipWeapon = from?.Item?.Value?.Blueprint as BlueprintStarshipWeapon;
			if ((bool)blueprintStarshipWeapon && (blueprintStarshipWeapon == null || !((IList)blueprintStarshipWeapon.AllowedSlots).Contains((object)shipComponentSlotVM.WeaponSlotType)))
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(UIStrings.Instance.ShipCustomization.CantInsertInThisWeaponSlot.Text, addToLog: false, WarningNotificationFormat.Short);
				});
				return;
			}
		}
		if ((bool)(from?.Item?.Value?.Blueprint as BlueprintStarshipItem) && from is ShipComponentSlotVM shipComponentSlotVM2 && ((shipComponentSlotVM2.SlotType == ShipComponentSlotType.PlasmaDrives && to.Item.Value == null) || from.Item == to.Item))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.PlaceEngineSlot.Text, addToLog: false);
			});
		}
		else
		{
			InventoryHelper.TryMoveSlotInInventory(from, to);
		}
	}

	public void HandleTrySplitSlot(ItemSlotVM slot)
	{
		if (!m_IsLocked.Value)
		{
			InventoryHelper.TrySplitSlot(slot, isLoot: false);
		}
	}

	private ItemsFilterType GetFilterType(ShipComponentSlotType shipComponentSlotType)
	{
		return shipComponentSlotType switch
		{
			ShipComponentSlotType.PlasmaDrives => ItemsFilterType.PlasmaDrives, 
			ShipComponentSlotType.VoidShieldGenerator => ItemsFilterType.VoidShieldGenerator, 
			ShipComponentSlotType.ArmorPlating => ItemsFilterType.ArmorPlating, 
			ShipComponentSlotType.AugerArray => ItemsFilterType.AugerArray, 
			ShipComponentSlotType.Dorsal => ItemsFilterType.Dorsal, 
			ShipComponentSlotType.Prow1 => ItemsFilterType.Prow, 
			ShipComponentSlotType.Prow2 => ItemsFilterType.Prow, 
			ShipComponentSlotType.Port => ItemsFilterType.Port, 
			ShipComponentSlotType.Starboard => ItemsFilterType.Starboard, 
			ShipComponentSlotType.Arsenal => ItemsFilterType.Arsenal, 
			_ => throw new ArgumentOutOfRangeException("shipComponentSlotType", shipComponentSlotType, null), 
		};
	}

	public void HandleMoveItem(bool isEquip)
	{
		ShipInventoryStashVM.CollectionChanged();
	}

	public void ChooseSlotToItem(InventorySlotConsoleView item)
	{
		List<ShipComponentSlotVM> list = m_AllSlots.Where((ShipComponentSlotVM slotVM) => slotVM.IsPossibleTarget(item.SlotVM.ItemEntity)).ToList();
		if (list.Count > 1)
		{
			ItemToSlotView = item;
			ChooseSlotMode.Value = true;
		}
		else if (list.Count == 1)
		{
			list.FirstOrDefault()?.InsertItem(item.SlotVM.Item.Value);
		}
	}

	public void HandleOnRotationStart()
	{
		CanSetDefaultPosition.Value = true;
	}

	public void HandleOnRotationStop()
	{
		CanSetDefaultPosition.Value = false;
	}
}

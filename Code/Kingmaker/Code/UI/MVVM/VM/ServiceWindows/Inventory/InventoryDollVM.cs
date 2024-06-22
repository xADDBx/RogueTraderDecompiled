using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UI.Sound;
using Kingmaker.View.Mechadendrites;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public class InventoryDollVM : CharInfoComponentVM, IInventoryItemHandler, ISubscriber, IEquipSlotHandler
{
	public EquipSlotVM Armor;

	public EquipSlotVM Belt;

	public EquipSlotVM Head;

	public EquipSlotVM Feet;

	public EquipSlotVM Gloves;

	public EquipSlotVM Neck;

	public EquipSlotVM Ring1;

	public EquipSlotVM Ring2;

	public EquipSlotVM Wrist;

	public EquipSlotVM Shoulders;

	public EquipSlotVM Glasses;

	public EquipSlotVM Shirt;

	public readonly EquipSlotVM[] QuickSlots = new EquipSlotVM[4];

	public SelectionGroupRadioVM<WeaponSetVM> WeaponSetSelector;

	public readonly ReactiveProperty<WeaponSetVM> CurrentSet = new ReactiveProperty<WeaponSetVM>();

	public List<WeaponSetVM> WeaponSets;

	public readonly ReactiveProperty<bool> CanChangeEquipment = new ReactiveProperty<bool>(initialValue: true);

	private readonly AutoDisposingList<EquipSlotVM> m_AllEquipSlots = new AutoDisposingList<EquipSlotVM>();

	public readonly CharInfoEncumbranceVM EncumbranceVM;

	public readonly ReactiveProperty<CharacterVisualSettingsVM> VisualSettingsVM = new ReactiveProperty<CharacterVisualSettingsVM>();

	public readonly ReactiveProperty<InventorySelectorWindowVM> InventorySelectorWindowVM;

	public readonly BoolReactiveProperty ChooseSlotMode = new BoolReactiveProperty();

	private InventorySlotConsoleView m_ItemToSlotView;

	public readonly bool IsPet;

	private Action OnWeaponSetChangedAction;

	public InventorySlotConsoleView ItemToSlotView => m_ItemToSlotView;

	public InventoryDollVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, Action onWeaponSetChanged)
		: base(unit)
	{
		OnWeaponSetChangedAction = onWeaponSetChanged;
		IsPet = unit?.Value != null && (unit.Value.Body.IsPolymorphed || unit.Value.IsPet);
		AddDisposable(CurrentSet.Subscribe(OnWeaponSetChanged));
		AddDisposable(EncumbranceVM = new CharInfoEncumbranceVM(unit));
		AddDisposable(InventorySelectorWindowVM = new ReactiveProperty<InventorySelectorWindowVM>());
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		ChooseSlotMode.Value = false;
		HideSelectionWindow();
	}

	private void OnWeaponSetChanged(WeaponSetVM set)
	{
		if (CanChangeEquipment.Value && Unit?.Value != null)
		{
			Game.Instance.GameCommandQueue.SwitchHandEquipment(Unit.Value, set.Index);
			OnWeaponSetChangedAction();
		}
	}

	private void HideSelectionWindow()
	{
		InventorySelectorWindowVM.Value?.Dispose();
		InventorySelectorWindowVM.Value = null;
	}

	public override void HandleUICommitChanges()
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		if (Unit.Value == null)
		{
			return;
		}
		ClearEquipSlots();
		PartUnitBody body = Unit.Value.Body;
		AddDisposable(Armor = new EquipSlotVM(EquipSlotType.Armor, body.Armor));
		m_AllEquipSlots.Add(Armor);
		AddDisposable(Belt = new EquipSlotVM(EquipSlotType.Belt, body.Belt));
		m_AllEquipSlots.Add(Belt);
		AddDisposable(Head = new EquipSlotVM(EquipSlotType.Head, body.Head));
		m_AllEquipSlots.Add(Head);
		AddDisposable(Feet = new EquipSlotVM(EquipSlotType.Feet, body.Feet));
		m_AllEquipSlots.Add(Feet);
		AddDisposable(Gloves = new EquipSlotVM(EquipSlotType.Gloves, body.Gloves));
		m_AllEquipSlots.Add(Gloves);
		AddDisposable(Neck = new EquipSlotVM(EquipSlotType.Neck, body.Neck));
		m_AllEquipSlots.Add(Neck);
		AddDisposable(Ring1 = new EquipSlotVM(EquipSlotType.Ring1, body.Ring1));
		m_AllEquipSlots.Add(Ring1);
		AddDisposable(Ring2 = new EquipSlotVM(EquipSlotType.Ring2, body.Ring2));
		m_AllEquipSlots.Add(Ring2);
		AddDisposable(Wrist = new EquipSlotVM(EquipSlotType.Wrist, body.Wrist));
		m_AllEquipSlots.Add(Wrist);
		AddDisposable(Shoulders = new EquipSlotVM(EquipSlotType.Shoulders, body.Shoulders));
		m_AllEquipSlots.Add(Shoulders);
		AddDisposable(Glasses = new EquipSlotVM(EquipSlotType.Glasses, body.Glasses));
		m_AllEquipSlots.Add(Glasses);
		AddDisposable(Shirt = new EquipSlotVM(EquipSlotType.Shirt, body.Shirt));
		m_AllEquipSlots.Add(Shirt);
		for (int i = 0; i < body.QuickSlots.Length; i++)
		{
			QuickSlots[i]?.Dispose();
			AddDisposable(QuickSlots[i] = new EquipSlotVM(EquipSlotType.QuickSlot1, body.QuickSlots.ElementAt(i), -1, null, i));
			m_AllEquipSlots.Add(QuickSlots[i]);
		}
		if (WeaponSets == null)
		{
			WeaponSets = new List<WeaponSetVM>();
			for (int j = 0; j < 2; j++)
			{
				int setId = j;
				WeaponSets.Add(new WeaponSetVM(j, delegate
				{
					CurrentSet.Value = WeaponSets.ElementAtOrDefault(setId);
				}));
				AddDisposable(WeaponSets[j]);
			}
			AddDisposable(WeaponSetSelector = new SelectionGroupRadioVM<WeaponSetVM>(WeaponSets, CurrentSet));
		}
		for (int k = 0; k < WeaponSets.Count; k++)
		{
			WeaponSets[k].SetEnabled(!Unit.Value.HasMechadendrites() || k == 0);
			WeaponSets[k].Primary?.Dispose();
			EquipSlotVM disposable = (WeaponSets[k].Primary = new EquipSlotVM(EquipSlotType.PrimaryHand, body.HandsEquipmentSets[k].PrimaryHand, -1, null, k));
			AddDisposable(disposable);
			m_AllEquipSlots.Add(WeaponSets[k].Primary);
			WeaponSets[k].Secondary?.Dispose();
			disposable = (WeaponSets[k].Secondary = new EquipSlotVM(EquipSlotType.SecondaryHand, body.HandsEquipmentSets[k].SecondaryHand, -1, WeaponSets[k].Primary, k));
			AddDisposable(disposable);
			m_AllEquipSlots.Add(WeaponSets[k].Secondary);
		}
		CurrentSet.Value = WeaponSets[Unit.Value.Body.CurrentHandEquipmentSetIndex];
		UpdateVisualSettings();
	}

	private void ClearEquipSlots()
	{
		m_AllEquipSlots.Clear();
	}

	public void SwitchVisualSettings()
	{
		if (VisualSettingsVM.Value == null)
		{
			ShowVisualSettings();
		}
		else
		{
			HideVisualSettings();
		}
	}

	public void ShowVisualSettings()
	{
		if (VisualSettingsVM.Value == null)
		{
			CharacterVisualSettingsVM disposable = (VisualSettingsVM.Value = new CharacterVisualSettingsVM(Unit.Value, HideVisualSettings));
			AddDisposable(disposable);
		}
	}

	public void HideVisualSettings()
	{
		DisposeAndRemove(VisualSettingsVM);
	}

	private void UpdateVisualSettings()
	{
		if (VisualSettingsVM.Value != null && VisualSettingsVM.Value.Unit != Unit.Value)
		{
			HideVisualSettings();
		}
	}

	public void HandleChangeItem(EquipSlotVM slot)
	{
		if (ChooseSlotMode.Value && ItemToSlotView != null)
		{
			InventoryHelper.TryMoveSlotInInventory(ItemToSlotView.SlotVM, slot);
			EventBus.RaiseEvent(delegate(IInventoryHandler h)
			{
				h.Refresh();
			});
			ChooseSlotMode.Value = false;
			return;
		}
		List<EquipSelectorSlotVM> list = SetupSlots(Game.Instance.Player.Inventory, slot);
		if (list.Count > 0)
		{
			ShowSelectorWindow(slot, list);
			return;
		}
		UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(UIStrings.Instance.ShipCustomization.NothingToInsertInThisSlot.Text, addToLog: false, WarningNotificationFormat.Attention, withSound: false);
		});
	}

	private static List<EquipSelectorSlotVM> SetupSlots(ItemsCollection itemsCollection, EquipSlotVM slot)
	{
		List<ItemEntity> list = ItemsFilter.ItemSorter(itemsCollection.ToList(), ItemsSorterType.TypeUp, ItemsFilterType.NoFilter);
		list.RemoveAll((ItemEntity item) => item != null && !ItemsFilter.ShouldShowItem(item, ItemsFilterType.NoFilter));
		list.RemoveAll((ItemEntity item) => item.HoldingSlot != null || !slot.ItemSlot.PossibleEquipItem(item));
		if (slot.HasItem)
		{
			list.Insert(0, slot.ItemEntity);
		}
		return list.Select((ItemEntity item) => new EquipSelectorSlotVM(item)).ToList();
	}

	private void ShowSelectorWindow(EquipSlotVM slot, List<EquipSelectorSlotVM> possibleItems)
	{
		InventorySelectorWindowVM.Value?.Dispose();
		InventorySelectorWindowVM.Value = new InventorySelectorWindowVM(TryInsertItem, HideSelectionWindow, possibleItems, slot);
		void TryInsertItem(EquipSelectorSlotVM selectorSlotVM)
		{
			if (Game.Instance.Player.IsInCombat)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, addToLog: false, WarningNotificationFormat.Short);
				});
			}
			else
			{
				BaseUnitEntity baseUnitEntity = (BaseUnitEntity)slot.ItemSlot.Owner;
				if (baseUnitEntity != null && InventoryHelper.CanEquipItem(selectorSlotVM.Item, baseUnitEntity))
				{
					Game.Instance.GameCommandQueue.EquipItem(selectorSlotVM.Item, baseUnitEntity, slot.ToSlotRef());
					EventBus.RaiseEvent(delegate(IInventoryHandler h)
					{
						h.Refresh();
					});
					HideSelectionWindow();
				}
			}
		}
	}

	public void ChooseSlotToItem(InventorySlotConsoleView item)
	{
		if (Game.Instance.Player.IsInCombat)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, addToLog: false, WarningNotificationFormat.Short);
			});
			item.ReleaseSlot();
		}
		int count = m_AllEquipSlots.Where((EquipSlotVM slotVM) => slotVM.ItemSlot.CanInsertItem(item.SlotVM.ItemEntity)).ToList().Count;
		if (count <= 1)
		{
			if (count == 1)
			{
				EventBus.RaiseEvent(delegate(IInventoryHandler h)
				{
					h.TryEquip(item.SlotVM);
				});
			}
			else
			{
				item.ReleaseSlot();
			}
		}
		else
		{
			m_ItemToSlotView = item;
			ChooseSlotMode.Value = true;
		}
	}
}

using System;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public class InventoryVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IInventoryHandler, ISubscriber, INewSlotsHandler, ISelectionHandler, ISubscriber<IBaseUnitEntity>, IEquipItemAutomaticallyHandler, IInsertItemHandler, IDropItemHandler, IMoveItemHandler, IUnequipItemHandler
{
	public readonly ReactiveProperty<BaseUnitEntity> Unit;

	public readonly CharInfoNameAndPortraitVM NameAndPortraitVM;

	public readonly CharInfoLevelClassScoresVM LevelClassScoresVM;

	public readonly CharInfoSkillsAndWeaponsVM CharInfoSkillsAndWeaponsVM;

	public readonly InventoryDollVM DollVM;

	public readonly InventoryStashVM StashVM;

	public readonly UnitBuffPartVM UnitBuffPartVM;

	public readonly ReactiveProperty<InventoryEquipSelectorVM> EquipSelectorVM = new ReactiveProperty<InventoryEquipSelectorVM>();

	public InventoryVM()
	{
		ReactiveProperty<BaseUnitEntity> selectedUnitInUI = Game.Instance.SelectionCharacter.SelectedUnitInUI;
		if (selectedUnitInUI.Value == null)
		{
			BaseUnitEntity baseUnitEntity = (selectedUnitInUI.Value = Game.Instance.Player.MainCharacterEntity);
		}
		Unit = Game.Instance.SelectionCharacter.SelectedUnitInUI;
		AddDisposable(Unit.ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			OnUnitChanged();
		}));
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(NameAndPortraitVM = new CharInfoNameAndPortraitVM(Unit));
		AddDisposable(LevelClassScoresVM = new CharInfoLevelClassScoresVM(Unit));
		AddDisposable(CharInfoSkillsAndWeaponsVM = new CharInfoSkillsAndWeaponsVM(Unit));
		AddDisposable(StashVM = new InventoryStashVM(inventory: true));
		AddDisposable(DollVM = new InventoryDollVM(Unit, OnWeaponSetChanged));
		AddDisposable(UnitBuffPartVM = new UnitBuffPartVM(Unit.Value));
		EventBus.RaiseEvent(delegate(IUIEventHandler h)
		{
			h.HandleUIEvent(UIEventType.InventoryOpen);
		});
	}

	private void OnWeaponSetChanged()
	{
		StashVM?.OnWeaponSetChanged();
	}

	private void OnUnitChanged()
	{
		if (Unit.Value != null)
		{
			StashVM?.CollectionChanged();
		}
	}

	public void Refresh()
	{
		Unit.SetValueAndForceNotify(Unit.Value);
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
		InventoryHelper.TryEquip(slot, Unit?.Value);
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
		InventoryHelper.TryMoveToInventory(slot);
	}

	public void ShowEquipSelector(EquipSlotVM slot)
	{
		InventoryEquipSelectorVM disposable = (EquipSelectorVM.Value = new InventoryEquipSelectorVM(HideEquipSelector, slot));
		AddDisposable(disposable);
	}

	private void HideEquipSelector()
	{
		EquipSelectorVM.Value.Dispose();
		EquipSelectorVM.Value = null;
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
	}

	public void HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
		InventoryHelper.TryMoveSlotInInventory(from, to);
	}

	public void HandleTrySplitSlot(ItemSlotVM slot)
	{
		InventoryHelper.TrySplitSlot(slot, isLoot: false);
	}

	public void OnUnitSelectionAdd(bool single, bool ask)
	{
		UnitBuffPartVM.SetUnitData(EventInvokerExtensions.BaseUnitEntity);
	}

	public void OnUnitSelectionRemove()
	{
	}

	void IEquipItemAutomaticallyHandler.HandleEquipItemAutomatically(ItemEntity item)
	{
		Refresh();
	}

	void IDropItemHandler.HandleDropItem(ItemEntity item, bool split)
	{
		Refresh();
	}

	void IInsertItemHandler.HandleInsertItem(ItemSlot slot)
	{
		Refresh();
	}

	void IUnequipItemHandler.HandleUnequipItem()
	{
		Refresh();
	}

	void IMoveItemHandler.HandleMoveItem(bool isEquip)
	{
		Refresh();
	}
}

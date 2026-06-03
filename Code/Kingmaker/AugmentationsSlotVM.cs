using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.Root.Strings;
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
using Kingmaker.UI.Sound;
using UniRx;

namespace Kingmaker;

public class AugmentationsSlotVM : ItemSlotVM, IInsertItemHandler, ISubscriber, IUnequipItemHandler, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, IMoveItemHandler, IEquipSlotPossibleTarget, ISubscriber<ItemEntity>, IEquipSlotHoverHandler, IAugmentOverdriveToggleHandler, IAugmentSlotRefreshHandler
{
	public BaseUnitEntity Unit;

	public BlueprintAugmentSlot BlueprintAugmentSlot;

	public readonly AugmentationsEnum SlotType;

	private readonly ReactiveProperty<string> m_SlotDescription = new ReactiveProperty<string>(string.Empty);

	public new readonly ReactiveProperty<bool> PossibleTarget = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> ShowPossibleTarget = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveProperty<AugmentationsPlayerSize> PlayerSize = new ReactiveProperty<AugmentationsPlayerSize>();

	public new readonly BoolReactiveProperty IsLocked = new BoolReactiveProperty();

	public readonly BoolReactiveProperty HasTrauma = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsDirty = new BoolReactiveProperty();

	public ReactiveCommand RefreshOverchargeAbility = new ReactiveCommand();

	public ReactiveCommand<bool> ToggleLinesCommand = new ReactiveCommand<bool>();

	private bool m_IsPossibleHighlighted;

	private bool m_IsPossibleHovered;

	public bool IsOverchargeSlot;

	private ItemEntity m_ItemCache;

	public int SetIndex { get; }

	public ItemSlot ItemSlot { get; }

	public bool IsDefault
	{
		get
		{
			if (ItemSlot is AugmentSlot augmentSlot)
			{
				return augmentSlot.IsDefaultAugmentEquipped;
			}
			return false;
		}
	}

	public AugmentationsSlotVM(BaseUnitEntity unit, BlueprintAugmentSlot augmentSlot, ItemSlot itemSlot, int index = -1, bool isLocked = false, bool isOverchargeSlot = false, bool hasTrauma = false)
		: base(itemSlot.MaybeItem, index)
	{
		Unit = unit;
		BlueprintAugmentSlot = augmentSlot;
		SlotType = UIUtilityItem.GetAugmentationSlotType(augmentSlot);
		ItemSlot = itemSlot;
		SetIndex = index;
		IsLocked.Value = isLocked;
		HasTrauma.Value = hasTrauma;
		m_SlotDescription.Value = UIUtilityItem.GetAugmentSlotName(SlotType);
		IsOverchargeSlot = isOverchargeSlot;
		IsDirty.Value = false;
		m_ItemCache = itemSlot.MaybeItem;
		RefreshItem();
	}

	public AugmentationsSlotVM(BaseUnitEntity unit, bool isOverchargeSlot = true, bool isLocked = false)
		: base(null, -1)
	{
		Unit = unit;
		SlotType = UIUtilityItem.GetAugmentationSlotType(null);
		m_SlotDescription.Value = string.Empty;
		IsOverchargeSlot = isOverchargeSlot;
		IsLocked.Value = isLocked;
		RefreshItem();
	}

	public bool IsPossibleTarget(ItemEntity item)
	{
		if (ItemSlot == null)
		{
			return false;
		}
		if (ItemSlot.CanInsertItem(item))
		{
			if (ItemSlot.HasItem)
			{
				return ItemSlot.CanRemoveItem();
			}
			return true;
		}
		return false;
	}

	void IInsertItemHandler.HandleInsertItem(ItemSlot slot)
	{
		RefreshItem();
	}

	void IUnitEquipmentHandler.HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		RefreshItem();
	}

	void IUnequipItemHandler.HandleUnequipItem()
	{
		RefreshItem();
	}

	void IMoveItemHandler.HandleMoveItem(bool isEquip)
	{
		if (isEquip)
		{
			RefreshItem();
		}
	}

	private void RefreshItem()
	{
		if (IsOverchargeSlot)
		{
			RefreshOverchargeAbility.Execute();
			return;
		}
		IsDirty.Value = ItemSlot.MaybeItem != m_ItemCache;
		if (IsDefault)
		{
			IsDirty.Value = false;
		}
		if (ItemSlot != null)
		{
			Item.Value = ItemSlot.MaybeItem;
			RefreshOverchargeAbility.Execute();
		}
	}

	public void HandleHighlightStart(ItemEntity item)
	{
		m_IsPossibleHighlighted = IsPossibleTarget(item);
		UpdatePossibleTarget();
	}

	public new void HandleHighlightStop()
	{
		m_IsPossibleHighlighted = false;
		UpdatePossibleTarget();
	}

	private void UpdatePossibleTarget()
	{
		PossibleTarget.Value = m_IsPossibleHighlighted || m_IsPossibleHovered;
	}

	public void HandleHoverStart(ItemEntity item)
	{
		m_IsPossibleHovered = IsPossibleTarget(item);
		UpdatePossibleTarget();
	}

	public new void HandleHoverStop()
	{
		m_IsPossibleHovered = false;
		UpdatePossibleTarget();
	}

	public void SetPossibleTargetState(bool state)
	{
		ShowPossibleTarget.Value = state;
	}

	public void HandleAugmentOverdriveToggle(BlueprintAugmentSlot currentSlot, int currentLayer)
	{
		if (Unit.CanBeControlled() && !Game.Instance.TurnController.InCombat && !IsDirty.Value && currentSlot == BlueprintAugmentSlot)
		{
			switch (currentLayer)
			{
			case 1:
				Game.Instance.GameCommandQueue.SetAugmentOverdrive(Unit, BlueprintAugmentSlot);
				break;
			case 2:
				Game.Instance.GameCommandQueue.SetAugmentOverdrive(Unit, null);
				break;
			}
		}
	}

	public void HandleAugmentSlotRefresh()
	{
		RefreshItem();
	}

	public void HideLines()
	{
		ToggleLinesCommand.Execute(parameter: false);
	}

	public void ShowLines()
	{
		ToggleLinesCommand.Execute(parameter: true);
	}

	public void InsertItem(ItemEntity item)
	{
		if (IsLocked.Value)
		{
			UISounds.Instance.Sounds.AugmentationsWindow.AugmentViewOnlyInstall.Play();
		}
		else if (HasTrauma.Value)
		{
			UISounds.Instance.Sounds.AugmentationsWindow.AugmentBrokenInstall.Play();
		}
		else if (ItemSlot != null && ItemSlot.CanInsertItem(item))
		{
			Game.Instance.GameCommandQueue.EquipItem(item, ItemSlot.Owner, this.ToSlotRef());
		}
	}

	public void ApplyInstallation()
	{
		Game.Instance.GameCommandQueue.ApplyAugmentInsertion(Unit, BlueprintAugmentSlot);
		m_ItemCache = ItemSlot.MaybeItem;
		IsDirty.Value = false;
		RefreshOverchargeAbility.Execute();
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(UIStrings.Instance.UIAugmentations.AugmentInstalled.Text, addToLog: false);
		});
	}
}

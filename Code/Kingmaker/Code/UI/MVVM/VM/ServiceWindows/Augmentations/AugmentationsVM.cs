using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Inspect;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Augmentations;

public class AugmentationsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IInventoryHandler, ISubscriber, INewSlotsHandler, IUnequipItemHandler, IAugmentUnequipHandler, IAugmentOverdriveToggleHandler, IAugmentOverchargeSlotDragHandler, IDollCharacterDragUIHandler, IEquipSlotHandler, IAugmentationItemHandler, IAgumentationsDollRotationHandler, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, IUnitOverdriveAugmentHandler
{
	public readonly ReactiveProperty<BaseUnitEntity> Unit;

	private BaseUnitEntity PreviousUnit;

	public UnitAugments UnitAugments;

	public readonly AugmentationsInventoryStashVM StashVM;

	public AugmentationsInspectVM InspectVM;

	public List<AugmentationsSlotVM> AllAugmentSlots = new List<AugmentationsSlotVM>();

	public ReactiveProperty<string> Metallization = new ReactiveProperty<string>();

	public ReactiveProperty<bool> IsForgeWorldUnit = new ReactiveProperty<bool>();

	public ReactiveProperty<bool> IsPasqal = new ReactiveProperty<bool>();

	public ReactiveProperty<bool> IsManipulus = new ReactiveProperty<bool>();

	public AugmentationsSlotVM EmptyAugmentSlotVM;

	public readonly BoolReactiveProperty ChooseSlotMode = new BoolReactiveProperty();

	public InventorySlotConsoleView ItemToSlotView;

	public readonly ReactiveProperty<AugmentationsSelectorWindowVM> InventorySelectorWindowVM;

	public ReactiveCommand HandleOverdriveLabelsUpdate = new ReactiveCommand();

	public readonly StarSystemSpaceBarksHolderVM StarSystemSpaceBarksHolderVM;

	public bool HaveDirtySlot;

	private bool m_IsOverchargeSlotDragInProgress;

	private float m_BarkBanterHideTimer = 3f;

	public bool RootUiConfigTest;

	public static bool AugmentsViewOnly => Game.Instance.LoadedAreaState?.Settings.IsAugmentsViewOnly() ?? true;

	public AugmentationsVM()
	{
		ReactiveProperty<BaseUnitEntity> selectedUnitInUI = Game.Instance.SelectionCharacter.SelectedUnitInUI;
		if (selectedUnitInUI.Value == null)
		{
			BaseUnitEntity baseUnitEntity = (selectedUnitInUI.Value = Game.Instance.Player.MainCharacterEntity);
		}
		Unit = Game.Instance.SelectionCharacter.SelectedUnitInUI;
		PreviousUnit = Unit.Value;
		UnitAugments = Unit.Value.GetOptional<PartUnitBody>()?.Augments;
		AddDisposable(Unit.Subscribe(delegate
		{
			OnUnitChanged();
		}));
		AddDisposable(InspectVM = new AugmentationsInspectVM());
		AddDisposable(StashVM = new AugmentationsInventoryStashVM(inventory: true, null, Unit));
		AddDisposable(InventorySelectorWindowVM = new ReactiveProperty<AugmentationsSelectorWindowVM>());
		AddDisposable(EventBus.Subscribe(this));
		EventBus.RaiseEvent(delegate(IUIEventHandler h)
		{
			h.HandleUIEvent(UIEventType.InventoryOpen);
		});
		if (!AugmentsViewOnly)
		{
			EventBus.RaiseEvent(delegate(IUIEventHandler h)
			{
				h.HandleUIEvent(UIEventType.AugmentWindowOpenWithEdit);
			});
		}
		Refresh();
		RefreshMetallization();
		if (!IsInSpace())
		{
			return;
		}
		m_BarkBanterHideTimer = 3f;
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.UpdateAsObservable(), delegate
		{
			BarkBanterOnTimerHideHandler();
		}));
		AddDisposable(StarSystemSpaceBarksHolderVM = new StarSystemSpaceBarksHolderVM());
		if (UIConfig.Instance.UIAugmentationsBarkBanters.IsFirstLaunchInSpace())
		{
			EventBus.RaiseEvent(delegate(IBarkBanterPlayedHandler e)
			{
				e.HandleBarkBanter(UIConfig.Instance.UIAugmentationsBarkBanters.FirstLaunchBarkBanter);
			});
			UIConfig.Instance.UIAugmentationsBarkBanters.SetFirstAugmentationsLaunchInSpace();
		}
		else
		{
			List<BlueprintBarkBanter> list = UIConfig.Instance.UIAugmentationsBarkBanters.BanterList.GetBarkBanters().ToList();
			if (list.Count > 0)
			{
				BlueprintBarkBanter currentBarkBanter = list[UnityEngine.Random.Range(0, list.Count)];
				EventBus.RaiseEvent(delegate(IBarkBanterPlayedHandler e)
				{
					e.HandleBarkBanter(currentBarkBanter);
				});
			}
		}
		UISounds.Instance.Sounds.AugmentationsWindow.AugmentBarkShow.Play();
	}

	protected override void DisposeImplementation()
	{
		TooltipHelper.HideTooltip();
		TooltipHelper.HideInfo();
		AugmentationsInspectVM inspectVM = InspectVM;
		if (inspectVM != null && inspectVM.IsShown)
		{
			Game.Instance.Player.UISettings.ShowInspect = false;
		}
		if (!Game.Instance.LoadedAreaState.Settings.AugmentsViewOnly)
		{
			Game.Instance.LoadedAreaState.Settings.AugmentsViewOnly.Retain();
		}
		EventBus.RaiseEvent(delegate(IBarkForceHideHandler e)
		{
			e.ForceHideBarkHandle();
		});
	}

	public void Refresh()
	{
		RefreshData();
	}

	protected void RefreshData()
	{
		ClearSlots();
		CreateSlotVMs();
		CheckIfUnitIsForgeWorld(Unit.Value);
		CheckIfUnitIsPascal(Unit.Value);
		CheckIfUnitIsManipulus(Unit.Value);
		EmptyAugmentSlotVM.RefreshOverchargeAbility.Execute();
		CheckUnitSize();
		ResetOrientationsAndLines();
		DelayedInvoker.InvokeInFrames(delegate
		{
			HandleOverdriveLabelsUpdate.Execute();
		}, 4);
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
		if (!IsBlockedByViewOnly())
		{
			ResetOrientationsAndLines();
			InventoryHelper.TryEquip(slot, Unit?.Value);
		}
	}

	void IInventoryHandler.TryDrop(ItemSlotVM slot)
	{
		if (!IsBlockedByViewOnly())
		{
			ResetOrientationsAndLines();
			InventoryHelper.TryDrop(slot);
		}
	}

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM slot, bool immediately = false)
	{
		if (!IsBlockedByViewOnly())
		{
			ResetOrientationsAndLines();
			InventoryHelper.TryUnequip((AugmentationsSlotVM)slot);
		}
	}

	private bool IsBlockedByViewOnly()
	{
		if (!AugmentsViewOnly)
		{
			return false;
		}
		UISounds.Instance.Sounds.AugmentationsWindow.AugmentViewOnlyInstall.Play();
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(UIStrings.Instance.UIAugmentations.CannotInsertAtThisMoment.Text, addToLog: false, WarningNotificationFormat.Short);
		});
		return true;
	}

	private void ResetOrientationsAndLines()
	{
		DelayedInvoker.InvokeWhenTrue(UIDollRooms.Instance.AugmentationsDollRoom.RotateToDefaultPosition, () => UIDollRooms.Instance != null && UIDollRooms.Instance.AugmentationsDollRoom != null);
	}

	public void TryMoveToCargo(ItemSlotVM slot, bool immediately = false)
	{
	}

	public void HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
	}

	public void HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
		if (m_IsOverchargeSlotDragInProgress)
		{
			m_IsOverchargeSlotDragInProgress = false;
			TryDisableOverdriveFromDrag();
		}
		else if (to is AugmentationsSlotVM { IsOverchargeSlot: not false } && from is AugmentationsSlotVM { IsOverchargeSlot: false } augmentationsSlotVM2)
		{
			TryEnableOverdriveFromDrag(augmentationsSlotVM2);
		}
		else
		{
			if (to is AugmentationsSlotVM && IsBlockedByViewOnly())
			{
				return;
			}
			if (from is AugmentationsSlotVM augmentationsSlotVM3 && UnitAugments.OverdriveSlot == augmentationsSlotVM3.BlueprintAugmentSlot && AllAugmentSlots.FirstOrDefault((AugmentationsSlotVM vm) => vm.IsOverchargeSlot) != null)
			{
				EventBus.RaiseEvent(delegate(IForceRebindOverdriveBlock h)
				{
					h.HandleForceRebindOverdriveBlock();
				});
			}
			if (to is AugmentationsSlotVM augmentationsSlotVM4 && augmentationsSlotVM4.HasTrauma.Value)
			{
				UISounds.Instance.Sounds.AugmentationsWindow.AugmentBrokenInstall.Play();
				return;
			}
			if (from is AugmentationsSlotVM augmentationsSlotVM5 && augmentationsSlotVM5.HasTrauma.Value)
			{
				UISounds.Instance.Sounds.AugmentationsWindow.AugmentBrokenInstall.Play();
				return;
			}
			ResetOrientationsAndLines();
			InventoryHelper.TryMoveSlotInInventory(from, to);
		}
	}

	public void HandleTrySplitSlot(ItemSlotVM slot)
	{
		InventoryHelper.TrySplitSlot(slot, isLoot: false);
	}

	private void OnUnitChanged()
	{
		if (Unit.Value != null)
		{
			StashVM?.CollectionChanged();
			RemovePreviousNotInstalled();
			RefreshData();
			RefreshMetallization();
			UnitAugments = Unit.Value.GetOptional<PartUnitBody>()?.Augments;
			PreviousUnit = Unit.Value;
			AugmentationsInspectVM inspectVM = InspectVM;
			if (inspectVM != null && inspectVM.IsShown)
			{
				InspectVM.HandleUnitConsoleInvoke(Unit.Value);
			}
		}
	}

	private void CreateSlotVMs()
	{
		PartUnitBody optional = Unit.Value.GetOptional<PartUnitBody>();
		if (optional == null)
		{
			return;
		}
		foreach (BlueprintAugmentSlot key in optional.Augments.Slots.Keys)
		{
			bool hasTrauma = false;
			List<BlueprintBuff> traumaList = UIConfig.Instance.UIAugmentationsTraumaSlotReferences.GetTraumaList(key);
			if (traumaList != null && traumaList.Any((BlueprintBuff traumaBuff) => Unit.Value.Buffs.Contains(traumaBuff)))
			{
				hasTrauma = true;
			}
			AugmentationsSlotVM augmentationsSlotVM = new AugmentationsSlotVM(Unit.Value, key, optional.Augments.GetOrCreateSlot(key), -1, AugmentsViewOnly, isOverchargeSlot: false, hasTrauma);
			AddDisposable(augmentationsSlotVM.IsDirty.Subscribe(delegate(bool isDirty)
			{
				HaveDirtySlot = isDirty;
			}));
			AllAugmentSlots.Add(augmentationsSlotVM);
			AddDisposable(augmentationsSlotVM);
		}
		EmptyAugmentSlotVM = new AugmentationsSlotVM(Unit.Value, isOverchargeSlot: true, AugmentsViewOnly);
		AllAugmentSlots.Add(EmptyAugmentSlotVM);
		AddDisposable(EmptyAugmentSlotVM);
	}

	private void RefreshMetallization()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			Buff buff = Unit.Value.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.MetallicizationBuff);
			Metallization.Value = ((buff != null) ? buff.Rank.ToString() : "0");
			StashVM.UpdateMetallizationBuffTooltip.Execute();
		}, 4);
	}

	private void ClearSlots()
	{
		AllAugmentSlots.ForEach(delegate(AugmentationsSlotVM vm)
		{
			vm.Dispose();
		});
		AllAugmentSlots.Clear();
	}

	public void HandleUnequipItem()
	{
	}

	public void HandleAugmentUnequip()
	{
		if (AllAugmentSlots.FirstOrDefault((AugmentationsSlotVM vm) => vm.IsOverchargeSlot) != null)
		{
			EventBus.RaiseEvent(delegate(IForceRebindOverdriveBlock h)
			{
				h.HandleForceRebindOverdriveBlock();
			});
		}
	}

	private void CheckIfUnitIsForgeWorld(BaseUnitEntity unit)
	{
		if (unit != null)
		{
			BlueprintFeature blueprint = UIConfig.Instance.ForgeworldHomeworldReference.Get();
			IsForgeWorldUnit.Value = unit.Facts.Contains(blueprint);
		}
	}

	private void CheckIfUnitIsPascal(BaseUnitEntity unit)
	{
		IsPasqal.Value = Unit.Value.Facts.Get(UIConfig.Instance.PasqalOccupationReference.Get()) != null;
	}

	private void CheckIfUnitIsManipulus(BaseUnitEntity unit)
	{
		IsManipulus.Value = Unit.Value.Facts.Get(UIConfig.Instance.ManipulusOccupationReference.Get()) != null;
	}

	private void CheckUnitSize()
	{
		AugmentationsPlayerSize value = ((Unit.Value.Name != null && (Unit.Value.Name.Contains("Manipulus") || Unit.Value.Name.Contains("Ulfar"))) ? AugmentationsPlayerSize.Big : AugmentationsPlayerSize.Regular);
		foreach (AugmentationsSlotVM allAugmentSlot in AllAugmentSlots)
		{
			allAugmentSlot.PlayerSize.Value = value;
		}
	}

	public void HandleAugmentOverdriveToggle(BlueprintAugmentSlot currentSlot, int layer)
	{
		UISounds.Instance.Sounds.AugmentationsWindow.AugmentOverdrive.Play();
		RefreshOverchargeSlotData();
	}

	void IAugmentOverchargeSlotDragHandler.HandleOverchargeSlotDragStart()
	{
		m_IsOverchargeSlotDragInProgress = true;
	}

	void IAugmentOverchargeSlotDragHandler.HandleOverchargeSlotDragEnd()
	{
		if (m_IsOverchargeSlotDragInProgress)
		{
			m_IsOverchargeSlotDragInProgress = false;
			TryDisableOverdriveFromDrag();
		}
	}

	private void TryEnableOverdriveFromDrag(AugmentationsSlotVM fromSlot)
	{
		if (Unit.Value != null && Unit.Value.CanBeControlled() && !Game.Instance.TurnController.InCombat && !fromSlot.IsDirty.Value && fromSlot.ItemSlot is AugmentSlot augmentSlot && augmentSlot.ItemBlueprint?.OverdriveAbility != null)
		{
			UISounds.Instance.Sounds.AugmentationsWindow.AugmentOverdrive.Play();
			Game.Instance.GameCommandQueue.SetAugmentOverdrive(Unit.Value, fromSlot.BlueprintAugmentSlot);
		}
	}

	private void TryDisableOverdriveFromDrag()
	{
		if (Unit.Value != null && Unit.Value.CanBeControlled() && !Game.Instance.TurnController.InCombat && UnitAugments?.OverdriveSlot != null)
		{
			UISounds.Instance.Sounds.AugmentationsWindow.AugmentOverdrive.Play();
			Game.Instance.GameCommandQueue.SetAugmentOverdrive(Unit.Value, null);
		}
	}

	private void RefreshOverchargeSlotData()
	{
		if (Unit.Value.GetOptional<PartUnitBody>() != null)
		{
			AllAugmentSlots.FirstOrDefault((AugmentationsSlotVM vm) => vm.IsOverchargeSlot)?.Dispose();
		}
	}

	public void StartDrag()
	{
		HideLines();
	}

	public void EndDrag()
	{
	}

	public void HideLines()
	{
		AllAugmentSlots.ForEach(delegate(AugmentationsSlotVM s)
		{
			s.HideLines();
		});
	}

	public void ShowLines()
	{
		AllAugmentSlots.ForEach(delegate(AugmentationsSlotVM s)
		{
			s.ShowLines();
		});
	}

	public void ChooseSlotToItem(InventorySlotConsoleView item)
	{
		List<AugmentationsSlotVM> list = AllAugmentSlots.Where((AugmentationsSlotVM slotVM) => slotVM.IsPossibleTarget(item.SlotVM.ItemEntity)).ToList();
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

	private void ShowSelectorWindow(AugmentationsSlotVM slot, List<EquipSelectorSlotVM> possibleItems)
	{
		InventorySelectorWindowVM.Value?.Dispose();
		InventorySelectorWindowVM.Value = new AugmentationsSelectorWindowVM(delegate(EquipSelectorSlotVM x)
		{
			slot.InsertItem(x.Item);
			HideSelectionWindow();
		}, HideSelectionWindow, possibleItems, slot);
	}

	private static List<EquipSelectorSlotVM> SetupSlots(ItemsCollection itemsCollection, ItemSlotVM slot, ItemsSorterType sorterType, ItemsFilterType filterType)
	{
		AugmentationsSlotVM augmentationsSlotVM = slot as AugmentationsSlotVM;
		if (augmentationsSlotVM == null)
		{
			return null;
		}
		List<ItemEntity> list = ItemsFilter.ItemSorter(itemsCollection.ToList(), sorterType, filterType);
		list.RemoveAll((ItemEntity item) => item != null && !ItemsFilter.ShouldShowItem(item, filterType));
		list.RemoveAll((ItemEntity item) => item.HoldingSlot != null || !augmentationsSlotVM.ItemSlot.PossibleEquipItem(item));
		if (slot.HasItem && slot.Item.Value.Blueprint != augmentationsSlotVM.BlueprintAugmentSlot.DefaultAugment.Get())
		{
			list.Insert(0, slot.ItemEntity);
		}
		return list.Select((ItemEntity item) => new EquipSelectorSlotVM(item)).ToList();
	}

	private void HideSelectionWindow()
	{
		InventorySelectorWindowVM.Value?.Dispose();
		InventorySelectorWindowVM.Value = null;
	}

	public void HandleChangeItem(AugmentationsSlotVM slot)
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
		List<EquipSelectorSlotVM> list = SetupSlots(Game.Instance.Player.Inventory, slot, ItemsSorterType.NameDown, ItemsFilterType.AugmentationsAll);
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

	public void HandleOverdriveSlotUpdate()
	{
		AllAugmentSlots.FirstOrDefault((AugmentationsSlotVM vm) => vm.IsOverchargeSlot)?.RefreshOverchargeAbility.Execute();
	}

	public void HandleOnRotationStop()
	{
		ShowLines();
	}

	public void HandleBuffDidAdded(Buff buff)
	{
		CheckBuffAndUpdate(buff);
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		if (buff.Blueprint == BlueprintRoot.Instance.SystemMechanics.MetallicizationBuff)
		{
			Metallization.Value = "0";
			StashVM.UpdateMetallizationBuffTooltip.Execute();
		}
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
		CheckBuffAndUpdate(buff);
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
		CheckBuffAndUpdate(buff);
	}

	public void HandleBuffIsSuppressedChanged(Buff buff)
	{
		if (buff.IsSuppressed)
		{
			HandleBuffDidRemoved(buff);
		}
		else
		{
			HandleBuffDidAdded(buff);
		}
	}

	private void CheckBuffAndUpdate(Buff buff)
	{
		if (buff.Blueprint == BlueprintRoot.Instance.SystemMechanics.MetallicizationBuff)
		{
			Metallization.Value = buff.Rank.ToString();
			StashVM.UpdateMetallizationBuffTooltip.Execute();
		}
	}

	public void HandleAugmentActivateOverdrive(BaseUnitEntity owner)
	{
		if (Unit.Value == owner)
		{
			HandleOverdriveLabelsUpdate.Execute();
			EventBus.RaiseEvent(delegate(IAugmentSlotRefreshHandler h)
			{
				h.HandleAugmentSlotRefresh();
			});
		}
	}

	public void HandleAugmentDeactivateOverdrive(BaseUnitEntity owner)
	{
		if (Unit.Value == owner)
		{
			HandleOverdriveLabelsUpdate.Execute();
			EventBus.RaiseEvent(delegate(IAugmentSlotRefreshHandler h)
			{
				h.HandleAugmentSlotRefresh();
			});
		}
	}

	public bool IsInSpace()
	{
		if (!Game.Instance.IsModeActive(GameModeType.StarSystem))
		{
			return Game.Instance.IsModeActive(GameModeType.GlobalMap);
		}
		return true;
	}

	private bool IsOnBridge()
	{
		BlueprintArea blueprintArea = Game.Instance.LoadedAreaState?.Blueprint;
		if (blueprintArea != null)
		{
			return UIConfig.Instance.VoidshipBridgeAreaReference.Get() == blueprintArea;
		}
		return false;
	}

	private bool IsOnMedicare()
	{
		BlueprintArea blueprintArea = Game.Instance.LoadedAreaState?.Blueprint;
		if (blueprintArea != null)
		{
			return UIConfig.Instance.MedicareAreaReference.Get() == blueprintArea;
		}
		return false;
	}

	private bool IsAugmentsTierIsNone()
	{
		return Game.Instance.Player.PartyAugmentManager.CurrentAvailableTier == AugmentTier.None;
	}

	private void ProcessNotInstalledAugmentRemoval(BaseUnitEntity unit)
	{
		if (!unit.CanBeControlled())
		{
			return;
		}
		foreach (AugmentationsSlotVM allAugmentSlot in AllAugmentSlots)
		{
			if (allAugmentSlot.IsDirty.Value && !allAugmentSlot.IsOverchargeSlot && !allAugmentSlot.IsDefault)
			{
				Game.Instance.GameCommandQueue.UnequipItem(unit, allAugmentSlot.ToSlotRef(), null);
			}
		}
	}

	public void RemoveNotInstalled()
	{
		ProcessNotInstalledAugmentRemoval(Unit.Value);
	}

	private void RemovePreviousNotInstalled()
	{
		ProcessNotInstalledAugmentRemoval(PreviousUnit);
	}

	public void SelectNextCharacter()
	{
		SelectCharacter(1);
	}

	public void SelectPrevCharacter()
	{
		SelectCharacter(-1);
	}

	private void SelectCharacter(int shift)
	{
		List<BaseUnitEntity> actualGroup = Game.Instance.SelectionCharacter.ActualGroup;
		if (actualGroup.Count > 1)
		{
			int num = (actualGroup.IndexOf(Unit.Value) + shift) % actualGroup.Count;
			if (num < 0)
			{
				num += actualGroup.Count;
			}
			Game.Instance.SelectionCharacter.SetSelected(actualGroup[num]);
		}
	}

	private void BarkBanterOnTimerHideHandler()
	{
		if (!(m_BarkBanterHideTimer < 0f))
		{
			m_BarkBanterHideTimer -= Time.deltaTime;
			if (!(m_BarkBanterHideTimer >= 0f) && StarSystemSpaceBarksHolderVM != null && StarSystemSpaceBarksHolderVM.BarksVMs != null)
			{
				StarSystemSpaceBarksHolderVM.RequestBlockHide();
			}
		}
	}
}

using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.PC;

public class InventorySlotPCView : InventorySlotView
{
	[Header("PC")]
	[SerializeField]
	protected ItemSlotPCView m_ItemSlotPCView;

	private ContextMenuCollectionEntity m_ToCargoAuto = new ContextMenuCollectionEntity();

	private ContextMenuCollectionEntity m_AddRemoveFromFavorites = new ContextMenuCollectionEntity();

	protected override void BindViewImplementation()
	{
		m_ItemSlotPCView.SetMainButtonHoverSound(UISounds.ButtonSoundsEnum.NoSound);
		base.BindViewImplementation();
		m_ItemSlotPCView.Bind(base.ViewModel);
		AddDisposable(m_ItemSlotPCView.OnSingleLeftClickAsObservable.Subscribe(OnClick));
		AddDisposable(m_ItemSlotPCView.OnDoubleClickAsObservable.Subscribe(base.OnDoubleClick));
		AddDisposable(m_ItemSlotPCView.OnBeginDragCommand.Subscribe(base.OnBeginDrag));
		AddDisposable(m_ItemSlotPCView.OnEndDragCommand.Subscribe(base.OnEndDrag));
		AddDisposable(m_ItemSlotPCView.OnPointerEnterAsObservable().Subscribe(delegate
		{
			OnHoverStart();
		}));
		AddDisposable(m_ItemSlotPCView.OnPointerExitAsObservable().Subscribe(delegate
		{
			OnHoverEnd();
		}));
		AddDisposable(base.ViewModel.ToCargoAutomaticallyChange.Subscribe(HandleToCargoAutomaticallyChanged));
		AddDisposable(base.ViewModel.IsFavorite.Subscribe(delegate
		{
			SetupContextMenu();
		}));
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		string title = UIStrings.Instance.ContextMenu.AutoAddToCargo;
		Action command = base.ViewModel.AddToCargoAutomatically;
		bool condition = CargoHelper.CanTransferFromCargo(base.ViewModel.Item.Value) && CargoHelper.CanTransferToCargo(base.ViewModel.Item.Value);
		ItemEntity value = base.ViewModel.Item.Value;
		m_ToCargoAuto = new ContextMenuCollectionEntity(title, command, condition, isInteractable: true, (value != null && value.ToCargoAutomatically) ? BlueprintRoot.Instance.UIConfig.UIIcons.Check : BlueprintRoot.Instance.UIConfig.UIIcons.NotCheck);
		value = base.ViewModel.Item.Value;
		LocalizedString title2 = ((value != null && value.IsFavorite) ? UIStrings.Instance.ContextMenu.RemoveFromFav : UIStrings.Instance.ContextMenu.AddToFav);
		bool isInteractable = base.ViewModel.Item != null;
		m_AddRemoveFromFavorites = new ContextMenuCollectionEntity(title2, base.AddRemoveFromFavorites, condition: true, isInteractable);
		bool flag = RootUIContext.Instance.IsInventoryShow && base.ViewModel.ItemEntity?.Blueprint is BlueprintStarshipItem;
		List<ContextMenuCollectionEntity> value2 = new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(contextMenu.Equip, base.EquipItem, base.ViewModel.IsEquipPossible && !flag),
			new ContextMenuCollectionEntity(UIStrings.Instance.LootWindow.SendToCargo, delegate
			{
				MoveToCargo(immediately: true);
			}, condition: true, base.ViewModel.IsInStash && base.ViewModel.CanTransferToCargo),
			new ContextMenuCollectionEntity(UIStrings.Instance.LootWindow.SendToInventory, delegate
			{
				MoveToInventory(immediately: true);
			}, condition: true, base.ViewModel.SlotsGroupType == ItemSlotsGroupType.Cargo && base.ViewModel.CanTransferToInventory),
			m_AddRemoveFromFavorites,
			m_ToCargoAuto,
			new ContextMenuCollectionEntity(contextMenu.Split, base.Split, base.ViewModel.IsPosibleSplit),
			new ContextMenuCollectionEntity(contextMenu.Drop, base.DropItem, base.ViewModel.CanEquip),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		};
		base.ViewModel.ContextMenu.Value = value2;
	}

	protected void HandleToCargoAutomaticallyChanged()
	{
		if (base.ViewModel.Item.Value != null)
		{
			m_ToCargoAuto.SetNewIcon(base.ViewModel.Item.Value.ToCargoAutomatically ? BlueprintRoot.Instance.UIConfig.UIIcons.Check : BlueprintRoot.Instance.UIConfig.UIIcons.NotCheck);
		}
	}

	protected override void CheckChangeSoundsImpl(SetServoSkullItemClickAndHoverSound component)
	{
		if (component == null)
		{
			m_ItemSlotPCView.SetMainButtonClickSound(UISounds.ButtonSoundsEnum.NormalSound);
			m_ItemSlotPCView.SetMainButtonHoverSound(UISounds.ButtonSoundsEnum.NoSound);
			return;
		}
		if (component.SetClickSound)
		{
			m_ItemSlotPCView.SetMainButtonClickSound(UISounds.ButtonSoundsEnum.ServoSkullTwitchDrops);
		}
		m_ItemSlotPCView.SetMainButtonHoverSound(component.SetHoverSound ? UISounds.ButtonSoundsEnum.ServoSkullTwitchDrops : UISounds.ButtonSoundsEnum.NoSound);
	}
}

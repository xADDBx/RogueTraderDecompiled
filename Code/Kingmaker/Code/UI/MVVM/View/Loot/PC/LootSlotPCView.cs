using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.PC;

public class LootSlotPCView : LootSlotView
{
	[SerializeField]
	private ItemSlotPCView m_ItemSlotPCView;

	private ContextMenuCollectionEntity m_ToCargoAuto = new ContextMenuCollectionEntity();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotPCView.Bind(base.ViewModel);
		AddDisposable(m_ItemSlotPCView.OnSingleLeftClickAsObservable.Subscribe(base.OnClick));
		AddDisposable(m_ItemSlotPCView.OnBeginDragCommand.Subscribe(OnBeginDrag));
		AddDisposable(m_ItemSlotPCView.OnEndDragCommand.Subscribe(OnEndDrag));
		AddDisposable(base.ViewModel.ToCargoAutomaticallyChange.Subscribe(HandleToCargoAutomaticallyChanged));
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		string title = UIStrings.Instance.ContextMenu.AutoAddToCargo;
		Action command = base.ViewModel.AddToCargoAutomatically;
		bool condition = CanTransfer(base.ViewModel.Item.Value) && CargoHelper.CanTransferToCargo(base.ViewModel.Item.Value);
		ItemEntity value = base.ViewModel.Item.Value;
		m_ToCargoAuto = new ContextMenuCollectionEntity(title, command, condition, isInteractable: true, (value != null && value.ToCargoAutomatically) ? BlueprintRoot.Instance.UIConfig.UIIcons.Check : BlueprintRoot.Instance.UIConfig.UIIcons.NotCheck);
		List<ContextMenuCollectionEntity> value2 = new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(UIStrings.Instance.LootWindow.SendToCargo, delegate
			{
				MoveToCargo(immediately: true);
			}, condition: true, base.ViewModel.CanTransferToCargo),
			new ContextMenuCollectionEntity(UIStrings.Instance.LootWindow.SendToInventory, delegate
			{
				MoveToInventory(immediately: true);
			}, condition: true, base.ViewModel.CanTransferToInventory),
			m_ToCargoAuto,
			new ContextMenuCollectionEntity(contextMenu.Split, base.Split, base.ViewModel.IsPosibleSplit),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		};
		base.ViewModel.ContextMenu.Value = value2;
	}

	private bool CanTransfer(ItemEntity item)
	{
		if (CargoHelper.IsItemInCargo(item))
		{
			if (CargoHelper.CanTransferFromCargo(item))
			{
				return true;
			}
			return false;
		}
		if (item != null)
		{
			return !CargoHelper.IsTrashItem(item);
		}
		return false;
	}

	protected void HandleToCargoAutomaticallyChanged()
	{
		if (base.ViewModel.Item.Value != null)
		{
			m_ToCargoAuto.SetNewIcon(base.ViewModel.Item.Value.ToCargoAutomatically ? BlueprintRoot.Instance.UIConfig.UIIcons.Check : BlueprintRoot.Instance.UIConfig.UIIcons.NotCheck);
		}
	}

	protected void OnBeginDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStart(base.Item);
		});
	}

	protected void OnEndDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStop();
		});
	}
}

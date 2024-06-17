using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class InventorySlotConsoleView : InventorySlotView, IConfirmClickHandler, IConsoleEntity, IConsoleNavigationEntity, IHasTooltipTemplates
{
	[Header("Console")]
	[SerializeField]
	private ItemSlotConsoleView m_ItemSlotConsoleView;

	private ContextMenuCollectionEntity m_ToCargoAuto = new ContextMenuCollectionEntity();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotConsoleView.Bind(base.ViewModel);
		AddDisposable(base.ViewModel.ToCargoAutomaticallyChange.Subscribe(HandleToCargoAutomaticallyChanged));
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		string title = UIStrings.Instance.ContextMenu.AutoAddToCargo;
		Action command = base.ViewModel.AddToCargoAutomatically;
		bool condition = CargoHelper.CanTransferFromCargo(base.ViewModel.Item.Value) && CargoHelper.CanTransferToCargo(base.ViewModel.Item.Value);
		ItemEntity value = base.ViewModel.Item.Value;
		m_ToCargoAuto = new ContextMenuCollectionEntity(title, command, condition, isInteractable: true, (value != null && value.ToCargoAutomatically) ? BlueprintRoot.Instance.UIConfig.UIIcons.Check : BlueprintRoot.Instance.UIConfig.UIIcons.NotCheck);
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
			m_ToCargoAuto,
			new ContextMenuCollectionEntity(contextMenu.Split, base.Split, base.ViewModel.IsPosibleSplit),
			new ContextMenuCollectionEntity(contextMenu.Drop, base.DropItem, base.ViewModel.CanEquip),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		};
		base.ViewModel.ContextMenu.Value = value2;
	}

	public void SetFocus(bool value)
	{
		if (value)
		{
			OnHoverStart();
		}
		else
		{
			OnHoverEnd();
		}
		m_ItemSlotConsoleView.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_ItemSlotConsoleView.IsValid();
	}

	public bool CanConfirmClick()
	{
		if (RootUIContext.Instance.IsInventoryShow)
		{
			if (base.ViewModel.HasItem)
			{
				return base.ViewModel.IsEquipPossible;
			}
			return false;
		}
		return base.ViewModel.HasItem;
	}

	public void OnConfirmClick()
	{
		if (RootUIContext.Instance.IsInventoryShow || RootUIContext.Instance.IsShipInventoryShown)
		{
			m_ItemSlotConsoleView.SetWaitingForSlotState(state: true);
			EventBus.RaiseEvent(delegate(IEquipSlotHandler h)
			{
				h.ChooseSlotToItem(this);
			});
		}
		else
		{
			OnClick();
		}
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel.Tooltip.Value;
	}

	public void ReleaseSlot()
	{
		m_ItemSlotConsoleView.SetWaitingForSlotState(state: false);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_ItemSlotConsoleView.SetFocus(value: false);
	}

	protected void HandleToCargoAutomaticallyChanged()
	{
		if (base.ViewModel.Item.Value != null)
		{
			m_ToCargoAuto.SetNewIcon(base.ViewModel.Item.Value.ToCargoAutomatically ? BlueprintRoot.Instance.UIConfig.UIIcons.Check : BlueprintRoot.Instance.UIConfig.UIIcons.NotCheck);
		}
	}
}

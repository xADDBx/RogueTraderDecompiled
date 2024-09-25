using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Items;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.Console;

public class LootSlotConsoleView : LootSlotView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplates
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

	public void SetFocus(bool value)
	{
		m_ItemSlotConsoleView.SetFocus(value);
	}

	private bool CanTransfer(ItemEntity item)
	{
		if (!CargoHelper.IsItemInCargo(item))
		{
			if (item != null)
			{
				return !CargoHelper.IsTrashItem(item);
			}
			return false;
		}
		return CargoHelper.CanTransferFromCargo(item);
	}

	public bool IsValid()
	{
		return m_ItemSlotConsoleView.IsValid();
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel?.HasItem ?? false;
	}

	public void OnConfirmClick()
	{
		OnClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return m_ItemSlotConsoleView.SlotVM?.Tooltip?.Value;
	}

	protected void HandleToCargoAutomaticallyChanged()
	{
		if (base.ViewModel.Item.Value != null)
		{
			m_ToCargoAuto.SetNewIcon(base.ViewModel.Item.Value.ToCargoAutomatically ? BlueprintRoot.Instance.UIConfig.UIIcons.Check : BlueprintRoot.Instance.UIConfig.UIIcons.NotCheck);
		}
	}
}

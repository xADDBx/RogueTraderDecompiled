using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class InventoryEquipSlotConsoleView : InventoryEquipSlotView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplates
{
	[SerializeField]
	private ItemSlotConsoleView m_ItemSlotConsoleView;

	public void SetAvailable(bool value)
	{
		m_ItemSlotConsoleView.SetAvailable(value);
	}

	public void SetSelected(bool value)
	{
		m_ItemSlotConsoleView.SetSelected(value);
	}

	public void SetFocus(bool value)
	{
		m_ItemSlotConsoleView.SetSelected(value);
	}

	public bool IsValid()
	{
		return m_ItemSlotConsoleView.IsValid();
	}

	public bool CanConfirmClick()
	{
		return m_ItemSlotConsoleView.IsValid();
	}

	public void OnConfirmClick()
	{
		switch (UsableSource)
		{
		case UsableSourceType.Inventory:
			EventBus.RaiseEvent(delegate(IInventoryItemHandler h)
			{
				h.HandleChangeItem(base.ViewModel);
			});
			break;
		case UsableSourceType.Vendor:
			base.ViewModel.VendorTryMove(split: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel?.Tooltip?.Value ?? new List<TooltipBaseTemplate>();
	}
}

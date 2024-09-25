using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.Console;

public class InsertableLootSlotConsoleView : InsertableLootSlotView, IConfirmClickHandler, IConsoleEntity, IConsoleNavigationEntity, IHasTooltipTemplates
{
	[Header("Console")]
	[SerializeField]
	private ItemSlotConsoleView m_ItemSlotConsoleView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotConsoleView.Bind(base.ViewModel);
	}

	public void SetFocus(bool value)
	{
		m_ItemSlotConsoleView.SetFocus(value);
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
		OnClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel.Tooltip.Value;
	}
}

using Kingmaker.Code.UI.MVVM.View.ActionBar.Console;
using Kingmaker.UI.MVVM.View.SpaceCombat.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Console;

public class ShipAbilitySlotConsoleView : ShipAbilitySlotBaseView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplate
{
	[Header("ConsoleSlot")]
	[SerializeField]
	private ActionBarSlotConsoleView m_SlotConsoleView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_SlotConsoleView.Bind(base.ViewModel);
	}

	public void SetFocus(bool value)
	{
		m_SlotConsoleView.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_SlotConsoleView.IsValid();
	}

	public bool CanConfirmClick()
	{
		return m_SlotConsoleView.CanConfirmClick();
	}

	public void OnConfirmClick()
	{
		m_SlotConsoleView.OnConfirmClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.Value;
	}
}

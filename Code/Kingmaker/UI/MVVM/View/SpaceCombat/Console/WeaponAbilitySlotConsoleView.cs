using Kingmaker.Code.UI.MVVM.View.ActionBar;
using Kingmaker.Code.UI.MVVM.View.ActionBar.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Console;

public class WeaponAbilitySlotConsoleView : ActionBarBaseSlotView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IFunc02ClickHandler, IHasTooltipTemplate
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

	public bool CanFunc02Click()
	{
		return base.ViewModel.HasConvert.Value;
	}

	public void OnFunc02Click()
	{
		m_SlotConsoleView.ShowConvertRequest();
	}

	public string GetFunc02ClickHint()
	{
		return string.Empty;
	}
}

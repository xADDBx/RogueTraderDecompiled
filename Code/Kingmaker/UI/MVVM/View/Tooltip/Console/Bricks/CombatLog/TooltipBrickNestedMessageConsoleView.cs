using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.Console.Bricks.CombatLog;

public class TooltipBrickNestedMessageConsoleView : TooltipBrickNestedMessageView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_FocusMultiButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetFocus(value: false);
	}

	public void SetFocus(bool value)
	{
		m_HighlightCanvasGroup.alpha = (value ? 1f : 0f);
	}

	public bool IsValid()
	{
		return true;
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel.Unit != null;
	}

	public void OnConfirmClick()
	{
		OnConfirm();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return new SimpleConsoleNavigationEntity(m_FocusMultiButton, base.ViewModel.TooltipTemplate);
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}

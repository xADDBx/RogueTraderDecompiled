using System;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoFeatureConsoleView : CharInfoFeatureBaseView, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate, IConfirmClickHandler
{
	private Action<Ability> m_OnClick;

	public void SetupClickAction(Action<Ability> onClick)
	{
		m_OnClick = onClick;
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.Value;
	}

	public bool CanConfirmClick()
	{
		if (m_Button.IsValid())
		{
			return m_OnClick != null;
		}
		return false;
	}

	public void OnConfirmClick()
	{
		m_OnClick?.Invoke(base.ViewModel.Ability);
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}

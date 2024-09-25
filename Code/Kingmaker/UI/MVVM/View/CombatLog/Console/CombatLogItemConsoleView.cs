using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CombatLog.Console;

public class CombatLogItemConsoleView : CombatLogItemBaseView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	private CanvasGroup m_FocusCanvasGroup;

	[SerializeField]
	private CanvasGroup m_HighlightCanvasGroup;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 17f;

	public TooltipBaseTemplate TooltipTemplate => base.ViewModel.TooltipTemplate;

	protected override void BindViewImplementation()
	{
		SetFocus(value: false);
		base.BindViewImplementation();
		SetTextFontSize();
	}

	public void SetFocus(bool value)
	{
		m_FocusCanvasGroup.alpha = (value ? 1f : 0f);
		m_HighlightCanvasGroup.alpha = (value ? 1f : 0f);
	}

	public bool IsValid()
	{
		return true;
	}

	private void SetTextFontSize()
	{
		m_Text.fontSize = m_DefaultConsoleFontSize * base.ViewModel.FontSizeMultiplier;
	}

	public override void UpdateTextSize(float multiplier)
	{
		m_Text.fontSize = m_DefaultConsoleFontSize * multiplier;
		base.UpdateTextSize(multiplier);
	}

	public bool CanConfirmClick()
	{
		return base.ViewModel.Unit != null;
	}

	public void OnConfirmClick()
	{
		Game.Instance.CameraController?.Follower?.ScrollTo(base.ViewModel.Unit);
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}

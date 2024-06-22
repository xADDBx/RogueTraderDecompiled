using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoFeatureConsoleView : CharInfoFeatureSimpleBaseView, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate, IConfirmClickHandler, ICharInfoAbilitiesChooseModeHandler, ISubscriber
{
	[SerializeField]
	protected OwlcatMultiSelectable m_Button;

	[SerializeField]
	private OwlcatMultiSelectable m_FeatureSelectable;

	private Action<CharInfoFeatureConsoleView> m_OnClick;

	private Action<CharInfoFeatureConsoleView> m_OnFocus;

	private bool m_ChooseAbilityMode;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EventBus.Subscribe(this));
	}

	public void SetupChooseModeActions(Action<CharInfoFeatureConsoleView> onClick, Action<CharInfoFeatureConsoleView> onFocus)
	{
		m_OnClick = onClick;
		m_OnFocus = onFocus;
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
		if (value && m_ChooseAbilityMode)
		{
			m_OnFocus?.Invoke(this);
		}
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
		m_OnClick?.Invoke(this);
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public void SetMoveState(bool state)
	{
		int activeLayer = (state ? 1 : 0);
		m_FeatureSelectable.Or(null)?.SetActiveLayer(activeLayer);
	}

	public void HandleChooseMode(bool active)
	{
		m_ChooseAbilityMode = active;
	}
}

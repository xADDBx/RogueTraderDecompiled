using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression.SelectionTabs;

public class AvailableTalentsDropDownCommonView : VirtualListElementViewBase<AvailableTalentsDropDownVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private ExpandableCollapseMultiButtonPC m_ExpandableElement;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		if (m_Label != null)
		{
			m_Label.text = base.ViewModel.Title;
		}
		if (!(m_ExpandableElement != null))
		{
			return;
		}
		m_ExpandableElement.SetValue(base.ViewModel.IsExpanded.Value, isImmediately: true);
		AddDisposable(m_ExpandableElement.IsOn.Subscribe(delegate(bool isOn)
		{
			if (isOn != base.ViewModel.IsExpanded.Value)
			{
				base.ViewModel.Switch();
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		m_ExpandableElement?.SetFocus(value);
	}

	public bool IsValid()
	{
		if (m_ExpandableElement != null)
		{
			return m_ExpandableElement.IsValid();
		}
		return false;
	}

	public bool CanConfirmClick()
	{
		if (m_ExpandableElement != null)
		{
			return m_ExpandableElement.IsValid();
		}
		return false;
	}

	public void OnConfirmClick()
	{
		m_ExpandableElement?.OnConfirmClick();
	}

	public string GetConfirmClickHint()
	{
		return (base.ViewModel?.IsExpanded?.Value).GetValueOrDefault() ? UIStrings.Instance.CommonTexts.Collapse : UIStrings.Instance.CommonTexts.Expand;
	}
}

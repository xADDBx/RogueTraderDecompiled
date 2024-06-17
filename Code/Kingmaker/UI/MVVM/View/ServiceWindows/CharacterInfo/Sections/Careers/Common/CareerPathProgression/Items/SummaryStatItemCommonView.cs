using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public class SummaryStatItemCommonView : VirtualListElementViewBase<RankEntrySelectionStatVM>, IWidgetView, IConfirmClickHandler, IConsoleEntity, IConsoleNavigationEntity, IHasTooltipTemplate
{
	[SerializeField]
	private TextMeshProUGUI m_StatDisplayName;

	[SerializeField]
	private TextMeshProUGUI m_StatIncreaseText;

	[SerializeField]
	private Image m_RecommendMark;

	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_StatDisplayName.text = base.ViewModel.StatDisplayName;
		if (m_RecommendMark != null)
		{
			m_RecommendMark.gameObject.SetActive(base.ViewModel.IsRecommended);
			AddDisposable(m_RecommendMark.SetHint(UIStrings.Instance.CharacterSheet.RecommendedByCareerPath));
		}
		AddDisposable(base.ViewModel.SummaryStatIncreaseLabel.Subscribe(delegate(string value)
		{
			m_StatIncreaseText.text = value;
		}));
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as RankEntrySelectionStatVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is RankEntrySelectionStatVM;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.Value;
	}

	public bool CanConfirmClick()
	{
		return false;
	}

	public void OnConfirmClick()
	{
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return true;
	}
}

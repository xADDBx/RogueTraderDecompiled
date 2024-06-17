using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public class SummaryUltimateFeatureUpgradeItemCommonView : VirtualListElementViewBase<BaseRankEntryFeatureVM>, IWidgetView, IConfirmClickHandler, IConsoleEntity, IConsoleNavigationEntity, IHasTooltipTemplate
{
	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_Description.text = base.ViewModel.FactDescription;
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.Tooltip));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as RankEntrySelectionFeatureVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is RankEntrySelectionFeatureVM;
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

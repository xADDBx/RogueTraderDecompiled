using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.ExitBattlePopup;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ExitBattlePopup.Base;

public class ScrapRewardSlotView : ViewBase<ScrapRewardSlotVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	private GameObject m_Container;

	[SerializeField]
	private TextMeshProUGUI m_ScrapTitle;

	[SerializeField]
	private TextMeshProUGUI m_ScrapAmount;

	public MonoBehaviour MonoBehaviour => this;

	public void Initialize()
	{
		m_ScrapTitle.text = UIStrings.Instance.ShipCustomization.Scrap;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Amount.Subscribe(SetAmount));
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None)));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetAmount(int amount)
	{
		m_Container.SetActive(amount > 0);
		m_ScrapAmount.text = "+" + amount;
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ScrapRewardSlotVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ScrapRewardSlotVM;
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.Value;
	}
}

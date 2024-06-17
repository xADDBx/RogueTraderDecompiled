using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.Colonization;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class ColonyRewardsOtherRewardView : ViewBase<ColonyRewardsOtherRewardVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private TextMeshProUGUI m_CountText;

	private TooltipBaseTemplate m_Tooltip;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(SetIcon));
		AddDisposable(base.ViewModel.Description.Subscribe(delegate(string value)
		{
			m_Description.text = value;
		}));
		AddDisposable(base.ViewModel.CountText.Subscribe(SetCountText));
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None)));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void SetIcon(Sprite sprite)
	{
		if (sprite == null)
		{
			m_Icon.gameObject.SetActive(value: false);
			return;
		}
		m_Icon.sprite = sprite;
		m_Icon.gameObject.SetActive(value: true);
	}

	private void SetCountText(string text)
	{
		if (text == null)
		{
			m_CountText.gameObject.SetActive(value: false);
			return;
		}
		m_CountText.text = text;
		m_CountText.gameObject.SetActive(value: true);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as ColonyRewardsOtherRewardVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ColonyRewardsOtherRewardVM;
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

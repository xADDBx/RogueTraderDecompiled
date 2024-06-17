using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
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

public class CargoRewardSlotView : ViewBase<CargoRewardSlotVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	private Image m_TypeIcon;

	[SerializeField]
	private TextMeshProUGUI m_FillValue;

	[SerializeField]
	private TextMeshProUGUI m_Count;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_TypeIcon.sprite = base.ViewModel.TypeIcon;
		AddDisposable(base.ViewModel.TotalFillValue.Subscribe(delegate(int value)
		{
			m_FillValue.text = $"{value}%";
		}));
		AddDisposable(base.ViewModel.Count.Subscribe(delegate(int value)
		{
			m_Count.text = $"x{value}";
		}));
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None)));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CargoRewardSlotVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CargoRewardSlotVM;
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

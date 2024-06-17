using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoSlotBaseView : VirtualListElementViewBase<CargoSlotVM>, IHasTooltipTemplate
{
	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	private Image m_TypeIcon;

	[SerializeField]
	private TextMeshProUGUI m_TypeLabel;

	[SerializeField]
	private TextMeshProUGUI m_FillValue;

	[SerializeField]
	protected GameObject m_FilledCargoMark;

	protected override void BindViewImplementation()
	{
		m_TypeIcon.sprite = base.ViewModel.TypeIcon;
		m_TypeLabel.text = base.ViewModel.TypeLabel;
		m_MainButton.SetActiveLayer(base.ViewModel.IsEmpty ? "Empty" : "Active");
		m_MainButton.SetInteractable(!base.ViewModel.IsEmpty);
		m_TypeIcon.gameObject.SetActive(!base.ViewModel.IsEmpty);
		m_TypeLabel.gameObject.SetActive(!base.ViewModel.IsEmpty);
		m_FillValue.gameObject.SetActive(!base.ViewModel.IsEmpty);
		AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.HandleClick();
		}));
		AddDisposable(base.ViewModel.TotalFillValue.Subscribe(CargoFilling));
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.LeftMouseButton)));
	}

	protected void CargoFilling(int value)
	{
		m_FilledCargoMark.SetActive(value >= 100);
		m_FillValue.text = $"{value}%";
	}

	protected override void DestroyViewImplementation()
	{
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip;
	}
}

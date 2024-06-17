using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorCargoSlotView : VirtualListElementViewBase<CargoSlotVM>, IHasTooltipTemplate
{
	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	protected Image m_TypeIcon;

	[SerializeField]
	protected TextMeshProUGUI m_TypeLabel;

	[SerializeField]
	protected TextMeshProUGUI m_FillValue;

	[Header("Little Cube Part")]
	[SerializeField]
	protected RectTransform m_CubeInteractable;

	[SerializeField]
	protected OwlcatButton m_CubeInteractableButton;

	[SerializeField]
	protected Image m_CubeFrame;

	[SerializeField]
	protected Image m_CubeIcon;

	[SerializeField]
	protected Image m_NewStateIcon;

	[SerializeField]
	protected TextMeshProUGUI m_SelectText;

	protected bool IsChecked => base.ViewModel.IsChecked;

	protected bool CanCheck => base.ViewModel.CanCheck;

	protected override void BindViewImplementation()
	{
		m_TypeIcon.sprite = base.ViewModel.TypeIcon;
		m_TypeLabel.text = base.ViewModel.TypeLabel;
		m_MainButton.SetInteractable(!base.ViewModel.IsEmpty);
		m_TypeIcon.gameObject.SetActive(!base.ViewModel.IsEmpty);
		m_TypeLabel.gameObject.SetActive(!base.ViewModel.IsEmpty);
		m_FillValue.gameObject.SetActive(!base.ViewModel.IsEmpty);
		AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.HandleClick));
		AddDisposable(m_MainButton.OnHoverAsObservable().Subscribe(delegate
		{
			SetNotNewState();
		}));
		AddDisposable(base.ViewModel.TotalFillValue.Subscribe(delegate(int value)
		{
			m_FillValue.text = $"{value}%";
			AvailableToSellOrNot();
		}));
		AddDisposable(m_MainButton.OnHoverAsObservable().Subscribe(ShowInteractableCubeSize));
		AddDisposable(m_CubeInteractableButton.OnHoverAsObservable().Subscribe(ShowInteractableCubeSize));
		AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.HandleCheck));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.OnCheck, delegate
		{
			SetCheckForSale();
		}));
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.LeftMouseButton)));
		m_NewStateIcon.gameObject.SetActive(base.ViewModel.IsNew);
		m_CubeIcon.gameObject.SetActive(value: false);
		AvailableToSellOrNot();
		SetCheckForSale();
	}

	private void AvailableToSellOrNot()
	{
		m_CubeFrame.color = new Color(0.2784314f, 0.4823529f, 0.2901961f, 1f);
		m_CubeInteractable.gameObject.SetActive(value: true);
		if (!base.ViewModel.IsEmpty)
		{
			m_CubeInteractable.gameObject.SetActive(value: true);
			if (CanCheck)
			{
				m_CubeIcon.gameObject.SetActive(value: false);
			}
			m_MainButton.SetActiveLayer((!CanCheck) ? 2 : 0);
		}
		else
		{
			m_CubeInteractable.gameObject.SetActive(value: false);
		}
	}

	protected void SetCheckForSale()
	{
		if (!CanCheck)
		{
			return;
		}
		if (IsChecked)
		{
			if (m_SelectText.gameObject.activeSelf)
			{
				m_SelectText.gameObject.SetActive(value: false);
			}
			m_CubeFrame.color = new Color(0.5607843f, 0.8392157f, 0.5372549f, 1f);
			m_CubeIcon.gameObject.SetActive(value: true);
		}
		else
		{
			if (!m_SelectText.gameObject.activeSelf)
			{
				m_SelectText.gameObject.SetActive(value: true);
			}
			m_CubeFrame.color = new Color(0.2784314f, 0.4823529f, 0.2901961f, 1f);
			m_CubeIcon.gameObject.SetActive(value: false);
		}
	}

	private void ShowInteractableCubeSize(bool state)
	{
		if (!CanCheck)
		{
			return;
		}
		if (state)
		{
			if (!IsChecked)
			{
				m_SelectText.gameObject.SetActive(value: true);
			}
		}
		else if (!IsChecked && m_SelectText.gameObject.activeSelf)
		{
			m_SelectText.gameObject.SetActive(value: false);
		}
	}

	protected void SetNotNewState()
	{
		m_NewStateIcon.gameObject.SetActive(value: false);
		base.ViewModel.SetNotNewState();
	}

	protected override void DestroyViewImplementation()
	{
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip;
	}
}

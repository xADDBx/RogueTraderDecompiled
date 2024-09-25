using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class InventoryCargoPCView : InventoryCargoView
{
	[SerializeField]
	protected OwlcatMultiButton m_CargoButton;

	[SerializeField]
	protected OwlcatMultiButton m_ListButton;

	[SerializeField]
	private Image m_ListButtonImage;

	[SerializeField]
	private Image m_CargoButtonImage;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_CargoButtonImage != null && m_CargoButtonImage != null)
		{
			if (base.ViewModel.CargoViewType == InventoryCargoViewType.Vendor)
			{
				m_ListButtonImage.gameObject.SetActive(value: true);
				m_CargoButtonImage.gameObject.SetActive(value: true);
			}
			else
			{
				m_ListButtonImage.gameObject.SetActive(value: false);
				m_CargoButtonImage.gameObject.SetActive(value: false);
			}
		}
		base.ViewModel.IsCargoDetailedZone.Value = false;
		if (m_ListButton != null)
		{
			AddDisposable(m_ListButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				ChangeToListView();
			}));
		}
		if (m_CargoButton != null)
		{
			AddDisposable(m_CargoButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				ChangeToCargoView();
			}));
		}
		if (base.ViewModel.CargoViewType == InventoryCargoViewType.Vendor)
		{
			m_CloseButton.Or(null)?.gameObject.SetActive(value: false);
		}
		else if (base.ViewModel.CargoViewType == InventoryCargoViewType.Loot && !RootUIContext.Instance.IsLootShow)
		{
			m_CloseButton.gameObject.SetActive(value: true);
			AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				base.ViewModel.Close();
			}));
		}
		else
		{
			m_CloseButton.gameObject.SetActive(value: false);
			m_CargoZoneView.gameObject.SetActive(value: false);
		}
		if (base.ViewModel.CargoViewType != InventoryCargoViewType.Vendor)
		{
			AddDisposable(base.ViewModel.SelectedCargo.Subscribe(delegate
			{
				ChangeToListView();
			}));
		}
		AddDisposable(base.ViewModel.IsCargoDetailedZone.Subscribe(delegate(bool listMode)
		{
			m_ListButton.Or(null)?.SetActiveLayer(listMode ? "Selected" : "Normal");
			m_CargoButton.Or(null)?.SetActiveLayer((!listMode) ? "Selected" : "Normal");
		}));
	}

	public void ChangeToCargoView()
	{
		if (base.ViewModel.IsCargoDetailedZone.Value && base.ViewModel.HasVisibleCargo.Value)
		{
			base.ViewModel.IsCargoDetailedZone.Value = false;
			m_CargoZoneView.gameObject.SetActive(value: false);
			m_ListContentFadeAnimator.AppearAnimation();
		}
	}

	public void ChangeToListView()
	{
		if (!base.ViewModel.IsCargoDetailedZone.Value && base.ViewModel.HasVisibleCargo.Value)
		{
			base.ViewModel.IsCargoDetailedZone.Value = base.ViewModel.HasVisibleCargo.Value;
			m_CargoZoneView.Bind(base.ViewModel.CargoZoneVM);
			m_CargoZoneView.gameObject.SetActive(base.ViewModel.HasVisibleCargo.Value);
			m_ListContentFadeAnimator.PlayAnimation(!m_CargoZoneView.gameObject.activeSelf);
		}
	}
}

using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorReputationPartPCView : VendorReputationPartView<InventoryCargoPCView, VendorReputationForItemWindowPCView>
{
	[SerializeField]
	protected OwlcatButton m_SelectAllButton;

	[SerializeField]
	protected TextMeshProUGUI m_SelectAllButtonText;

	[SerializeField]
	protected OwlcatButton m_UnselectAllButton;

	[SerializeField]
	protected TextMeshProUGUI m_UnselectButtonText;

	[SerializeField]
	private OwlcatMultiButton m_CargoButton;

	[SerializeField]
	private OwlcatMultiButton m_ListButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.HasItemsToSell.Subscribe(m_SelectAllButton.SetInteractable));
		AddDisposable(base.ViewModel.CanSellCargo.Subscribe(m_UnselectAllButton.SetInteractable));
		AddDisposable(SellButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SellCargo();
		}));
		AddDisposable(m_SelectAllButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SelectAll();
		}));
		AddDisposable(m_UnselectAllButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.UnselectAll();
		}));
		AddDisposable(m_SelectAllButton.SetHint(UIStrings.Instance.Vendor.SelectAllRelevant));
		AddDisposable(m_UnselectAllButton.SetHint(UIStrings.Instance.Vendor.UnselectAllRelevant));
		AddDisposable(base.ViewModel.CanSellCargo.Subscribe(delegate(bool val)
		{
			SellButton.Interactable = val;
		}));
		AddDisposable(base.ViewModel.InventoryCargoVM.HasVisibleCargo.Subscribe(m_CargoButton.SetInteractable));
		AddDisposable(base.ViewModel.InventoryCargoVM.HasVisibleCargo.Subscribe(m_ListButton.SetInteractable));
		m_SelectAllButtonText.text = UIStrings.Instance.Vendor.SelectAllRelevant;
		m_UnselectButtonText.text = UIStrings.Instance.Vendor.UnselectAllRelevant;
	}
}

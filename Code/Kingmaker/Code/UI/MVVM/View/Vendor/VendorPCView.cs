using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorPCView : VendorView<InventoryStashPCView, InventoryCargoPCView, ItemsFilterPCView, VendorLevelItemsPCView, VendorTransitionWindowPCView, VendorReputationForItemWindowPCView>
{
	[SerializeField]
	private OwlcatButton m_CloseButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.Close));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.Close));
	}
}

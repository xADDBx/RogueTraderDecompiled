using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorTabNavigationVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<VendorWindowsTab> ActiveTab = new ReactiveProperty<VendorWindowsTab>(VendorWindowsTab.Trade);

	protected override void DisposeImplementation()
	{
	}

	public void SetActiveTab(VendorWindowsTab activeTab)
	{
		if (ActiveTab.Value != activeTab)
		{
			ActiveTab.Value = activeTab;
		}
	}

	public void SetNextTab()
	{
		if (ActiveTab.Value == VendorWindowsTab.Trade)
		{
			SetActiveTab(VendorWindowsTab.Reputation);
		}
		else
		{
			SetActiveTab(VendorWindowsTab.Trade);
		}
	}

	public void OnNextActiveTab()
	{
		VendorWindowsTab vendorWindowsTab = ActiveTab.Value + 1;
		if (vendorWindowsTab >= VendorWindowsTab.Trade && vendorWindowsTab <= VendorWindowsTab.Trade)
		{
			SetActiveTab(vendorWindowsTab);
		}
	}

	public void OnPrevActiveTab()
	{
		VendorWindowsTab vendorWindowsTab = ActiveTab.Value - 1;
		if (vendorWindowsTab >= VendorWindowsTab.Trade && vendorWindowsTab <= VendorWindowsTab.Trade)
		{
			SetActiveTab(vendorWindowsTab);
		}
	}
}

using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ShipCustomization;

public class ShipTabsNavigationVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<ShipCustomizationTab> ActiveTab = new ReactiveProperty<ShipCustomizationTab>(ShipCustomizationTab.Upgrade);

	protected override void DisposeImplementation()
	{
		ActiveTab.Value = ShipCustomizationTab.Upgrade;
	}

	public void SetActiveTab(ShipCustomizationTab activeTab)
	{
		if (ActiveTab.Value != activeTab)
		{
			ActiveTab.Value = activeTab;
		}
	}

	public void SetNextTab()
	{
		int length = Enum.GetValues(typeof(ShipCustomizationTab)).Length;
		SetActiveTab((ShipCustomizationTab)((int)(ActiveTab.Value + 1) % length));
	}

	public void SetPrevTab()
	{
		int length = Enum.GetValues(typeof(ShipCustomizationTab)).Length;
		SetActiveTab((ShipCustomizationTab)((int)(ActiveTab.Value - 1 + length) % length));
	}

	public void OnNextActiveTab()
	{
		ShipCustomizationTab shipCustomizationTab = ActiveTab.Value + 1;
		if (shipCustomizationTab >= ShipCustomizationTab.Upgrade && shipCustomizationTab <= ShipCustomizationTab.Upgrade)
		{
			SetActiveTab(shipCustomizationTab);
		}
	}

	public void OnPrevActiveTab()
	{
		ShipCustomizationTab shipCustomizationTab = ActiveTab.Value - 1;
		if (shipCustomizationTab >= ShipCustomizationTab.Upgrade && shipCustomizationTab <= ShipCustomizationTab.Upgrade)
		{
			SetActiveTab(shipCustomizationTab);
		}
	}
}

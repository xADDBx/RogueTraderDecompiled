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
		if (ActiveTab.Value == ShipCustomizationTab.Upgrade)
		{
			SetActiveTab(ShipCustomizationTab.Skills);
		}
		else if (ActiveTab.Value == ShipCustomizationTab.Skills)
		{
			SetActiveTab(ShipCustomizationTab.Posts);
		}
		else
		{
			SetActiveTab(ShipCustomizationTab.Upgrade);
		}
	}

	public void SetPrevTab()
	{
		if (ActiveTab.Value == ShipCustomizationTab.Upgrade)
		{
			SetActiveTab(ShipCustomizationTab.Posts);
		}
		else if (ActiveTab.Value == ShipCustomizationTab.Skills)
		{
			SetActiveTab(ShipCustomizationTab.Upgrade);
		}
		else
		{
			SetActiveTab(ShipCustomizationTab.Skills);
		}
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

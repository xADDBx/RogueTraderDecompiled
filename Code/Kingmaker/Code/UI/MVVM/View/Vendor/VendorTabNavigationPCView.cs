using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorTabNavigationPCView : ViewBase<VendorTabNavigationVM>
{
	[SerializeField]
	private OwlcatMultiButton m_TradeButton;

	[SerializeField]
	private OwlcatMultiButton m_ReputationButton;

	[SerializeField]
	private OwlcatButton m_ReputationDoubleButton;

	private readonly string m_ActiveTabLayer = "Active";

	private readonly string m_UnactiveTabLayer = "Unactive";

	public BoolReactiveProperty IsReputation = new BoolReactiveProperty();

	public ReactiveProperty<VendorWindowsTab> CurrentTab => base.ViewModel.ActiveTab;

	protected override void BindViewImplementation()
	{
		AddDisposable(m_TradeButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetActiveTab(VendorWindowsTab.Trade);
		}));
		AddDisposable(m_ReputationButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetActiveTab(VendorWindowsTab.Reputation);
		}));
		AddDisposable(m_ReputationDoubleButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetActiveTab(VendorWindowsTab.Reputation);
		}));
		AddDisposable(base.ViewModel.ActiveTab.AsObservable().Subscribe(UpdateActiveTab));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void UpdateActiveTab(VendorWindowsTab activeTab)
	{
		m_TradeButton.SetActiveLayer((activeTab == VendorWindowsTab.Trade) ? m_ActiveTabLayer : m_UnactiveTabLayer);
		m_ReputationButton.SetActiveLayer((activeTab == VendorWindowsTab.Reputation) ? m_ActiveTabLayer : m_UnactiveTabLayer);
		IsReputation.Value = CurrentTab.Value == VendorWindowsTab.Reputation;
		base.ViewModel.SetActiveTab(activeTab);
	}

	public VendorWindowsTab GetActiveTab()
	{
		return base.ViewModel.ActiveTab.Value;
	}

	public void SetNextTab()
	{
		m_TradeButton.SetActiveLayer((base.ViewModel.ActiveTab.Value == VendorWindowsTab.Trade) ? m_UnactiveTabLayer : m_ActiveTabLayer);
		m_ReputationButton.SetActiveLayer((base.ViewModel.ActiveTab.Value == VendorWindowsTab.Reputation) ? m_UnactiveTabLayer : m_ActiveTabLayer);
		base.ViewModel.SetNextTab();
	}
}

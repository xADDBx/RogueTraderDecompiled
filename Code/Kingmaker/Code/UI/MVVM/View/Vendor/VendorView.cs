using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorView<TStashView, TInventoryCargoView, TItemsFilter, TVendorSlot, TVendorTransitionWindow, TVendorReputationForItem> : ViewBase<VendorVM>, IInitializable where TStashView : InventoryStashView where TInventoryCargoView : InventoryCargoView where TItemsFilter : ItemsFilterPCView where TVendorSlot : VendorLevelItemsBaseView where TVendorTransitionWindow : VendorTransitionWindowView where TVendorReputationForItem : VendorReputationForItemWindowView
{
	[SerializeField]
	protected TStashView m_StashView;

	[SerializeField]
	protected VendorTradePartView<TItemsFilter, TVendorSlot, TVendorTransitionWindow> m_VendorTradePartView;

	[SerializeField]
	protected VendorReputationPartView<TInventoryCargoView, TVendorReputationForItem> m_VendorReputationPartPCView;

	[SerializeField]
	protected VendorTabNavigationPCView m_VendorTabNavigation;

	[SerializeField]
	protected FlexibleLensSelectorView m_SelectorView;

	[SerializeField]
	protected TextMeshProUGUI m_ReputationButtonText;

	[SerializeField]
	protected TextMeshProUGUI m_TradeButtonText;

	[SerializeField]
	protected CargoDropZonePCView m_DropZonePCView;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_StashView.Initialize();
		m_VendorTradePartView.Initialize();
		m_VendorReputationPartPCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_VendorTabNavigation.Bind(base.ViewModel.VendorTabNavigationVM);
		m_SelectorView.Bind(base.ViewModel.Selector);
		m_StashView.Bind(base.ViewModel.StashVM);
		AddDisposable(base.ViewModel.ActiveTab.Subscribe(delegate(VendorWindowsTab val)
		{
			BindSelectedView(val);
		}));
		m_ReputationButtonText.text = UIStrings.Instance.CharacterSheet.FactionsReputation;
		m_TradeButtonText.text = UIStrings.Instance.Vendor.Trade;
		if (m_DropZonePCView != null)
		{
			AddDisposable(base.ViewModel.InventoryCargoVM.CargoDropZoneVM.Subscribe(m_DropZonePCView.Bind));
		}
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void BindSelectedView(VendorWindowsTab tab)
	{
		switch (tab)
		{
		case VendorWindowsTab.Trade:
			m_VendorTradePartView.Bind(base.ViewModel.VendorTradePartVM);
			m_VendorReputationPartPCView.Unbind();
			break;
		case VendorWindowsTab.Reputation:
			m_VendorTradePartView.Unbind();
			m_VendorReputationPartPCView.Bind(base.ViewModel.VendorReputationPartVM);
			break;
		}
	}
}

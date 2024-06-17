using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Vendor.Console;

public class VendorReputationPartConsoleView : VendorReputationPartView<InventoryCargoConsoleView, VendorReputationForItemWindowConsoleView>
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	[SerializeField]
	private ConsoleHint m_SellHint;

	[SerializeField]
	private ConsoleHint m_ShowUnrelevantHint;

	[SerializeField]
	private ConsoleHint m_SelectMenuHint;

	[SerializeField]
	private TextMeshProUGUI m_SelectMenuText;

	[SerializeField]
	private Image m_ContextMenuPlace;

	[SerializeField]
	protected GameObject m_ReputationPartTabsBlock;

	public IConsoleEntity m_CurrentFocus;

	public InventoryCargoConsoleView CargoConsoleView => m_InventoryCargoPCView;

	public BoolReactiveProperty CanSell => base.ViewModel?.CanSellCargo;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_SelectMenuText.text = UIStrings.Instance.Vendor.CargoSelectingMenu;
		AddDisposable(m_InventoryCargoPCView.OnCargoViewChange.Subscribe(delegate
		{
			ChangeView();
		}));
		AddDisposable(CargoConsoleView.HasVisibleCargo.Subscribe(delegate(bool value)
		{
			m_ReputationPartTabsBlock.gameObject.SetActive(value);
		}));
		SetupContextMenu();
	}

	private void ChangeView()
	{
		m_SelectorView.SetNextTab();
		m_ReputationForItemWindowPCView.ForceScrollToTop();
	}

	public ConsoleNavigationBehaviour GetNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		m_NavigationBehaviour.AddEntityHorizontal(m_ReputationForItemWindowPCView.GetNavigation());
		ConsoleNavigationBehaviour currentStateNavigation = m_InventoryCargoPCView.GetCurrentStateNavigation();
		m_NavigationBehaviour.AddEntityHorizontal(currentStateNavigation);
		if (currentStateNavigation.Entities.Any((IConsoleEntity x) => x.IsValid()))
		{
			m_NavigationBehaviour.FocusOnEntityManual(currentStateNavigation);
			m_CurrentFocus = currentStateNavigation;
		}
		else
		{
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_CurrentFocus = m_NavigationBehaviour.DeepestNestedFocus;
		}
		m_ReputationForItemWindowPCView.ForceScrollToTop();
		return m_NavigationBehaviour;
	}

	public void SetupContextMenu()
	{
		UIVendor vendor = UIStrings.Instance.Vendor;
		base.ViewModel.ContextMenu.Value = new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(vendor.SelectAllRelevant, base.ViewModel.SelectAll),
			new ContextMenuCollectionEntity(vendor.UnselectAllRelevant, base.ViewModel.UnselectAll)
		};
	}

	public void HandleContextMenu()
	{
		m_ContextMenuPlace.ShowContextMenu(base.ViewModel.ContextMenu?.Value);
	}

	public void SellCargo()
	{
		base.ViewModel.SellCargo();
	}

	public ConsoleHint GetSellHint()
	{
		return m_SellHint;
	}

	public ConsoleHint GetSelectContextMenuHint()
	{
		return m_SelectMenuHint;
	}

	public ConsoleHint GetUnrelevantHint()
	{
		return m_ShowUnrelevantHint;
	}

	public void SetUnrelevantToggle()
	{
		m_ShowUnrelevantToggle.Set(!m_ShowUnrelevantToggle.IsOn.Value);
	}
}

using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Menu;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Menu;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Console;
using Kingmaker.UI.MVVM.View.ShipCustomization.Console;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows;

public class ServiceWindowsConsoleView : ViewBase<ServiceWindowsVM>
{
	[SerializeField]
	private ServiceWindowMenuPCView m_ServiceWindowMenuPcView;

	[SerializeField]
	private InventoryConsoleView InventoryBaseView;

	[SerializeField]
	private CharacterInfoConsoleView m_CharacterInfoConsoleView;

	[SerializeField]
	private JournalConsoleView m_JournalConsoleView;

	[SerializeField]
	private LocalMapConsoleView m_LocalMapConsoleView;

	[SerializeField]
	private UIViewLink<EncyclopediaConsoleView, EncyclopediaVM> m_EncyclopediaView;

	[SerializeField]
	private UIViewLink<ColonyManagementConsoleView, ColonyManagementVM> m_ColonyManagementConsoleView;

	[SerializeField]
	private UIViewLink<ShipCustomizationConsoleView, ShipCustomizationVM> m_ShipCustomizationConsoleView;

	[SerializeField]
	private UIViewLink<CargoManagementConsoleView, CargoManagementVM> m_CargoManagementConsoleView;

	[SerializeField]
	private ShipNameAndPortraitPCView m_ShipNameAndPortraitPCView;

	[SerializeField]
	private CanvasScalerWorkaround m_CanvasScaler;

	[SerializeField]
	private FadeAnimator m_Background;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_ServiceWindowMenuPcView.Initialize();
			InventoryBaseView.Initialize();
			m_CharacterInfoConsoleView.Initialize();
			m_JournalConsoleView.Initialize();
			m_LocalMapConsoleView.Initialize();
			m_CargoManagementConsoleView.CustomInitialize = InitializeCargoManagement;
			m_ShipCustomizationConsoleView.CustomInitialize = InitializeShipCustomization;
			m_IsInit = true;
		}
	}

	private void InitializeCargoManagement(CargoManagementConsoleView view)
	{
		view.ShipNameAndPortraitPCView = m_ShipNameAndPortraitPCView;
	}

	private void InitializeShipCustomization(ShipCustomizationConsoleView view)
	{
		DollRoomTargetController[] componentsInChildren = view.gameObject.GetComponentsInChildren<DollRoomTargetController>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].CanvasScaler = m_CanvasScaler;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.ServiceWindowsMenuVM.Subscribe(m_ServiceWindowMenuPcView.Bind));
		AddDisposable(base.ViewModel.InventoryVM.Subscribe(InventoryBaseView.Bind));
		AddDisposable(base.ViewModel.CharacterInfoVM.Subscribe(m_CharacterInfoConsoleView.Bind));
		AddDisposable(base.ViewModel.JournalVM.Subscribe(m_JournalConsoleView.Bind));
		AddDisposable(base.ViewModel.LocalMapVM.Subscribe(m_LocalMapConsoleView.Bind));
		AddDisposable(base.ViewModel.EncyclopediaVM.Subscribe(m_EncyclopediaView.Bind));
		AddDisposable(base.ViewModel.ShipCustomizationVM.Subscribe(m_ShipCustomizationConsoleView.Bind));
		AddDisposable(base.ViewModel.ColonyManagementVM.Subscribe(m_ColonyManagementConsoleView.Bind));
		AddDisposable(base.ViewModel.CargoManagementVM.Subscribe(m_CargoManagementConsoleView.Bind));
		AddDisposable(base.ViewModel.ServiceWindowsMenuVM.Subscribe(delegate(ServiceWindowsMenuVM vm)
		{
			if (vm != null)
			{
				OnShow();
			}
			else
			{
				OnHide();
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void OnShow()
	{
		m_Background.Or(null)?.AppearAnimation();
	}

	private void OnHide()
	{
		m_Background.Or(null)?.DisappearAnimation();
	}
}

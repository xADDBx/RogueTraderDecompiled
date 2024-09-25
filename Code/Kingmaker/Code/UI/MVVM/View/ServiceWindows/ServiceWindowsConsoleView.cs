using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Menu;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Menu;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Console;
using Kingmaker.UI.MVVM.View.ShipCustomization.Console;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility.CanvasSorting;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows;

public class ServiceWindowsConsoleView : ViewBase<ServiceWindowsVM>
{
	[SerializeField]
	private ServiceWindowMenuPCView m_ServiceWindowMenuPcView;

	[SerializeField]
	private CharacterInfoConsoleView m_CharacterInfoConsoleView;

	[SerializeField]
	private UIDestroyViewLink<InventoryConsoleView, InventoryVM> InventoryBaseView;

	[SerializeField]
	private UIDestroyViewLink<JournalConsoleView, JournalVM> m_JournalConsoleView;

	[SerializeField]
	private UIDestroyViewLink<LocalMapConsoleView, LocalMapVM> m_LocalMapConsoleView;

	[SerializeField]
	private UIDestroyViewLink<EncyclopediaConsoleView, EncyclopediaVM> m_EncyclopediaView;

	[SerializeField]
	private UIDestroyViewLink<ColonyManagementConsoleView, ColonyManagementVM> m_ColonyManagementConsoleView;

	[SerializeField]
	private UIDestroyViewLink<ShipCustomizationConsoleView, ShipCustomizationVM> m_ShipCustomizationConsoleView;

	[SerializeField]
	private UIDestroyViewLink<CargoManagementConsoleView, CargoManagementVM> m_CargoManagementConsoleView;

	[SerializeField]
	private CanvasScalerWorkaround m_CanvasScaler;

	[SerializeField]
	private CanvasSortingComponent m_PartySortingComponent;

	[SerializeField]
	private FadeAnimator m_Background;

	private bool m_IsInit;

	private List<ServiceWindowsType> m_WindowsWithoutBgr = new List<ServiceWindowsType>
	{
		ServiceWindowsType.ShipCustomization,
		ServiceWindowsType.Inventory
	};

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_ServiceWindowMenuPcView.Initialize();
			m_CharacterInfoConsoleView.Initialize();
			m_ShipCustomizationConsoleView.CustomInitialize = InitializeDollRoomScale;
			InventoryBaseView.CustomInitialize = InitializeInventory;
			m_IsInit = true;
		}
	}

	private void InitializeDollRoomScale(IHasDollRoom dollRoomTarget)
	{
		dollRoomTarget.SetCanvasScaler(m_CanvasScaler);
	}

	private void InitializeInventory(InventoryConsoleView view)
	{
		InitializeDollRoomScale(view);
		view.AddSortingComponent(m_PartySortingComponent);
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
			if (vm != null && !base.ViewModel.ForceHideBackground.Value)
			{
				m_Background.Or(null)?.AppearAnimation();
			}
			else
			{
				m_Background.Or(null)?.DisappearAnimation();
			}
		}));
		AddDisposable(base.ViewModel.OnOpen.Subscribe(delegate(ServiceWindowsType vm)
		{
			if (!m_WindowsWithoutBgr.Contains(vm))
			{
				m_Background.Or(null)?.AppearAnimation();
			}
			else
			{
				m_Background.Or(null)?.DisappearAnimation();
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}

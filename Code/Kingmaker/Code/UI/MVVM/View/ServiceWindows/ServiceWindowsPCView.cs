using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.PC;
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
using Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.PC;
using Kingmaker.UI.MVVM.View.ShipCustomization;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows;

public class ServiceWindowsPCView : ViewBase<ServiceWindowsVM>
{
	[SerializeField]
	private ServiceWindowMenuPCView m_ServiceWindowMenuPcView;

	[SerializeField]
	private InventoryPCView InventoryBaseView;

	[SerializeField]
	private CharacterInfoPCView m_CharacterInfoPCView;

	[SerializeField]
	private JournalPCView m_JournalPCView;

	[SerializeField]
	private LocalMapPCView m_LocalMapPCView;

	[SerializeField]
	private UIDestroyViewLink<EncyclopediaPCView, EncyclopediaVM> m_EncyclopediaView;

	[SerializeField]
	private UIDestroyViewLink<ColonyManagementPCView, ColonyManagementVM> m_ColonyManagementPCView;

	[SerializeField]
	private UIDestroyViewLink<ShipCustomizationPCView, ShipCustomizationVM> m_ShipCustomizationPCView;

	[SerializeField]
	private UIDestroyViewLink<CargoManagementPCView, CargoManagementVM> m_CargoManagementPCView;

	[SerializeField]
	private ShipNameAndPortraitPCView m_ShipNameAndPortraitPCView;

	[SerializeField]
	private CanvasScalerWorkaround m_CanvasScaler;

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
			InventoryBaseView.Initialize();
			m_CharacterInfoPCView.Initialize();
			m_JournalPCView.Initialize();
			m_LocalMapPCView.Initialize();
			m_CargoManagementPCView.CustomInitialize = InitializeCargoManagement;
			m_ShipCustomizationPCView.CustomInitialize = InitializeShipCustomization;
			m_IsInit = true;
		}
	}

	private void InitializeCargoManagement(CargoManagementPCView view)
	{
		view.ShipNameAndPortraitPCView = m_ShipNameAndPortraitPCView;
	}

	private void InitializeShipCustomization(ShipCustomizationPCView view)
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
		AddDisposable(base.ViewModel.CharacterInfoVM.Subscribe(m_CharacterInfoPCView.Bind));
		AddDisposable(base.ViewModel.JournalVM.Subscribe(m_JournalPCView.Bind));
		AddDisposable(base.ViewModel.LocalMapVM.Subscribe(m_LocalMapPCView.Bind));
		AddDisposable(base.ViewModel.EncyclopediaVM.Subscribe(m_EncyclopediaView.Bind));
		AddDisposable(base.ViewModel.ShipCustomizationVM.Subscribe(m_ShipCustomizationPCView.Bind));
		AddDisposable(base.ViewModel.ColonyManagementVM.Subscribe(m_ColonyManagementPCView.Bind));
		AddDisposable(base.ViewModel.CargoManagementVM.Subscribe(m_CargoManagementPCView.Bind));
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

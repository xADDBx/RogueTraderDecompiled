using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Common;
using Kingmaker.Code.UI.MVVM.View.Credits;
using Kingmaker.Code.UI.MVVM.View.Dialog;
using Kingmaker.Code.UI.MVVM.View.GameOver;
using Kingmaker.Code.UI.MVVM.View.IngameMenu.PC;
using Kingmaker.Code.UI.MVVM.View.Inspect;
using Kingmaker.Code.UI.MVVM.View.Loot.PC;
using Kingmaker.Code.UI.MVVM.View.Notification;
using Kingmaker.Code.UI.MVVM.View.Party.PC;
using Kingmaker.Code.UI.MVVM.View.PointMarkers;
using Kingmaker.Code.UI.MVVM.View.SectorMap;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows;
using Kingmaker.Code.UI.MVVM.View.Subtitle;
using Kingmaker.Code.UI.MVVM.View.SystemMap;
using Kingmaker.Code.UI.MVVM.View.Transition.PC;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.View.Vendor;
using Kingmaker.Code.UI.MVVM.VM.Credits;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.Code.UI.MVVM.VM.Transition;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.View.Colonization;
using Kingmaker.UI.MVVM.View.CombatLog.PC;
using Kingmaker.UI.MVVM.View.Exploration.PC;
using Kingmaker.UI.MVVM.View.GroupChanger.PC;
using Kingmaker.UI.MVVM.View.ShipCustomization;
using Kingmaker.UI.MVVM.View.SpaceCombat.PC;
using Kingmaker.UI.MVVM.View.SystemMap;
using Kingmaker.UI.MVVM.View.SystemMap.PC;
using Kingmaker.UI.MVVM.View.SystemMapNotification.PC;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Space.PC;

public class SpaceStaticPartPCView : ViewBase<SpaceStaticPartVM>
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityView;

	[Space]
	[Header("General")]
	[SerializeField]
	private CanvasScalerWorkaround m_CanvasScalerWorkaround;

	[SerializeField]
	private ServiceWindowsPCView m_ServiceWindowsPCView;

	[SerializeField]
	private LootContextPCView m_LootContextPCView;

	[SerializeField]
	private DialogContextPCView m_DialogContextPCView;

	[SerializeField]
	private GroupChangerContextPCView m_GroupChangerContextPCView;

	[SerializeField]
	private UIViewLink<TransitionPCView, TransitionVM> m_TransitionPCViewLink;

	[SerializeField]
	private InspectPCView m_InspectPCView;

	[SerializeField]
	private IngameMenuNewPCView m_IngameMenuPCView;

	[SerializeField]
	private IngameMenuSettingsButtonPCView m_IngameMenuSettingsButtonPCView;

	[SerializeField]
	private PartyPCView m_PartyPCView;

	[SerializeField]
	private UIViewLink<VendorPCView, VendorVM> m_VendorPCViewLink;

	[SerializeField]
	private ShipCustomizationPCView m_ShipCustomizationPCView;

	[SerializeField]
	private GameOverPCView m_GameOverPCView;

	[SerializeField]
	private SubtitleView m_SubtitleView;

	[SerializeField]
	private UIViewLink<CreditsPCView, CreditsVM> m_CreditsPCView;

	[Header("Space Combat")]
	[SerializeField]
	private SpaceCombatPCView m_SpaceCombatPCView;

	[Space(10f)]
	[SerializeField]
	private CombatLogPCView m_CombatLogPCView;

	[SerializeField]
	private float m_CombatLogSpaceCombatPosition;

	[SerializeField]
	private float m_CombatLogSpacePostion;

	[Header("System Map")]
	[SerializeField]
	private SystemMapPCView m_SystemMapPCView;

	[SerializeField]
	private ZoneExitPCView m_ZoneExitPCView;

	[SerializeField]
	private ShipHealthAndRepairPCView m_ShipHealthAndRepairPCView;

	[FormerlySerializedAs("m_SpaceResourcesPCView")]
	[SerializeField]
	private SystemMapSpaceResourcesPCView m_SystemMapSpaceResourcesPCView;

	[SerializeField]
	private ExplorationPCView m_ExplorationPCView;

	[SerializeField]
	private AnomalyPCView m_AnomalyPCView;

	[SerializeField]
	private PointMarkersPCView m_SpacePointMarkersPCView;

	[FormerlySerializedAs("m_SystemTitlePCView")]
	[SerializeField]
	private SystemTitleView m_SystemTitleView;

	[SerializeField]
	private ShipPositionRulersView m_ShipPositionRulersView;

	[SerializeField]
	private CircleArcsView m_SystemMapCircleArcsView;

	[SerializeField]
	private SystemScannerView m_SystemScannerView;

	[SerializeField]
	private SystemMapNoisesView m_SystemMapNoisesView;

	[SerializeField]
	private SystemMapShipTrajectoryView m_SystemMapShipTrajectoryView;

	[Header("Sector Map")]
	[SerializeField]
	private SectorMapPCView m_SectorMapPCView;

	[Header("Notifications")]
	[SerializeField]
	private ExperienceNotificationPCView m_ExperienceNotificationPCView;

	[SerializeField]
	private EncyclopediaNotificationPCView m_EncyclopediaNotificationPCView;

	[SerializeField]
	private ColonyNotificationPCView m_ColonyNotificationPCView;

	[SerializeField]
	private MiningNotificationPCView m_MiningNotificationPCView;

	[SerializeField]
	private ColonyEventIngameMenuNotificatorPCView m_ColonyEventIngameMenuNotificatorPCView;

	[SerializeField]
	private UIViewLink<VendorSelectingWindowPCView, VendorSelectingWindowVM> m_VendorSelectingWindowContextPCView;

	private readonly Dictionary<SpaceStaticComponentType, ICommonStaticComponentView> m_ComponentViews = new Dictionary<SpaceStaticComponentType, ICommonStaticComponentView>();

	public void Initialize()
	{
		m_UIVisibilityView.Initialize();
		m_ServiceWindowsPCView.Initialize();
		m_LootContextPCView.Initialize();
		m_GroupChangerContextPCView.Initialize();
		m_InspectPCView.Initialize();
		m_CombatLogPCView.Initialize();
		m_IngameMenuPCView.Initialize();
		m_IngameMenuSettingsButtonPCView.Initialize();
		m_PartyPCView.Initialize();
		m_ExplorationPCView.Initialize();
		m_AnomalyPCView.Initialize();
		m_ExperienceNotificationPCView.Initialize();
		m_EncyclopediaNotificationPCView.Initialize();
		m_ColonyNotificationPCView.Initialize();
		m_MiningNotificationPCView.Initialize();
		m_SpacePointMarkersPCView.Initialize(m_CanvasScalerWorkaround);
		m_ComponentViews[SpaceStaticComponentType.SpacePointMarkers] = m_SpacePointMarkersPCView;
		m_ComponentViews[SpaceStaticComponentType.SystemMap] = m_SystemMapPCView;
		m_ComponentViews[SpaceStaticComponentType.SectorMap] = m_SectorMapPCView;
		m_SpaceCombatPCView.Initialize();
		m_ComponentViews[SpaceStaticComponentType.SpaceCombat] = m_SpaceCombatPCView;
		m_GameOverPCView.Initialize();
		m_ComponentViews[SpaceStaticComponentType.GameOver] = m_GameOverPCView;
		m_SubtitleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityView.Bind(base.ViewModel.UIVisibilityVM);
		m_ServiceWindowsPCView.Bind(base.ViewModel.ServiceWindowsVM);
		m_LootContextPCView.Bind(base.ViewModel.LootContextVM);
		m_DialogContextPCView.Bind(base.ViewModel.DialogContextVM);
		m_GroupChangerContextPCView.Bind(base.ViewModel.GroupChangerContextVM);
		m_InspectPCView.Bind(base.ViewModel.InspectVM);
		m_IngameMenuPCView.Bind(base.ViewModel.IngameMenuVM);
		m_IngameMenuSettingsButtonPCView.Bind(base.ViewModel.IngameMenuSettingsButtonVM);
		m_PartyPCView.Bind(base.ViewModel.PartyVM);
		m_ZoneExitPCView.Bind(base.ViewModel.ZoneExitVM);
		m_ShipHealthAndRepairPCView.Bind(base.ViewModel.ShipHealthAndRepairVM);
		m_SystemMapSpaceResourcesPCView.Bind(base.ViewModel.SystemMapSpaceResourcesVM);
		m_ExplorationPCView.Bind(base.ViewModel.ExplorationVM);
		m_AnomalyPCView.Bind(base.ViewModel.AnomalyVM);
		m_SubtitleView.Bind(base.ViewModel.SubtitleVM);
		m_SystemTitleView.Bind(base.ViewModel.SystemTitleVM);
		m_CombatLogPCView.Bind(base.ViewModel.CombatLogVM);
		m_ExperienceNotificationPCView.Bind(base.ViewModel.ExperienceNotificationVM);
		m_EncyclopediaNotificationPCView.Bind(base.ViewModel.EncyclopediaNotificationVM);
		m_ColonyNotificationPCView.Bind(base.ViewModel.ColonyNotificationVM);
		m_MiningNotificationPCView.Bind(base.ViewModel.MiningNotificationVM);
		m_ColonyEventIngameMenuNotificatorPCView.Bind(base.ViewModel.ColonyEventIngameMenuNotificatorVM);
		m_ShipPositionRulersView.Bind(base.ViewModel.ShipPositionRulersVM);
		m_SystemMapCircleArcsView.Bind(base.ViewModel.SystemMapCircleArcsVM);
		m_SystemScannerView.Bind(base.ViewModel.SystemScannerVM);
		m_SystemMapNoisesView.Bind(base.ViewModel.SystemMapNoisesVM);
		m_SystemMapShipTrajectoryView.Bind(base.ViewModel.SystemMapShipTrajectoryVM);
		AddDisposable(base.ViewModel.TransitionVM.Subscribe(m_TransitionPCViewLink.Bind));
		AddDisposable(base.ViewModel.VendorVM.Subscribe(m_VendorPCViewLink.Bind));
		AddDisposable(base.ViewModel.CreditsVM.Subscribe(m_CreditsPCView.Bind));
		AddDisposable(base.ViewModel.VendorSelectingWindowVM.Subscribe(m_VendorSelectingWindowContextPCView.Bind));
		foreach (KeyValuePair<SpaceStaticComponentType, ICommonStaticComponentView> componentView in m_ComponentViews)
		{
			AddDisposable(base.ViewModel.ComponentVMs[componentView.Key].Subscribe(componentView.Value.BindComponent));
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}

using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Credits;
using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog.Console;
using Kingmaker.Code.UI.MVVM.View.GameOver;
using Kingmaker.Code.UI.MVVM.View.IngameMenu.Console;
using Kingmaker.Code.UI.MVVM.View.Inspect;
using Kingmaker.Code.UI.MVVM.View.Loot.Console;
using Kingmaker.Code.UI.MVVM.View.Notification;
using Kingmaker.Code.UI.MVVM.View.Party.Console;
using Kingmaker.Code.UI.MVVM.View.PointMarkers;
using Kingmaker.Code.UI.MVVM.View.SectorMap.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows;
using Kingmaker.Code.UI.MVVM.View.SystemMap;
using Kingmaker.Code.UI.MVVM.View.Transition.Console;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.View.Vendor;
using Kingmaker.Code.UI.MVVM.View.Vendor.Console;
using Kingmaker.Code.UI.MVVM.VM.Credits;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.Transition;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.CombatLog.Console;
using Kingmaker.UI.MVVM.View.Exploration.Console;
using Kingmaker.UI.MVVM.View.GroupChanger.Console;
using Kingmaker.UI.MVVM.View.Space.Console;
using Kingmaker.UI.MVVM.View.SpaceCombat.Console;
using Kingmaker.UI.MVVM.View.SystemMap;
using Kingmaker.UI.MVVM.View.SystemMap.Console;
using Kingmaker.UI.MVVM.View.SystemMapNotification.Console;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Space.Console;

public class SpaceStaticPartConsoleView : ViewBase<SpaceStaticPartVM>
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityView;

	[Space]
	[Header("General")]
	[SerializeField]
	private CanvasScalerWorkaround m_CanvasScalerWorkaround;

	[SerializeField]
	private ServiceWindowsConsoleView m_ServiceWindowsConsoleView;

	[SerializeField]
	private LootContextConsoleView m_LootContextConsoleView;

	[SerializeField]
	private GroupChangerContextConsoleView m_GroupChangerContextConsoleView;

	[SerializeField]
	private UIViewLink<TransitionConsoleView, TransitionVM> m_TransitionConsoleViewLink;

	[SerializeField]
	private DialogContextConsoleView m_DialogContextConsoleView;

	[SerializeField]
	private IngameMenuConsoleView m_IngameMenuConsoleView;

	[SerializeField]
	private CombatLogConsoleView m_CombatLogConsoleView;

	[SerializeField]
	private InspectConsoleView m_InspectConsoleView;

	[SerializeField]
	private PartySelectorConsoleView m_PartySelectorConsoleView;

	[SerializeField]
	private UIViewLink<VendorConsoleView, VendorVM> m_VendorConsoleViewLink;

	[SerializeField]
	private GameOverConsoleView m_GameOverConsoleView;

	[SerializeField]
	private UIViewLink<CreditsConsoleView, CreditsVM> m_CreditsConsoleView;

	[SerializeField]
	private PartyConsoleView m_PartyConsoleView;

	[Header("Space Combat")]
	[SerializeField]
	private SpaceCombatConsoleView m_SpaceCombatConsoleView;

	[Header("System Map")]
	[SerializeField]
	private SystemMapConsoleView m_SystemMapConsoleView;

	[SerializeField]
	private ZoneExitConsoleView m_ZoneExitConsoleView;

	[SerializeField]
	private ShipHealthAndRepairConsoleView m_ShipHealthAndRepairConsoleView;

	[SerializeField]
	private SystemMapSpaceResourcesPCView m_SystemMapSpaceResourcesPCView;

	[SerializeField]
	private ExplorationConsoleView m_ExplorationConsoleView;

	[SerializeField]
	private AnomalyConsoleView m_AnomalyConsoleView;

	[SerializeField]
	private PointMarkersPCView m_SpacePointMarkersPCView;

	[SerializeField]
	private UIViewLink<VendorSelectingWindowConsoleView, VendorSelectingWindowVM> m_VendorSelectingWindowContextConsoleView;

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
	private SectorMapConsoleView m_SectorMapConsoleView;

	[Header("Notifications")]
	[SerializeField]
	private ExperienceNotificationPCView m_ExperienceNotificationPCView;

	[SerializeField]
	private EncyclopediaNotificationConsoleView m_EncyclopediaNotificationConsoleView;

	[SerializeField]
	private ColonyNotificationConsoleView m_ColonyNotificationConsoleView;

	[SerializeField]
	private MiningNotificationConsoleView m_MiningNotificationConsoleView;

	[Header("Console Input")]
	[SerializeField]
	private ConsoleHint m_ShowResourcesHint;

	[SerializeField]
	private ConsoleHint m_CloseResourcesHint;

	[SerializeField]
	private ConsoleHint m_MenuHint;

	[SerializeField]
	private ConsoleHint m_PointerHint;

	[SerializeField]
	private ConsoleHint m_CoopRolesHint;

	[SerializeField]
	private Image m_NetRolesAttentionMark;

	[SerializeField]
	private ConsoleHint m_EscMenuHint;

	private InputLayer m_ResourcesInputLayer;

	private GridConsoleNavigationBehaviour m_ResourcesNavigationBehavior;

	private readonly BoolReactiveProperty m_ResourcesMode = new BoolReactiveProperty();

	private bool IsIngameMenuAllowed
	{
		get
		{
			if (!Game.Instance.IsModeActive(GameModeType.Dialog) && !Game.Instance.Vendor.IsTrading && !RootUIContext.IsSpaceCombatRewardsShown && !RootUIContext.Instance.IsColonyRewardsShown && !RootUIContext.Instance.IsExplorationWindow && !RootUIContext.Instance.IsShipMoving)
			{
				return !RootUIContext.Instance.IsCharInfoAbilitiesChooseMode;
			}
			return false;
		}
	}

	public void Initialize()
	{
		m_UIVisibilityView.Initialize();
		m_LootContextConsoleView.Initialize();
		m_GroupChangerContextConsoleView.Initialize();
		m_ExplorationConsoleView.Initialize();
		m_AnomalyConsoleView.Initialize();
		m_IngameMenuConsoleView.Initialize();
		m_SpaceCombatConsoleView.Initialize();
		m_CombatLogConsoleView.Initialize();
		m_InspectConsoleView.Initialize();
		m_ServiceWindowsConsoleView.Initialize();
		m_ExperienceNotificationPCView.Initialize();
		m_EncyclopediaNotificationConsoleView.Initialize();
		m_ColonyNotificationConsoleView.Initialize();
		m_MiningNotificationConsoleView.Initialize();
		m_PartySelectorConsoleView.Initialize();
		m_GameOverConsoleView.Initialize();
		m_SpacePointMarkersPCView.Initialize(m_CanvasScalerWorkaround);
		m_PartyConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityView.Bind(base.ViewModel.UIVisibilityVM);
		m_LootContextConsoleView.Bind(base.ViewModel.LootContextVM);
		m_DialogContextConsoleView.Bind(base.ViewModel.DialogContextVM);
		m_GroupChangerContextConsoleView.Bind(base.ViewModel.GroupChangerContextVM);
		m_CombatLogConsoleView.Bind(base.ViewModel.CombatLogVM);
		m_InspectConsoleView.Bind(base.ViewModel.InspectVM);
		m_ZoneExitConsoleView.Bind(base.ViewModel.ZoneExitVM);
		m_ShipHealthAndRepairConsoleView.Bind(base.ViewModel.ShipHealthAndRepairVM);
		m_SystemMapSpaceResourcesPCView.Bind(base.ViewModel.SystemMapSpaceResourcesVM);
		m_ExplorationConsoleView.Bind(base.ViewModel.ExplorationVM);
		m_AnomalyConsoleView.Bind(base.ViewModel.AnomalyVM);
		m_SystemTitleView.Bind(base.ViewModel.SystemTitleVM);
		m_ShipPositionRulersView.Bind(base.ViewModel.ShipPositionRulersVM);
		m_SystemMapCircleArcsView.Bind(base.ViewModel.SystemMapCircleArcsVM);
		m_SystemScannerView.Bind(base.ViewModel.SystemScannerVM);
		m_SystemMapNoisesView.Bind(base.ViewModel.SystemMapNoisesVM);
		m_SystemMapShipTrajectoryView.Bind(base.ViewModel.SystemMapShipTrajectoryVM);
		m_ServiceWindowsConsoleView.Bind(base.ViewModel.ServiceWindowsVM);
		m_PartyConsoleView.Bind(base.ViewModel.PartyVM);
		m_ExperienceNotificationPCView.Bind(base.ViewModel.ExperienceNotificationVM);
		m_EncyclopediaNotificationConsoleView.Bind(base.ViewModel.EncyclopediaNotificationVM);
		m_ColonyNotificationConsoleView.Bind(base.ViewModel.ColonyNotificationVM);
		m_MiningNotificationConsoleView.Bind(base.ViewModel.MiningNotificationVM);
		AddDisposable(base.ViewModel.TransitionVM.Subscribe(m_TransitionConsoleViewLink.Bind));
		AddDisposable(base.ViewModel.VendorVM.Subscribe(m_VendorConsoleViewLink.Bind));
		AddDisposable(base.ViewModel.CreditsVM.Subscribe(m_CreditsConsoleView.Bind));
		AddDisposable(base.ViewModel.ComponentVMs[SpaceStaticComponentType.SpacePointMarkers].Subscribe(m_SpacePointMarkersPCView.BindComponent));
		AddDisposable(base.ViewModel.ComponentVMs[SpaceStaticComponentType.SystemMap].Subscribe(m_SystemMapConsoleView.BindComponent));
		AddDisposable(base.ViewModel.ComponentVMs[SpaceStaticComponentType.SectorMap].Subscribe(m_SectorMapConsoleView.BindComponent));
		AddDisposable(base.ViewModel.ComponentVMs[SpaceStaticComponentType.SpaceCombat].Subscribe(m_SpaceCombatConsoleView.BindComponent));
		AddDisposable(base.ViewModel.ComponentVMs[SpaceStaticComponentType.GameOver].Subscribe(m_GameOverConsoleView.BindComponent));
		AddDisposable(base.ViewModel.VendorSelectingWindowVM.Subscribe(m_VendorSelectingWindowContextConsoleView.Bind));
		AddDisposable(base.ViewModel.PlayerHaveRoles.CombineLatest(base.ViewModel.NetFirstLoadState, (bool haveRoles, bool netFirstLoadState) => new { haveRoles, netFirstLoadState }).Subscribe(value =>
		{
			m_NetRolesAttentionMark.gameObject.SetActive(value.netFirstLoadState && !value.haveRoles);
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	public void AddBaseInput(InputLayer baseInputLayer)
	{
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			SwitchIngameMenu(IsIngameMenuAllowed);
		}, 13, InputActionEventType.ButtonJustPressed, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			SwitchIngameMenu(isEnabled: false);
		}, 13, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			SwitchIngameMenu(isEnabled: false);
		}, 13, InputActionEventType.ButtonLongPressJustReleased, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			HandleLeftTriggerPressed();
		}, 12, InputActionEventType.ButtonJustPressed, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			HandleLeftTriggerReleased();
		}, 12, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			HandleLeftTriggerReleased();
		}, 12, InputActionEventType.ButtonLongPressJustReleased, enableDefaultSound: false));
		AddDisposable(m_CoopRolesHint.Bind(baseInputLayer.AddButton(delegate
		{
			base.ViewModel.OpenNetRoles();
		}, 18, base.ViewModel.NetFirstLoadState, InputActionEventType.ButtonJustLongPressed)));
		m_CoopRolesHint.SetLabel(UIStrings.Instance.EscapeMenu.EscMenuRoles);
		m_SectorMapConsoleView.AddBaseInput(baseInputLayer);
		m_EncyclopediaNotificationConsoleView.AddSystemMapInput(baseInputLayer);
		m_ColonyNotificationConsoleView.AddSystemMapInput(baseInputLayer);
		m_MiningNotificationConsoleView.AddSystemMapInput(baseInputLayer);
	}

	public void AddCombatInput(InputLayer inputLayer)
	{
		m_SpaceCombatConsoleView.AddCombatInput(inputLayer);
	}

	public void AddSystemMapInput(InputLayer inputLayer)
	{
		m_SystemMapConsoleView.AddSystemMapInput(inputLayer);
		m_ZoneExitConsoleView.AddSystemMapInput(inputLayer);
		m_ShipHealthAndRepairConsoleView.AddInput(inputLayer);
		AddDisposable(m_ShowResourcesHint.Bind(inputLayer.AddButton(delegate
		{
			ShowResources();
		}, 11, InputActionEventType.ButtonJustReleased)));
		m_ShowResourcesHint.SetLabel(UIStrings.Instance.GlobalMap.ShowResources);
		AddDisposable(m_MenuHint.Bind(inputLayer.AddButton(delegate
		{
		}, 13, base.ViewModel.ZoneExitVM?.ShipIsMoving.Not().ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
		m_MenuHint.SetLabel(UIStrings.Instance.CommonTexts.Menu);
		AddDisposable(m_PointerHint.Bind(inputLayer.AddButton(delegate
		{
		}, 18, InputActionEventType.ButtonJustReleased)));
		m_PointerHint.SetLabel(UIStrings.Instance.HUDTexts.Pointer);
		AddDisposable(m_EscMenuHint.Bind(inputLayer.AddButton(delegate
		{
		}, 16, InputActionEventType.ButtonJustReleased)));
		m_EscMenuHint.SetLabel(UIStrings.Instance.MainMenu.Settings);
		AddResourceInput();
	}

	public void AddGlobalMapInput(InputLayer inputLayer)
	{
		m_SectorMapConsoleView.AddGlobalMapInput(inputLayer);
		m_CombatLogConsoleView.AddInput(inputLayer);
	}

	public void ShowEscMenu(InputActionEventData data)
	{
		OnShowEscMenu();
	}

	public void OnShowEscMenu()
	{
		base.ViewModel.HandleShowEscMenu();
	}

	public void SwitchIngameMenu(bool isEnabled)
	{
		bool flag = RootUIContext.Instance.IsBlockedFullScreenUIType() || m_PartySelectorConsoleView.IsBinded;
		FullScreenUIType fullScreenUIType = RootUIContext.Instance.FullScreenUIType;
		bool flag2 = fullScreenUIType == FullScreenUIType.Encyclopedia || fullScreenUIType == FullScreenUIType.Journal;
		bool flag3 = isEnabled && !flag && !flag2 && !LoadingProcess.Instance.IsLoadingScreenActive;
		m_IngameMenuConsoleView.Bind(flag3 ? base.ViewModel.IngameMenuVM : null);
		if (flag3)
		{
			TooltipHelper.HideTooltip();
		}
	}

	public void SwitchSystemMapCursor(bool cursorEnabled)
	{
		m_SystemMapConsoleView.SwitchCursor(cursorEnabled);
	}

	private void HandleLeftTriggerPressed()
	{
		SwitchPartySelectorIfNeeded(isEnabled: true);
		SwitchHighlightIfNeeded(isOn: true);
	}

	private void HandleLeftTriggerReleased()
	{
		SwitchPartySelectorIfNeeded(isEnabled: false);
		SwitchHighlightIfNeeded(isOn: false);
	}

	private void SwitchPartySelectorIfNeeded(bool isEnabled)
	{
		if (!isEnabled || RootUIContext.Instance.IsBlockedFullScreenUIType() || m_IngameMenuConsoleView.IsBinded)
		{
			m_PartySelectorConsoleView.Bind(null);
			return;
		}
		FullScreenUIType fullScreenUIType = RootUIContext.Instance.FullScreenUIType;
		if (fullScreenUIType == FullScreenUIType.Inventory || fullScreenUIType == FullScreenUIType.CharacterScreen || fullScreenUIType == FullScreenUIType.Vendor)
		{
			m_PartySelectorConsoleView.Bind(base.ViewModel.PartyVM);
			TooltipHelper.HideTooltip();
		}
	}

	private void SwitchHighlightIfNeeded(bool isOn)
	{
		if (Game.Instance.Player.IsInCombat)
		{
			InteractionHighlightController.Instance.Highlight(isOn);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void AddResourceInput()
	{
		AddDisposable(m_ResourcesNavigationBehavior = new GridConsoleNavigationBehaviour());
		m_ResourcesInputLayer = m_ResourcesNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "SpaceResources"
		});
		AddDisposable(m_CloseResourcesHint.Bind(m_ResourcesInputLayer.AddButton(delegate
		{
			CloseResources();
		}, 9, m_ResourcesMode)));
		m_CloseResourcesHint.SetLabel(UIStrings.Instance.GlobalMap.CloseResources);
		AddDisposable(m_ResourcesInputLayer.AddButton(delegate
		{
			CloseResources();
		}, 11, m_ResourcesMode, InputActionEventType.ButtonJustReleased));
	}

	private void ShowResources()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.StarSystem))
		{
			m_ResourcesMode.Value = true;
			SetResourcesNavigation();
			GamePad.Instance.PushLayer(m_ResourcesInputLayer);
			m_ResourcesNavigationBehavior.FocusOnFirstValidEntity();
		}
	}

	private void CloseResources()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.StarSystem))
		{
			m_ResourcesNavigationBehavior.UnFocusCurrentEntity();
			m_ResourcesMode.Value = false;
			TooltipHelper.HideTooltip();
			GamePad.Instance.PopLayer(m_ResourcesInputLayer);
		}
	}

	private void SetResourcesNavigation()
	{
		m_ResourcesNavigationBehavior.Clear();
		WidgetListMVVM widgetListResources = m_SystemMapSpaceResourcesPCView.WidgetListResources;
		SystemMapSpaceProfitFactorView systemMapSpaceProfitFactorViewPrefab = m_SystemMapSpaceResourcesPCView.SystemMapSpaceProfitFactorViewPrefab;
		List<IConsoleNavigationEntity> list = (from block in widgetListResources.Entries.OfType<SystemMapSpaceResourceView>()
			where block.gameObject.activeInHierarchy
			select block).Cast<IConsoleNavigationEntity>().ToList();
		if (systemMapSpaceProfitFactorViewPrefab != null && systemMapSpaceProfitFactorViewPrefab.gameObject.activeInHierarchy)
		{
			list.Add(systemMapSpaceProfitFactorViewPrefab);
		}
		if (list.Any())
		{
			m_ResourcesNavigationBehavior.SetEntitiesHorizontal(list);
		}
		if (m_ResourcesMode.Value)
		{
			TooltipHelper.HideTooltip();
		}
	}
}

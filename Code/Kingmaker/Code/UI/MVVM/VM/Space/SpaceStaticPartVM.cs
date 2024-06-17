using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.Credits;
using Kingmaker.Code.UI.MVVM.VM.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Exploration;
using Kingmaker.Code.UI.MVVM.VM.GameOver;
using Kingmaker.Code.UI.MVVM.VM.GroupChanger;
using Kingmaker.Code.UI.MVVM.VM.IngameMenu;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.Notification;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Code.UI.MVVM.VM.SectorMap;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Subtitle;
using Kingmaker.Code.UI.MVVM.VM.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.Transition;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CircleArc;
using Kingmaker.UI.MVVM.VM.Colonization.Events;
using Kingmaker.UI.MVVM.VM.CombatLog;
using Kingmaker.UI.MVVM.VM.Inspect;
using Kingmaker.UI.MVVM.VM.SystemMap;
using Kingmaker.UI.MVVM.VM.SystemMapNotification;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Space;

public class SpaceStaticPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IVendorUIHandler, ISubscriber<IMechanicEntity>, ISubscriber, IShipCustomizationUIHandler, IGameModeHandler, IMultiEntranceHandler, IVendorLogicStateChanged, IBeginSelectingVendorHandler, IOpenExplorationScreenAfterColonization, ICreditsWindowUIHandler, INetRoleSetHandler, INetEvents, IProfitFactorHandler
{
	public readonly ServiceWindowsVM ServiceWindowsVM;

	public readonly LootContextVM LootContextVM;

	public readonly DialogContextVM DialogContextVM;

	public readonly GroupChangerContextVM GroupChangerContextVM;

	public readonly InspectVM InspectVM;

	public readonly IngameMenuVM IngameMenuVM;

	public readonly IngameMenuSettingsButtonVM IngameMenuSettingsButtonVM;

	public readonly UIVisibilityVM UIVisibilityVM;

	public readonly PartyVM PartyVM;

	public readonly ZoneExitVM ZoneExitVM;

	public readonly ShipHealthAndRepairVM ShipHealthAndRepairVM;

	public readonly SystemMapSpaceResourcesVM SystemMapSpaceResourcesVM;

	public readonly ExplorationVM ExplorationVM;

	public readonly AnomalyVM AnomalyVM;

	public readonly CombatLogVM CombatLogVM;

	public readonly SubtitleVM SubtitleVM;

	public readonly SystemTitleVM SystemTitleVM;

	public readonly ExperienceNotificationVM ExperienceNotificationVM;

	public readonly EncyclopediaNotificationVM EncyclopediaNotificationVM;

	public readonly ColonyNotificationVM ColonyNotificationVM;

	public readonly MiningNotificationVM MiningNotificationVM;

	public readonly ColonyEventIngameMenuNotificatorVM ColonyEventIngameMenuNotificatorVM;

	public readonly ShipPositionRulersVM ShipPositionRulersVM;

	public readonly SystemMapCircleArcsVM SystemMapCircleArcsVM;

	public readonly SystemScannerVM SystemScannerVM;

	public readonly SystemMapNoisesVM SystemMapNoisesVM;

	public readonly SystemMapShipTrajectoryVM SystemMapShipTrajectoryVM;

	public readonly ReactiveProperty<TransitionVM> TransitionVM = new ReactiveProperty<TransitionVM>(null);

	public readonly ReactiveProperty<VendorVM> VendorVM = new ReactiveProperty<VendorVM>();

	public readonly ReactiveProperty<CreditsVM> CreditsVM = new ReactiveProperty<CreditsVM>();

	public readonly ReactiveProperty<VendorSelectingWindowVM> VendorSelectingWindowVM = new ReactiveProperty<VendorSelectingWindowVM>();

	public readonly BoolReactiveProperty PlayerHaveRoles = new BoolReactiveProperty();

	public readonly BoolReactiveProperty NetFirstLoadState = new BoolReactiveProperty();

	public readonly Dictionary<SpaceStaticComponentType, ReactiveProperty<CommonStaticComponentVM>> ComponentVMs = new Dictionary<SpaceStaticComponentType, ReactiveProperty<CommonStaticComponentVM>>();

	private List<SpaceStaticComponentType> m_CreatedVMs = new List<SpaceStaticComponentType>();

	private Action<MechanicEntity> m_BeginTradingAction;

	private Dictionary<GameModeType, List<SpaceStaticComponentType>> m_GameModeContent => new Dictionary<GameModeType, List<SpaceStaticComponentType>>
	{
		{
			GameModeType.GlobalMap,
			new List<SpaceStaticComponentType> { SpaceStaticComponentType.SectorMap }
		},
		{
			GameModeType.SpaceCombat,
			new List<SpaceStaticComponentType> { SpaceStaticComponentType.SpaceCombat }
		},
		{
			GameModeType.StarSystem,
			new List<SpaceStaticComponentType>
			{
				SpaceStaticComponentType.SystemMap,
				SpaceStaticComponentType.SpacePointMarkers
			}
		},
		{
			GameModeType.GameOver,
			new List<SpaceStaticComponentType> { SpaceStaticComponentType.GameOver }
		}
	};

	public SpaceStaticPartVM()
	{
		foreach (SpaceStaticComponentType value in Enum.GetValues(typeof(SpaceStaticComponentType)))
		{
			ComponentVMs[value] = new ReactiveProperty<CommonStaticComponentVM>();
		}
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ServiceWindowsVM = new ServiceWindowsVM());
		AddDisposable(LootContextVM = new LootContextVM());
		AddDisposable(DialogContextVM = new DialogContextVM());
		AddDisposable(GroupChangerContextVM = new GroupChangerContextVM());
		AddDisposable(InspectVM = new InGameInspectVM());
		AddDisposable(CombatLogVM = new CombatLogVM());
		AddDisposable(IngameMenuVM = new IngameMenuVM());
		AddDisposable(IngameMenuSettingsButtonVM = new IngameMenuSettingsButtonVM());
		AddDisposable(PartyVM = new PartyVM());
		AddDisposable(ZoneExitVM = new ZoneExitVM());
		AddDisposable(ShipHealthAndRepairVM = new ShipHealthAndRepairVM());
		AddDisposable(SystemMapSpaceResourcesVM = new SystemMapSpaceResourcesVM());
		AddDisposable(ExplorationVM = new ExplorationVM());
		AddDisposable(AnomalyVM = new AnomalyVM());
		AddDisposable(SubtitleVM = new SubtitleVM());
		AddDisposable(SystemTitleVM = new SystemTitleVM());
		AddDisposable(ExperienceNotificationVM = new ExperienceNotificationVM());
		AddDisposable(EncyclopediaNotificationVM = new EncyclopediaNotificationVM());
		AddDisposable(ColonyNotificationVM = new ColonyNotificationVM());
		AddDisposable(MiningNotificationVM = new MiningNotificationVM());
		AddDisposable(ColonyEventIngameMenuNotificatorVM = new ColonyEventIngameMenuNotificatorVM());
		AddDisposable(ShipPositionRulersVM = new ShipPositionRulersVM());
		AddDisposable(SystemMapCircleArcsVM = new SystemMapCircleArcsVM());
		AddDisposable(SystemScannerVM = new SystemScannerVM());
		AddDisposable(SystemMapNoisesVM = new SystemMapNoisesVM());
		AddDisposable(SystemMapShipTrajectoryVM = new SystemMapShipTrajectoryVM());
		AddDisposable(UIVisibilityVM = new UIVisibilityVM());
		OnGameModeUpdated();
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive && Game.Instance.CurrentMode != GameModeType.SpaceCombat;
	}

	protected override void DisposeImplementation()
	{
		foreach (SpaceStaticComponentType value in Enum.GetValues(typeof(SpaceStaticComponentType)))
		{
			ComponentVMs[value].Value?.Dispose();
			ComponentVMs[value].Value = null;
		}
	}

	public void HandleOpenCredits()
	{
		CreditsVM disposable = (CreditsVM.Value = new CreditsVM(DisposeCredits, onlyBakers: true));
		AddDisposable(disposable);
		void DisposeCredits()
		{
			CreditsVM.Value?.Dispose();
			CreditsVM.Value = null;
		}
	}

	void IVendorLogicStateChanged.HandleBeginTrading()
	{
		m_BeginTradingAction?.Invoke(EventInvokerExtensions.MechanicEntity);
		m_BeginTradingAction = null;
	}

	void IVendorLogicStateChanged.HandleEndTrading()
	{
		DisposeVendor();
	}

	void IVendorLogicStateChanged.HandleVendorAboutToTrading()
	{
	}

	public void HandleTradeStarted()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != null)
		{
			m_BeginTradingAction = delegate
			{
				VendorVM disposable = (VendorVM.Value = new VendorVM());
				AddDisposable(disposable);
			};
			VendorHelper.Vendor.BeginTrading(mechanicEntity);
		}
	}

	private void DisposeVendor()
	{
		VendorVM.Value?.Dispose();
		VendorVM.Value = null;
	}

	public void HandleMultiEntrance(BlueprintMultiEntrance multiEntrance)
	{
		TransitionVM disposable = (TransitionVM.Value = new TransitionVM(multiEntrance, DisposeTransition));
		AddDisposable(disposable);
	}

	private void DisposeTransition()
	{
		TransitionVM.Value?.Dispose();
		TransitionVM.Value = null;
	}

	public void HandleOpenShipCustomization()
	{
		SystemMapSpaceResourcesVM.ShouldShow.Value = false;
		SystemScannerVM.ShouldShow.Value = false;
		SystemTitleVM.ShouldShow.Value = false;
	}

	public void HandleCloseAllComponentsMenu()
	{
		SystemMapSpaceResourcesVM.ShouldShow.Value = true;
		SystemScannerVM.ShouldShow.Value = true;
		SystemTitleVM.ShouldShow.Value = true;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.GameOver)
		{
			InspectVM.HandleShowInspect(state: false);
			TooltipHelper.HideTooltip();
		}
		OnGameModeUpdated();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	private List<SpaceStaticComponentType> GetComponentsList(GameModeType gameModeType)
	{
		if (!m_GameModeContent.TryGetValue(gameModeType, out var value))
		{
			return null;
		}
		return value;
	}

	public CommonStaticComponentVM TryGetComponentVM(SpaceStaticComponentType type)
	{
		if (!ComponentVMs.TryGetValue(type, out var value))
		{
			return null;
		}
		return value.Value;
	}

	private void OnGameModeUpdated()
	{
		GameModeType currentMode = Game.Instance.CurrentMode;
		CreateVMs(GetComponentsList(currentMode));
	}

	private void CreateVMs(List<SpaceStaticComponentType> types)
	{
		if (types == null || m_CreatedVMs.SequenceEqual(types))
		{
			return;
		}
		foreach (SpaceStaticComponentType item in m_CreatedVMs.Except(types))
		{
			ComponentVMs[item].Value?.Dispose();
			ComponentVMs[item].Value = null;
		}
		foreach (SpaceStaticComponentType item2 in types.Except(m_CreatedVMs))
		{
			ComponentVMs[item2].Value = CreateVM(item2);
		}
		m_CreatedVMs = types;
	}

	private CommonStaticComponentVM CreateVM(SpaceStaticComponentType type)
	{
		return type switch
		{
			SpaceStaticComponentType.SectorMap => new SectorMapVM(), 
			SpaceStaticComponentType.SystemMap => new SystemMapVM(), 
			SpaceStaticComponentType.SpaceCombat => new SpaceCombatVM(), 
			SpaceStaticComponentType.SpacePointMarkers => new SpacePointMarkersVM(), 
			SpaceStaticComponentType.GameOver => new GameOverVM(), 
			_ => null, 
		};
	}

	public void HandleShowEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void HandleBeginSelectingVendor(List<MechanicEntity> vendors)
	{
		VendorSelectingWindowVM disposable = (VendorSelectingWindowVM.Value = new VendorSelectingWindowVM(vendors));
		AddDisposable(disposable);
	}

	public void HandleExitSelectingVendor()
	{
		VendorSelectingWindowVM.Value?.Dispose();
		VendorSelectingWindowVM.Value = null;
	}

	public void HandleTryOpenExplorationScreenAfterColonization(PlanetEntity entity)
	{
		if (!ExplorationVM.IsExploring.Value)
		{
			EventBus.RaiseEvent(delegate(IExplorationUIHandler h)
			{
				h.OpenExplorationScreen(entity.View);
			});
		}
	}

	public void OpenNetRoles()
	{
		EventBus.RaiseEvent(delegate(INetRolesRequest h)
		{
			h.HandleNetRolesRequest();
		});
	}

	public void HandleRoleSet(string entityId)
	{
		PlayerHaveRoles.Value = Game.Instance.CoopData.PlayerRole.PlayerContainsAnyRole(NetworkingManager.LocalNetPlayer);
	}

	public void HandleTransferProgressChanged(bool value)
	{
	}

	public void HandleNetStateChanged(LobbyNetManager.State state)
	{
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive && Game.Instance.CurrentMode != GameModeType.SpaceCombat;
	}

	public void HandleNetGameStateChanged(NetGame.State state)
	{
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive && Game.Instance.CurrentMode != GameModeType.SpaceCombat;
	}

	public void HandleNLoadingScreenClosed()
	{
		NetFirstLoadState.Value = PhotonManager.Lobby.IsActive && Game.Instance.CurrentMode != GameModeType.SpaceCombat;
	}

	public void HandleProfitFactorModifierAdded(float max, ProfitFactorModifier modifier)
	{
		UIUtility.ShowProfitFactorModifiedNotification(max, modifier);
	}

	public void HandleProfitFactorModifierRemoved(float max, ProfitFactorModifier modifier)
	{
		UIUtility.ShowProfitFactorModifiedNotification(max, modifier);
	}
}

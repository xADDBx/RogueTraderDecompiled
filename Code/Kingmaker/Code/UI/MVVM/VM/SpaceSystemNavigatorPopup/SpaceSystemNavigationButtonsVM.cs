using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.NavigatorResource;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceSystemNavigatorPopup;

public class SpaceSystemNavigationButtonsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISectorMapPassageChangeHandler, ISubscriber<ISectorMapPassageEntity>, ISubscriber, INavigatorResourceCountChangedHandler, ISectorMapWarpTravelHandler, ISubscriber<ISectorMapObjectEntity>
{
	private readonly Action m_TravelAction;

	public readonly ReactiveProperty<bool> IsTravelNewSectorAvailable = new ReactiveProperty<bool>();

	public readonly IntReactiveProperty CreateWayCost = new IntReactiveProperty();

	public readonly IntReactiveProperty UpgradeWayCost = new IntReactiveProperty();

	public readonly IntReactiveProperty CurrentValueOfResources = new IntReactiveProperty();

	public readonly BoolReactiveProperty IsScannedFrom = new BoolReactiveProperty();

	public readonly StringReactiveProperty SystemName = new StringReactiveProperty();

	public readonly BlueprintSectorMapPointStarSystem BlueprintSectorMapPointStarSystem = new BlueprintSectorMapPointStarSystem();

	private Action m_OnLowerPassageEnded;

	public int CurrentValueOfResourcesChangeCount;

	private bool m_CanVisit;

	public readonly ReactiveProperty<bool> IsScanning = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsTraveling = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsExplored = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsWayUpgrading = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsWayCreating = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsDialogActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsVisitAvailable = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsScanAvailable = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsScanConsoleAvailable = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsTravelAvailable = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsInformationWindowsInspectMode = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsAvailable = new ReactiveProperty<bool>();

	public bool IsCurrentSystem;

	public readonly BoolReactiveProperty SomeServiceWindowIsOpened = new BoolReactiveProperty();

	private SectorMapController SectorMapController => Game.Instance.SectorMapController;

	public SectorMapObjectEntity SectorMapObject { get; }

	private RootUIContext m_UIContext => RootUIContext.Instance;

	public SpaceSystemNavigationButtonsVM(Action closeAction, Action travelAction, SectorMapObjectEntity sectorMapObject, bool openOrNot)
	{
		AddDisposable(EventBus.Subscribe(this));
		m_TravelAction = travelAction;
		SectorMapObject = sectorMapObject;
		IsTravelNewSectorAvailable.Value = openOrNot;
		CreateWayCost.Value = Game.Instance.Player.WarpTravelState.CreateNewPassageCost;
		UpgradeWayCost.Value = BlueprintWarhammerRoot.Instance.WarpRoutesSettings.LowerPassageDifficultyCost;
		CurrentValueOfResources.Value = Game.Instance.Player.WarpTravelState.NavigatorResource;
		SystemName.Value = SectorMapObject.View.Name;
		if (SectorMapObject.Blueprint is BlueprintSectorMapPointStarSystem blueprintSectorMapPointStarSystem)
		{
			BlueprintSectorMapPointStarSystem = blueprintSectorMapPointStarSystem;
		}
		CheckEverything();
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
	}

	private void OnUpdateHandler()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.GlobalMap))
		{
			CheckEverything();
		}
	}

	private void CheckEverything()
	{
		m_CanVisit = SectorMapObject?.View?.StarSystemToTransit != null;
		IsExplored.Value = SectorMapObject.IsExplored;
		ReactiveProperty<bool> isDialogActive = IsDialogActive;
		BlueprintDialog blueprintDialog = Game.Instance.DialogController?.Dialog;
		isDialogActive.Value = blueprintDialog != null && (bool)blueprintDialog;
		bool flag = UINetUtility.IsControlMainCharacter();
		SectorMapObjectEntity sectorMapObjectEntity = SectorMapController?.CurrentStarSystem;
		SomeServiceWindowIsOpened.Value = m_UIContext.CurrentServiceWindow != ServiceWindowsType.None;
		if (sectorMapObjectEntity != null)
		{
			IsScannedFrom.Value = sectorMapObjectEntity.IsScannedFrom;
			IsCurrentSystem = SectorMapObject == sectorMapObjectEntity;
			IsVisitAvailable.Value = m_CanVisit && IsCurrentSystem && flag && SectorMapObject.IsAvailable;
			IsScanAvailable.Value = IsCurrentSystem && !SectorMapObject.IsScannedFrom && flag;
			IsScanConsoleAvailable.Value = !sectorMapObjectEntity.IsScannedFrom && flag;
			IsTravelAvailable.Value = !IsCurrentSystem && flag && SectorMapObject.IsAvailable;
			IsTravelNewSectorAvailable.Value = SectorMapController.GetStarSystemsToTravel().Contains(SectorMapObject.View) && flag && SectorMapObject.IsAvailable;
			IsScanning.Value = SectorMapController.IsScanning;
			IsInformationWindowsInspectMode.Value = SectorMapController.IsInformationWindowInspectMode;
			IsAvailable.Value = SectorMapObject.IsAvailable;
		}
	}

	protected override void DisposeImplementation()
	{
	}

	public void SpaceSystemCreateWay()
	{
		Game.Instance.GameCommandQueue.CreateNewWarpRoute(Game.Instance.SectorMapController.CurrentStarSystem, SectorMapObject);
	}

	public void HandleNewPassageCreated()
	{
		SectorMapPassageEntity passage = GetPassage();
		if (EventInvokerExtensions.Entity as SectorMapPassageEntity == passage)
		{
			IsTravelNewSectorAvailable.Value = true;
			BlockPopups(state: false);
		}
	}

	public SectorMapPassageEntity GetPassage()
	{
		return Game.Instance.SectorMapController.FindPassageBetween(Game.Instance.SectorMapController.CurrentStarSystem, SectorMapObject);
	}

	public void SpaceSystemUpgradeWay(Action onEnded, SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		Game.Instance.GameCommandQueue.LowerWarpRouteDifficulty(SectorMapObject, difficulty);
		m_OnLowerPassageEnded = onEnded;
	}

	public void HandlePassageLowerDifficulty()
	{
		SectorMapPassageEntity passage = GetPassage();
		if (EventInvokerExtensions.Entity as SectorMapPassageEntity == passage)
		{
			IsTravelNewSectorAvailable.Value = true;
			BlockPopups(state: false);
			m_OnLowerPassageEnded?.Invoke();
			IsWayUpgrading.Value = false;
			IsWayCreating.Value = false;
		}
	}

	public void SpaceSystemTravelToSystem()
	{
		m_TravelAction?.Invoke();
		if (Game.Instance.IsControllerMouse)
		{
			ClosePopups();
			BlockPopups(state: true);
		}
	}

	public void BlockPopups(bool state)
	{
		SectorMapOvertipsVM.Instance.BlockPopups(state);
	}

	public void ClosePopupsCanvas(bool state)
	{
		SectorMapOvertipsVM.Instance.ClosePopupsCanvas(state);
	}

	private void ClosePopups()
	{
		SectorMapOvertipsVM.Instance.ClosePopups();
	}

	public void NoMoneyReaction(int needMoneyCount)
	{
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(string.Concat(UIStrings.Instance.GlobalMap.NoResource, " [", UIStrings.Instance.SpaceCombatTexts.NavigatorResource, "]"), addToLog: true, WarningNotificationFormat.Attention);
		});
		SectorMapBottomHudVM.Instance.NoMoneyReaction(needMoneyCount);
	}

	public void HandleChaneNavigatorResourceCount(int count)
	{
		if (count != 0)
		{
			CurrentValueOfResourcesChangeCount = count;
			CurrentValueOfResources.Value = Game.Instance.Player.WarpTravelState.NavigatorResource;
		}
	}

	public void ScanSystem()
	{
		Game.Instance.GameCommandQueue.ScanOnSectorMap();
	}

	public void VisitSystem()
	{
		if (!CheckPingCoop() && m_CanVisit)
		{
			SectorMapController.VisitStarSystem(SectorMapObject);
			EventBus.RaiseEvent(delegate(IGlobalMapSpaceSystemInformationWindowHandler h)
			{
				h.HandleHideSpaceSystemInformationWindow();
			});
		}
	}

	public bool CheckPingCoop()
	{
		return PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingEntity(SectorMapObject);
		});
	}

	public void HandleWarpTravelBeforeStart()
	{
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
		IsTraveling.Value = true;
	}

	public void HandleWarpTravelStopped()
	{
		if (Game.Instance.SectorMapController.CurrentStarSystem != null)
		{
			IsCurrentSystem = SectorMapObject == Game.Instance.SectorMapController.CurrentStarSystem;
		}
		IsTraveling.Value = false;
	}

	public void HandleWarpTravelPaused()
	{
	}

	public void HandleWarpTravelResumed()
	{
	}
}

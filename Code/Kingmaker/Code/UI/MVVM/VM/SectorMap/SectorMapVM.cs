using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.NavigatorResource;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.Code.UI.MVVM.VM.SectorMap.AllSystemsInformationWindow;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Visual.SectorMap;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SectorMap;

public class SectorMapVM : CommonStaticComponentVM, ISectorMapWarpTravelHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber, ISectorMapScanHandler, IAreaHandler, ISectorMapPassageChangeHandler, ISubscriber<ISectorMapPassageEntity>, IAdditiveAreaSwitchHandler, IGlobalMapSetAllSystemsInformationWindowStateHandler, IGameModeHandler, IDialogStartHandler, IDialogFinishHandler
{
	public readonly WarpTravelVM WarpTravelVM;

	public readonly SectorMapBottomHudVM SectorMapBottomHudVM;

	public readonly SpaceSystemInformationWindowVM SpaceSystemInformationWindowVM;

	public readonly AllSystemsInformationWindowVM AllSystemsInformationWindowVM;

	public readonly ReactiveProperty<bool> AllSystemInformationIsShowed = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> SystemInformationIsShowed = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>();

	public readonly ReactiveCommand ShowInformationWindows = new ReactiveCommand();

	public readonly ReactiveProperty<bool> IsTraveling = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsDialogActive = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsScanning = new ReactiveProperty<bool>(initialValue: false);

	public GameModeType PreviousMode;

	private static bool IsGlobalMapArea => Game.Instance.CurrentMode == GameModeType.GlobalMap;

	public SectorMapView SectorMapArtView => SectorMapView.Instance;

	public SectorMapVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(WarpTravelVM = new WarpTravelVM());
		AddDisposable(SectorMapBottomHudVM = new SectorMapBottomHudVM());
		AddDisposable(SpaceSystemInformationWindowVM = new SpaceSystemInformationWindowVM());
		AddDisposable(AllSystemsInformationWindowVM = new AllSystemsInformationWindowVM());
		AddDisposable(SpaceSystemInformationWindowVM.ShowSystemWindow.Subscribe(delegate(bool state)
		{
			SystemInformationIsShowed.Value = state;
		}));
		ShouldShow.Value = IsGlobalMapArea;
	}

	protected override void DisposeImplementation()
	{
	}

	private void SetPassageAlpha()
	{
		List<SectorMapPassageEntity> list = Game.Instance.SectorMapController.AllPassagesForSystem(Game.Instance.SectorMapController.CurrentStarSystem);
		foreach (SectorMapPassageEntity item in Game.Instance.State.Entities.OfType<SectorMapPassageEntity>())
		{
			item.View.ChangeAlpha(list.Contains(item) ? 1f : 0.28f);
		}
	}

	private void SetSystemDecal()
	{
		SectorMapController sectorMapController = Game.Instance.SectorMapController;
		IEnumerable<SectorMapObjectEntity> source = Game.Instance.State.SectorMapObjects.Where((SectorMapObjectEntity entity) => entity.View.IsSystem);
		SectorMapObjectEntity currentStarSystem = sectorMapController.CurrentStarSystem;
		(from s in source
			where s.View.IsExploredOrHasQuests
			select s into data
			select data.View).ToList().ForEach(delegate(SectorMapObject es)
		{
			es.SetPlanetVisualState();
		});
		List<SectorMapObject> starSystemsToTravel = sectorMapController.GetStarSystemsToTravel();
		foreach (SectorMapObject item in starSystemsToTravel)
		{
			SectorMapPassageEntity sectorMapPassageEntity = sectorMapController.FindPassageBetween(item.Data, currentStarSystem);
			if (!(item == null) && sectorMapPassageEntity != null)
			{
				item.SetDecalColor(sectorMapPassageEntity.CurrentDifficulty);
				item.SetDecalVisibility(item != currentStarSystem.View);
			}
		}
		foreach (SectorMapObject item2 in source.Select((SectorMapObjectEntity data) => data.View).Except(starSystemsToTravel).ToList())
		{
			item2.SetDecalVisibility(state: false);
		}
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
		IsTraveling.Value = false;
		SetPassageAlpha();
		SetSystemDecal();
		ShowInformationWindows.Execute();
	}

	public void HandleWarpTravelPaused()
	{
		IsTraveling.Value = false;
	}

	public void HandleWarpTravelResumed()
	{
		IsTraveling.Value = true;
	}

	public void HandleScanStarted(float range, float duration)
	{
		IsScanning.Value = true;
	}

	public void HandleSectorMapObjectScanned(SectorMapPassageView passageToStarSystem)
	{
		(EventInvokerExtensions.Entity as SectorMapObjectEntity)?.View.SetPlanetVisualState();
	}

	public void HandleScanStopped()
	{
		IsScanning.Value = false;
		SetSystemDecal();
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		SetPassageAlpha();
		SetSystemDecal();
	}

	public void HandleNewPassageCreated()
	{
		SetSystemDecal();
	}

	public void HandlePassageLowerDifficulty()
	{
		SetSystemDecal();
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
	}

	public void OnAdditiveAreaDidActivated()
	{
		SetPassageAlpha();
		SetSystemDecal();
	}

	public void ShowHideAllSystemsInformation(bool state)
	{
		if (state)
		{
			EventBus.RaiseEvent(delegate(IGlobalMapAllSystemsInformationWindowHandler h)
			{
				h.HandleShowAllSystemsInformationWindow();
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IGlobalMapAllSystemsInformationWindowHandler h)
			{
				h.HandleHideAllSystemsInformationWindow();
			});
		}
		AllSystemInformationIsShowed.Value = state;
	}

	public void HandleSetAllSystemsInformationWindowState(bool state)
	{
		AllSystemInformationIsShowed.Value = state;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		ShouldShow.Value = IsGlobalMapArea;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		ShouldShow.Value = IsGlobalMapArea;
		PreviousMode = gameMode;
	}

	public void ShowVisitDialogBox(SectorMapObjectEntity sectorMapObjectEntity)
	{
		EventBus.RaiseEvent(delegate(IGlobalMapInformationWindowsConsoleHandler h)
		{
			h.HandleShowSystemInformationWindowConsole(sectorMapObjectEntity);
		});
		SectorMapOvertipsVM.Instance.BlockPopups(state: false);
	}

	public void HideVisitDialogBox()
	{
		EventBus.RaiseEvent(delegate(IGlobalMapInformationWindowsConsoleHandler h)
		{
			h.HandleHideSystemInformationWindowConsole();
		});
		SectorMapOvertipsVM.Instance.BlockPopups(state: false);
	}

	public void ChangeVisitDialogBox()
	{
		EventBus.RaiseEvent(delegate(IGlobalMapInformationWindowsConsoleHandler h)
		{
			h.HandleChangeInformationWindowsConsole();
		});
		SectorMapOvertipsVM.Instance.BlockPopups(state: false);
	}

	public void HandleDialogStarted(BlueprintDialog dialog)
	{
		IsDialogActive.Value = true;
	}

	public void HandleDialogFinished(BlueprintDialog dialog, bool success)
	{
		IsDialogActive.Value = false;
	}
}

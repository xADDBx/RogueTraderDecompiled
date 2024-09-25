using System;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.NavigatorResource;

public class SectorMapBottomHudVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISectorMapWarpTravelHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber, ISectorMapScanHandler, IAreaHandler, INavigatorResourceCountChangedHandler, IAdditiveAreaSwitchHandler, IGlobalMapWillChangeNavigatorResourceEffectHandler, INetRoleSetHandler
{
	public readonly IntReactiveProperty CurrentValue = new IntReactiveProperty();

	public readonly BoolReactiveProperty NoMoney = new BoolReactiveProperty();

	public int NeedMoneyCount;

	public readonly ReactiveProperty<bool> IsScanAvailable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsTraveling = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsDialogActive = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsScanning = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsExitAvailable = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveProperty<bool> HasAccessStarshipInventory = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsWillChangeNavigatorResource = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<int> WillChangeNavigatorResourceCount = new ReactiveProperty<int>();

	public int CurrentValueOfResourcesChangeCount;

	public static SectorMapBottomHudVM Instance;

	public SectorMapBottomHudVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		Instance = this;
		SetCurrentValue();
		CheckPlayerRole();
		HasAccessStarshipInventory.Value = Game.Instance.Player.CanAccessStarshipInventory;
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
	}

	protected override void DisposeImplementation()
	{
		Instance = null;
	}

	private void OnUpdateHandler()
	{
		IsScanning.Value = Game.Instance.SectorMapController.IsScanning;
		ReactiveProperty<bool> isDialogActive = IsDialogActive;
		BlueprintDialog blueprintDialog = Game.Instance.DialogController?.Dialog;
		isDialogActive.Value = blueprintDialog != null && (bool)blueprintDialog;
	}

	private void CheckPlayerRole()
	{
		SectorMapObjectEntity currentStarSystem = Game.Instance.SectorMapController.CurrentStarSystem;
		bool flag = UINetUtility.IsControlMainCharacter();
		IsScanAvailable.Value = !currentStarSystem.IsScannedFrom && flag;
		IsExitAvailable.Value = flag;
	}

	public void SetCurrentValue()
	{
		CurrentValue.Value = Game.Instance.Player.WarpTravelState.NavigatorResource;
	}

	public void NoMoneyReaction(int needMoneyCount)
	{
		NeedMoneyCount = needMoneyCount;
		NoMoney.Value = !NoMoney.Value;
	}

	public void SetCameraOnVoidShip()
	{
		CameraRig.Instance.ScrollTo(Game.Instance.SectorMapController.CurrentStarSystem.Position);
	}

	public void ExitToShip()
	{
		Game.Instance.SectorMapController.JumpToShipArea();
	}

	public void OpenShipCustomization()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenShipCustomization();
		});
	}

	public void ScanSystem()
	{
		Game.Instance.GameCommandQueue.ScanOnSectorMap();
	}

	public void HandleWarpTravelBeforeStart()
	{
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
		IsScanAvailable.Value = false;
		IsTraveling.Value = true;
	}

	public void HandleWarpTravelStopped()
	{
		IsTraveling.Value = false;
		CheckPlayerRole();
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
		IsScanAvailable.Value = false;
	}

	public void HandleSectorMapObjectScanned(SectorMapPassageView passageToStarSystem)
	{
	}

	public void HandleScanStopped()
	{
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		CheckPlayerRole();
		SetCurrentValue();
		HasAccessStarshipInventory.Value = Game.Instance.Player.CanAccessStarshipInventory;
	}

	public void HandleChaneNavigatorResourceCount(int count)
	{
		CurrentValueOfResourcesChangeCount = count;
		SetCurrentValue();
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
	}

	public void OnAdditiveAreaDidActivated()
	{
		CheckPlayerRole();
		HasAccessStarshipInventory.Value = Game.Instance.Player.CanAccessStarshipInventory;
	}

	public void HandleWillChangeNavigatorResourceEffect(bool state, int count)
	{
		WillChangeNavigatorResourceCount.Value = count;
		IsWillChangeNavigatorResource.Value = state;
	}

	void INetRoleSetHandler.HandleRoleSet(string entityId)
	{
		CheckPlayerRole();
	}
}

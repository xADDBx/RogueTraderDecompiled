using System;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.IngameMenu;

public abstract class IngameMenuBaseVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IHideUIWhileActionCameraHandler, ISubscriber, IGameModeHandler, IExplorationUIHandler, IUIEventHandler, ISectorMapWarpTravelHandler, ISubscriber<ISectorMapObjectEntity>
{
	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsInventoryActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsJournalActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsLocalMapActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsCharScreenActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsEncyclopediaActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsShipCustomization = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsColonyManagementActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsCargoManagementActive = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsFormationActive = new ReactiveProperty<bool>();

	private bool m_IsExplorationOpened;

	private bool m_IsWarpTravelInProgress;

	protected bool IsAppropriateGameMode
	{
		get
		{
			if (Game.Instance.CurrentMode != GameModeType.Dialog && Game.Instance.CurrentMode != GameModeType.Cutscene && Game.Instance.CurrentMode != GameModeType.GameOver)
			{
				return Game.Instance.CurrentMode != GameModeType.CutsceneGlobalMap;
			}
			return false;
		}
	}

	protected IngameMenuBaseVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.InfrequentUpdateAsObservable(), delegate
		{
			UpdateHandler();
		}));
		ShouldShow.Value = IsAppropriateGameMode;
	}

	protected override void DisposeImplementation()
	{
	}

	protected virtual void UpdateHandler()
	{
		if (Game.Instance.CurrentlyLoadedArea != null)
		{
			IsInventoryActive.Value = Game.Instance.RootUiContext.CurrentServiceWindow == ServiceWindowsType.Inventory;
			IsJournalActive.Value = Game.Instance.RootUiContext.CurrentServiceWindow == ServiceWindowsType.Journal;
			IsLocalMapActive.Value = Game.Instance.RootUiContext.CurrentServiceWindow == ServiceWindowsType.LocalMap;
			IsCharScreenActive.Value = Game.Instance.RootUiContext.CurrentServiceWindow == ServiceWindowsType.CharacterInfo;
			IsEncyclopediaActive.Value = Game.Instance.RootUiContext.CurrentServiceWindow == ServiceWindowsType.Encyclopedia;
			IsShipCustomization.Value = Game.Instance.RootUiContext.CurrentServiceWindow == ServiceWindowsType.ShipCustomization;
			IsColonyManagementActive.Value = Game.Instance.RootUiContext.CurrentServiceWindow == ServiceWindowsType.ColonyManagement;
			IsCargoManagementActive.Value = Game.Instance.RootUiContext.CurrentServiceWindow == ServiceWindowsType.CargoManagement;
		}
	}

	public void HandleHideUI()
	{
		ShouldShow.Value = false;
	}

	public void HandleShowUI()
	{
		DelayedInvoker.InvokeInTime(delegate
		{
			ShouldShow.Value = true;
		}, 2.5f);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		ShouldShow.Value = IsAppropriateGameMode && !m_IsExplorationOpened && !m_IsWarpTravelInProgress;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		m_IsExplorationOpened = true;
		ShouldShow.Value = false;
	}

	public void CloseExplorationScreen()
	{
		m_IsExplorationOpened = false;
		ShouldShow.Value = IsAppropriateGameMode;
	}

	public void HandleUIEvent(UIEventType type)
	{
		IsFormationActive.Value = type == UIEventType.FormationWindowOpen;
	}

	public void HandleWarpTravelBeforeStart()
	{
		ShouldShow.Value = false;
		m_IsWarpTravelInProgress = true;
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
	}

	public void HandleWarpTravelStopped()
	{
		ShouldShow.Value = IsAppropriateGameMode && !m_IsExplorationOpened;
		m_IsWarpTravelInProgress = false;
	}

	public void HandleWarpTravelPaused()
	{
	}

	public void HandleWarpTravelResumed()
	{
	}
}

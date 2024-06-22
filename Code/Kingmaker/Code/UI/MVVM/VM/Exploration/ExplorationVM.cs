using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.Exploration;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Exploration;

public class ExplorationVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IExplorationUIHandler, ISubscriber, IColonizationChronicleHandler, IGameModeHandler, IColonizationProjectsUIHandler
{
	public readonly ReactiveProperty<bool> IsExploring = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsExplored = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsLockUIForDialog = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> HasColony = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<Colony> CurrentColony = new ReactiveProperty<Colony>();

	public readonly ResourceMinersVM ResourceMinersVM;

	public readonly ExplorationVisualElementsWrapperVM ExplorationVisualElementsWrapperVM;

	public readonly ExplorationScanResultsWrapperVM ExplorationScanResultsWrapperVM;

	public readonly ExplorationPointOfInterestListWrapperVM ExplorationPointOfInterestListWrapperVM;

	public readonly ExplorationPlanetDollRoomWrapperVM ExplorationPlanetDollRoomWrapperVM;

	public readonly ExplorationScanButtonWrapperVM ExplorationScanButtonWrapperVM;

	public readonly ExplorationColonyStatsWrapperVM ExplorationColonyStatsWrapperVM;

	public readonly ExplorationColonyTraitsWrapperVM ExplorationColonyTraitsWrapperVM;

	public readonly ExplorationColonyEventsWrapperVM ExplorationColonyEventsWrapperVM;

	public readonly ExplorationColonyProjectsWrapperVM ExplorationColonyProjectsWrapperVM;

	public readonly ExplorationColonyProjectsButtonWrapperVM ExplorationColonyProjectsButtonWrapperVM;

	public readonly ExplorationColonyProjectsBuiltListWrapperVM ExplorationColonyProjectsBuiltListWrapperVM;

	public readonly ExplorationPointOfInterestListVM ExplorationPointOfInterestListVM;

	public readonly ExplorationResourceListVM ExplorationResourceListVM;

	public readonly ExplorationSpaceBarksHolderVM ExplorationSpaceBarksHolderVM;

	public readonly ExplorationSpaceResourcesWrapperVM ExplorationSpaceResourcesWrapperVM;

	public readonly ExplorationColonyRewardsWrapperVM ExplorationColonyRewardsWrapperVM;

	private readonly List<IExplorationUIComponentWrapper> m_ExplorationUIComponentWrapperList = new List<IExplorationUIComponentWrapper>();

	private StarSystemObjectView m_StarSystemObjectView;

	private PlanetView m_PlanetView;

	private bool m_IsColonyProjectsOpen;

	public StarSystemObjectView StarSystemObjectView => m_StarSystemObjectView;

	public PlanetView PlanetView => m_PlanetView;

	public bool IsPlanet => m_PlanetView != null;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	public ExplorationVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ResourceMinersVM = new ResourceMinersVM(HasColony));
		AddDisposable(ExplorationVisualElementsWrapperVM = new ExplorationVisualElementsWrapperVM());
		AddDisposable(ExplorationScanResultsWrapperVM = new ExplorationScanResultsWrapperVM());
		AddDisposable(ExplorationPointOfInterestListWrapperVM = new ExplorationPointOfInterestListWrapperVM());
		AddDisposable(ExplorationPlanetDollRoomWrapperVM = new ExplorationPlanetDollRoomWrapperVM());
		AddDisposable(ExplorationScanButtonWrapperVM = new ExplorationScanButtonWrapperVM());
		AddDisposable(ExplorationColonyStatsWrapperVM = new ExplorationColonyStatsWrapperVM());
		AddDisposable(ExplorationColonyTraitsWrapperVM = new ExplorationColonyTraitsWrapperVM());
		AddDisposable(ExplorationColonyEventsWrapperVM = new ExplorationColonyEventsWrapperVM());
		AddDisposable(ExplorationColonyProjectsWrapperVM = new ExplorationColonyProjectsWrapperVM());
		AddDisposable(ExplorationColonyProjectsButtonWrapperVM = new ExplorationColonyProjectsButtonWrapperVM());
		AddDisposable(ExplorationColonyProjectsBuiltListWrapperVM = new ExplorationColonyProjectsBuiltListWrapperVM());
		AddDisposable(ExplorationPointOfInterestListVM = new ExplorationPointOfInterestListVM());
		AddDisposable(ExplorationResourceListVM = new ExplorationResourceListVM());
		AddDisposable(ExplorationSpaceBarksHolderVM = new ExplorationSpaceBarksHolderVM());
		AddDisposable(ExplorationSpaceResourcesWrapperVM = new ExplorationSpaceResourcesWrapperVM());
		AddDisposable(ExplorationColonyRewardsWrapperVM = new ExplorationColonyRewardsWrapperVM());
		AddDisposable(StarSystemObjectStateVM.StarSystemObjectView.Subscribe(delegate(StarSystemObjectView val)
		{
			m_StarSystemObjectView = val;
		}));
		AddDisposable(StarSystemObjectStateVM.IsScanned.Subscribe(SetScanned));
		AddDisposable(StarSystemObjectStateVM.PlanetView.Subscribe(InitializePlanet));
		AddDisposable(StarSystemObjectStateVM.Colony.Subscribe(UpdateColony));
		m_ExplorationUIComponentWrapperList.Add(ExplorationVisualElementsWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationScanResultsWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationPointOfInterestListWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationPlanetDollRoomWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationScanButtonWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationColonyProjectsWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationColonyProjectsButtonWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationColonyProjectsBuiltListWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationColonyStatsWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationColonyTraitsWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationColonyEventsWrapperVM);
		m_ExplorationUIComponentWrapperList.Add(ExplorationColonyRewardsWrapperVM);
	}

	protected override void DisposeImplementation()
	{
	}

	private void SetScanned(bool isScanned)
	{
		IsExplored.Value = isScanned;
		UpdateComponentsVisibility();
		ExplorationPointOfInterestListVM.SetSSOScanned(isScanned);
	}

	private void UpdateColony(Colony colony)
	{
		CurrentColony.Value = colony;
		HasColony.Value = CurrentColony.Value != null;
		UpdateComponentsVisibility();
	}

	private void UpdateComponentsVisibility()
	{
		if (!IsExplored.Value)
		{
			SetVisualState(ExplorationUISection.NotScanned);
		}
		else if (m_IsColonyProjectsOpen)
		{
			SetVisualState(ExplorationUISection.ColonyProjects);
		}
		else if (HasColony.Value)
		{
			SetVisualState(ExplorationUISection.Colony);
		}
		else
		{
			SetVisualState(ExplorationUISection.Exploration);
		}
	}

	private void SetVisualState(ExplorationUISection explorationUISection)
	{
		foreach (IExplorationUIComponentWrapper explorationUIComponentWrapper in m_ExplorationUIComponentWrapperList)
		{
			explorationUIComponentWrapper.SetActiveOnScreen(explorationUISection);
		}
	}

	public void ScanObject()
	{
		if (!(m_StarSystemObjectView == null))
		{
			Game.Instance.GameCommandQueue.ScanStarSystemObject(m_StarSystemObjectView.Data, finishScan: true);
		}
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		IsExploring.Value = true;
		UpdateComponentsVisibility();
		Game.Instance.GameCommandQueue.StartChronicleUI(CurrentColony?.Value);
		UIVisibilityState.ShowAllUI();
		EventBus.RaiseEvent(delegate(ICombatLogForceDeactivateControlsHandler h)
		{
			h.HandleCombatLogForceDeactivateControls();
		});
	}

	public void OpenColonyProjects()
	{
		if (!HasColony.Value)
		{
			PFLog.UI.Error("ExplorationVM.OpenColonyProjects - can't open colony projects, colony is null!");
			return;
		}
		EventBus.RaiseEvent(delegate(IColonizationProjectsUIHandler h)
		{
			h.HandleColonyProjectsUIOpen(CurrentColony.Value);
		});
	}

	private void InitializePlanet(PlanetView planetView)
	{
		if (!(planetView == null))
		{
			m_PlanetView = planetView;
		}
	}

	public void CloseExplorationScreen()
	{
		ExplorationColonyProjectsWrapperVM.ClearNavigation();
		IsExploring.Value = false;
		IsExplored.Value = false;
		HasColony.Value = false;
		m_IsColonyProjectsOpen = false;
		CurrentColony.Value = null;
		ExplorationPointOfInterestListVM.ClearData();
	}

	public void HandleChronicleStarted(Colony colony, BlueprintDialog chronicle)
	{
	}

	public void HandleChronicleFinished(Colony colony, ColonyChronicle chronicle)
	{
		Game.Instance.GameCommandQueue.StartChronicleUI(colony);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsLockUIForDialog.Value = gameMode == GameModeType.Dialog;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void HandleColonyProjectsUIOpen(Colony colony)
	{
		m_IsColonyProjectsOpen = true;
		UpdateComponentsVisibility();
	}

	public void HandleColonyProjectsUIClose()
	{
		m_IsColonyProjectsOpen = false;
		UpdateComponentsVisibility();
	}

	public void HandleColonyProjectPage(BlueprintColonyProject blueprintColonyProject)
	{
	}
}

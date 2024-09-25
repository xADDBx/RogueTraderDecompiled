using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationResourceListVM : ExplorationComponentBaseVM, IMiningUIHandler, ISubscriber, IExplorationHandler
{
	public readonly AutoDisposingList<ExplorationResourceVM> CurrentResourcesVMs = new AutoDisposingList<ExplorationResourceVM>();

	public readonly ReactiveCommand UpdateResources = new ReactiveCommand();

	private StarSystemObjectEntity m_StarSystemObjectEntity;

	private bool m_HasColony;

	public bool HasColony => m_HasColony;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	public ExplorationResourceListVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(StarSystemObjectStateVM.StarSystemObjectView.Subscribe(UpdateData));
	}

	protected override void DisposeImplementation()
	{
		ClearCurrentResourcesVMs();
	}

	private void UpdateData(StarSystemObjectView starSystemObjectView)
	{
		if (starSystemObjectView == null)
		{
			m_StarSystemObjectEntity = null;
			ClearCurrentResourcesVMs();
			UpdateResources.Execute();
		}
		else
		{
			m_HasColony = starSystemObjectView is PlanetView planetView && planetView.Data.Colony != null;
			m_StarSystemObjectEntity = starSystemObjectView.Data;
			UpdateCurrentResourcesVMs();
		}
	}

	private void UpdateCurrentResourcesVMs()
	{
		ClearCurrentResourcesVMs();
		foreach (KeyValuePair<BlueprintResource, int> item in m_StarSystemObjectEntity.ResourcesOnObject)
		{
			ExplorationResourceVM explorationResourceVM = new ExplorationResourceVM(m_StarSystemObjectEntity, item.Key, item.Value);
			AddDisposable(explorationResourceVM);
			CurrentResourcesVMs.Add(explorationResourceVM);
		}
		UpdateResources.Execute();
	}

	private void ClearCurrentResourcesVMs()
	{
		CurrentResourcesVMs.Clear();
	}

	void IMiningUIHandler.HandleStartMining(StarSystemObjectEntity starSystemObjectEntity, BlueprintResource blueprintResource)
	{
		UpdateCurrentResourcesVMs();
	}

	void IMiningUIHandler.HandleStopMining(StarSystemObjectEntity starSystemObjectEntity, BlueprintResource blueprintResource)
	{
		UpdateCurrentResourcesVMs();
	}

	public void HandlePointOfInterestInteracted(BasePointOfInterest pointOfInterest)
	{
		UpdateCurrentResourcesVMs();
	}
}

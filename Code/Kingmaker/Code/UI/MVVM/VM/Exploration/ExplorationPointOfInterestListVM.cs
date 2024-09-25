using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Exploration;

public class ExplorationPointOfInterestListVM : ExplorationComponentBaseVM, IScanStarSystemObjectHandler, ISubscriber<StarSystemObjectEntity>, ISubscriber, IExplorationHandler, IEtudesUpdateHandler
{
	public readonly AutoDisposingReactiveCollection<ExplorationPointOfInterestVM> PointsOfInterestVMs = new AutoDisposingReactiveCollection<ExplorationPointOfInterestVM>();

	public readonly ReactiveCommand UpdatePointsOfInterestCommand = new ReactiveCommand();

	public readonly AutoDisposingReactiveCollection<ExplorationResourceVM> ResourcesVMs = new AutoDisposingReactiveCollection<ExplorationResourceVM>();

	public readonly ReactiveCommand UpdateResourcesCommand = new ReactiveCommand();

	public readonly ReactiveCommand ClearAllPoints = new ReactiveCommand();

	public readonly ReactiveProperty<bool> IsSSOExplored = new ReactiveProperty<bool>(initialValue: false);

	private int m_PlanetVariant;

	public int PlanetVariant => m_PlanetVariant;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	public ExplorationPointOfInterestListVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(StarSystemObjectStateVM.StarSystemObjectView.Subscribe(UpdateData));
	}

	protected override void DisposeImplementation()
	{
		PointsOfInterestVMs.Clear();
		ResourcesVMs.Clear();
	}

	public void HandleStartScanningStarSystemObject()
	{
	}

	public void HandleScanStarSystemObject()
	{
		IsSSOExplored.Value = true;
	}

	public void HandlePointOfInterestInteracted(BasePointOfInterest pointOfInterest)
	{
		InternalUpdate();
	}

	public void OnEtudesUpdate()
	{
		InternalUpdate();
	}

	private void InternalUpdate()
	{
		UpdateData(StarSystemObjectStateVM.StarSystemObjectView.Value);
		EventBus.RaiseEvent(delegate(IPointOfInterestListUIHandler h)
		{
			h.HandlePointOfInterestUpdated();
		});
	}

	public void SetSSOScanned(bool value)
	{
		IsSSOExplored.Value = value;
	}

	public void ClearData()
	{
		IsSSOExplored.Value = false;
	}

	private void UpdateData(StarSystemObjectView starSystemObjectView)
	{
		m_PlanetVariant = 0;
		if (starSystemObjectView != null && starSystemObjectView.Data.Blueprint is BlueprintPlanet blueprintPlanet)
		{
			m_PlanetVariant = (int)blueprintPlanet.Type % 6;
		}
		ClearAllPoints.Execute();
		UpdatePointsOfInterest(starSystemObjectView);
		UpdateResources(starSystemObjectView);
	}

	private void UpdatePointsOfInterest(StarSystemObjectView starSystemObjectView)
	{
		PointsOfInterestVMs.Clear();
		if (starSystemObjectView == null)
		{
			return;
		}
		StarSystemObjectEntity data = starSystemObjectView.Data;
		foreach (BasePointOfInterest pointOfInterest in data.PointOfInterests)
		{
			if (pointOfInterest.IsVisible())
			{
				ExplorationPointOfInterestVM explorationPointOfInterestVM = new ExplorationPointOfInterestVM(pointOfInterest, data);
				AddDisposable(explorationPointOfInterestVM);
				PointsOfInterestVMs.Add(explorationPointOfInterestVM);
			}
		}
		UpdatePointsOfInterestCommand.Execute();
	}

	private void UpdateResources(StarSystemObjectView starSystemObjectView)
	{
		ResourcesVMs.Clear();
		if (starSystemObjectView == null)
		{
			return;
		}
		StarSystemObjectEntity data = starSystemObjectView.Data;
		if (data.ResourcesOnObject == null)
		{
			return;
		}
		foreach (KeyValuePair<BlueprintResource, int> item in data.ResourcesOnObject)
		{
			ExplorationResourceVM explorationResourceVM = new ExplorationResourceVM(starSystemObjectView.Data, item.Key, item.Value);
			AddDisposable(explorationResourceVM);
			ResourcesVMs.Add(explorationResourceVM);
		}
		UpdateResourcesCommand.Execute();
	}
}

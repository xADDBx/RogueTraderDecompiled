using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.SectorMap;

public class SpaceSystemInformationWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGlobalMapSpaceSystemInformationWindowHandler, ISubscriber, IGlobalMapAllSystemsInformationWindowHandler, IGlobalMapInformationWindowsConsoleHandler
{
	public readonly ReactiveProperty<SectorMapObjectEntity> SectorMapObjectEntity = new ReactiveProperty<SectorMapObjectEntity>();

	public readonly ReactiveProperty<bool> ShowSystemWindow = new ReactiveProperty<bool>();

	public readonly AutoDisposingList<PlanetInfoSpaceSystemInformationWindowVM> Planets = new AutoDisposingList<PlanetInfoSpaceSystemInformationWindowVM>();

	public readonly AutoDisposingList<OtherObjectsInfoSpaceSystemInformationWindowVM> OtherObjects = new AutoDisposingList<OtherObjectsInfoSpaceSystemInformationWindowVM>();

	public readonly AutoDisposingList<AdditionalAnomaliesInfoSpaceSystemInformationWindowVM> AdditionalAnomalies = new AutoDisposingList<AdditionalAnomaliesInfoSpaceSystemInformationWindowVM>();

	public readonly List<BlueprintAnomaly> Anomalies = new List<BlueprintAnomaly>();

	public readonly ReactiveProperty<Sprite> StarSystemSprite = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<bool> IsVisitedSystem = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsInformSystemOnTheFront = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveCommand UpdateInformation = new ReactiveCommand();

	public static SpaceSystemInformationWindowVM Instance;

	public BlueprintStarSystemMap Area => SectorMapObjectEntity.Value.View.StarSystemBlueprint.StarSystemToTransit?.Get() as BlueprintStarSystemMap;

	public SpaceSystemInformationWindowVM()
	{
		ShowSystemWindow.Value = false;
		AddDisposable(EventBus.Subscribe(this));
		Instance = this;
	}

	protected override void DisposeImplementation()
	{
		Planets.Clear();
		OtherObjects.Clear();
		AdditionalAnomalies.Clear();
		Anomalies.Clear();
		Instance = null;
	}

	public void HandleShowSpaceSystemInformationWindow()
	{
		SetSystemSettings();
		ShowSystemWindow.SetValueAndForceNotify(value: true);
	}

	public void HandleHideSpaceSystemInformationWindow()
	{
		CloseWindow();
	}

	public void HandleShowAllSystemsInformationWindow()
	{
		CloseWindow();
	}

	public void HandleHideAllSystemsInformationWindow()
	{
	}

	public void CloseWindow()
	{
		ShowSystemWindow.SetValueAndForceNotify(value: false);
		SectorMapOvertipsVM.Instance.BlockPopups(state: false);
	}

	private void SetSystemSettings(SectorMapObjectEntity sectorMapObjectEntity = null)
	{
		SectorMapObjectEntity.Value = sectorMapObjectEntity ?? Game.Instance.SectorMapController.CurrentStarSystem;
		IsVisitedSystem.Value = SectorMapObjectEntity.Value.IsVisited;
		StarSystemSprite.Value = SectorMapObjectEntity.Value.StarSystemSprite;
		AddPlanetsInfo();
		AddOtherObjectsInfo();
		AddAdditionalAnomaliesInfo();
	}

	private void AddPlanetsInfo()
	{
		Planets.Clear();
		List<BlueprintPlanet> planets = SectorMapObjectEntity.Value.Planets;
		Game.Instance.Player.StarSystemsState.ScannedPlanets.Where((PlanetExplorationInfo info) => info.StarSystemMap == Area).EmptyIfNull();
		if (planets != null && planets.Count > 0)
		{
			planets.ForEach(delegate(BlueprintPlanet planet)
			{
				Planets.Add(new PlanetInfoSpaceSystemInformationWindowVM(planet, Area));
			});
		}
	}

	private void AddOtherObjectsInfo()
	{
		OtherObjects.Clear();
		List<BlueprintArtificialObject> otherObjects = SectorMapObjectEntity.Value.OtherObjects;
		if (otherObjects != null && otherObjects.Count > 0)
		{
			otherObjects.ForEach(delegate(BlueprintArtificialObject obj)
			{
				OtherObjects.Add(new OtherObjectsInfoSpaceSystemInformationWindowVM(obj, Area));
			});
		}
	}

	private void AddAdditionalAnomaliesInfo()
	{
		AdditionalAnomalies.Clear();
		List<BlueprintAnomaly> list = ((SectorMapObjectEntity?.Value == null) ? Area?.Anomalies?.Dereference()?.Where((BlueprintAnomaly anomaly) => anomaly.ShowOnGlobalMap).EmptyIfNull().ToList() : SectorMapObjectEntity?.Value?.AnomaliesForGlobalMap);
		if (list == null || list.Count == 0)
		{
			return;
		}
		Game.Instance.Player.StarSystemsState.InteractedAnomalies.TryGetValue(SectorMapObjectEntity?.Value?.StarSystemArea ?? Area, out var value);
		foreach (BlueprintAnomaly item in value.EmptyIfNull())
		{
			if (list.Contains(item) && !item.HideInUI)
			{
				AdditionalAnomalies.Add(new AdditionalAnomaliesInfoSpaceSystemInformationWindowVM(item, Area));
			}
		}
	}

	public List<BlueprintAnomaly> GetActiveAnomalies(bool allAnomalies = true, BlueprintAnomaly.AnomalyObjectType type = BlueprintAnomaly.AnomalyObjectType.Default)
	{
		Anomalies.Clear();
		List<BlueprintAnomaly> anomalies = SectorMapObjectEntity.Value.Anomalies;
		if (anomalies == null || anomalies.Count <= 0)
		{
			return Anomalies;
		}
		Game.Instance.Player.StarSystemsState.InteractedAnomalies.TryGetValue(SectorMapObjectEntity.Value.StarSystemArea, out var ia);
		ia = ia.EmptyIfNull().ToList();
		anomalies.Where((BlueprintAnomaly an) => !ia.Contains(an) && !an.HideInUI && (allAnomalies || an.AnomalyType == type)).ForEach(delegate(BlueprintAnomaly a)
		{
			Anomalies.Add(a);
		});
		return Anomalies;
	}

	public void HandleShowSystemInformationWindowConsole(SectorMapObjectEntity sectorMapObjectEntity)
	{
		SetSystemSettings(sectorMapObjectEntity);
		ShowSystemWindow.SetValueAndForceNotify(value: true);
	}

	public void HandleHideSystemInformationWindowConsole()
	{
		CloseWindow();
	}

	public void HandleShowAllSystemsInformationWindowConsole(SectorMapObjectEntity sectorMapObjectEntity)
	{
	}

	public void HandleHideAllSystemsInformationWindowConsole()
	{
	}

	public void HandleChangeInformationWindowsConsole()
	{
		IsInformSystemOnTheFront.Value = !IsInformSystemOnTheFront.Value;
	}

	public void HandleChangeCurrentSystemInfoConsole(SectorMapObjectEntity sectorMapObjectEntity)
	{
		SetSystemSettings(sectorMapObjectEntity);
		UpdateInformation.Execute();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.SystemMap;

public class SystemMapNoisesVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber, IAreaLoadingStagesHandler, ISystemMapRadarHandler
{
	public readonly ReactiveProperty<bool> IsSystemMap = new ReactiveProperty<bool>();

	private Vector3 m_ShipPosition;

	private readonly List<StarSystemObjectEntity> m_Objects = new List<StarSystemObjectEntity>();

	private const float Distance = 10f;

	public readonly ReactiveProperty<bool> AnomalyIsNear = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> PoiIsNear = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> ResourcesIsNear = new ReactiveProperty<bool>();

	public SystemMapNoisesVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		OnGameModeStart(Game.Instance.CurrentMode);
		GetObjectsPositions();
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			OnUpdate();
		}));
	}

	private void OnUpdate()
	{
		if (Game.Instance.StarSystemMapController.StarSystemShip == null || Game.Instance.CurrentlyLoadedArea.AreaStatGameMode != GameModeType.StarSystem)
		{
			return;
		}
		m_ShipPosition = Game.Instance.StarSystemMapController.StarSystemShip.Position;
		AnomalyIsNear.Value = m_Objects.Any((StarSystemObjectEntity o) => ObjectNearCriteria(o, (StarSystemObjectEntity obj) => obj is AnomalyEntityData anomalyEntityData && !anomalyEntityData.IsInteracted));
		PoiIsNear.Value = m_Objects.Any((StarSystemObjectEntity o) => ObjectNearCriteria(o, (StarSystemObjectEntity obj) => obj is PlanetEntity && obj.PointOfInterests.Count > 0 && !obj.IsFullyExplored));
		ResourcesIsNear.Value = m_Objects.Any((StarSystemObjectEntity o) => ObjectNearCriteria(o, delegate(StarSystemObjectEntity obj)
		{
			if (obj is PlanetEntity)
			{
				Dictionary<BlueprintResource, int> resourcesOnObject = obj.ResourcesOnObject;
				if (resourcesOnObject == null)
				{
					return false;
				}
				return resourcesOnObject.Count > 0;
			}
			return false;
		}));
	}

	private bool ObjectNearCriteria(StarSystemObjectEntity obj, Func<StarSystemObjectEntity, bool> criteria)
	{
		if (!criteria(obj))
		{
			return false;
		}
		return Vector3.Distance(m_ShipPosition, obj.Position) < 10f;
	}

	private void GetObjectsPositions()
	{
		m_Objects.Clear();
		Game.Instance.State.StarSystemObjects.ToList().ForEach(delegate(StarSystemObjectEntity o)
		{
			m_Objects.Add(o);
		});
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsSystemMap.Value = Game.Instance.CurrentMode == GameModeType.StarSystem;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		IsSystemMap.Value = Game.Instance.CurrentMode == GameModeType.StarSystem;
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		GetObjectsPositions();
	}

	public void HandleShowSystemMapRadar()
	{
		GetObjectsPositions();
	}
}

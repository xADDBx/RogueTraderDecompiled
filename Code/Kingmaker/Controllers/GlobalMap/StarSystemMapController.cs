using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.GlobalMap;

public class StarSystemMapController : IController, IAreaHandler, ISubscriber, IAdditiveAreaSwitchHandler, IAnomalyHandler, ISubscriber<AnomalyEntityData>, IExplorationHandler
{
	public StarSystemShip StarSystemShip;

	public AnomalyEntityData ClickedAnomaly;

	private BlueprintStarSystemMap m_PreviousMap;

	public AreaEnterPoint EnterPointStartedFrom;

	public void OnAreaBeginUnloading()
	{
		UnloadArea();
		Object.Destroy(StarSystemShip);
		StarSystemShip = null;
	}

	public void OnAreaDidLoad()
	{
		UpdateArea();
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		UnloadArea();
	}

	private void UnloadArea()
	{
		m_PreviousMap = Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap;
		Game.Instance.Player.PreviousVisitedArea = m_PreviousMap;
		Game.Instance.Player.LastPositionOnPreviousVisitedArea = StarSystemShip.Position;
		StarSystemMapMoveController.StopPlayerShip();
		StarSystemShip.UndrawPath();
		RecalculateResearchProgress();
	}

	public void OnAdditiveAreaDidActivated()
	{
		UpdateArea();
	}

	private void UpdateArea()
	{
		Game.Instance.Player.CurrentStarSystem = Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap;
		if (Game.Instance.Player.CurrentStarSystem != m_PreviousMap)
		{
			StarSystemShip = null;
		}
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		playerShip.Commands.InterruptAllInterruptible();
		playerShip.View.Or(null)?.StopMoving();
		playerShip.View.SetVisible(visible: false);
		playerShip.IsInGame = false;
		if (StarSystemShip == null)
		{
			GameObject go = Object.Instantiate(BlueprintRoot.Instance.Prefabs.StarSystemPlayerShip, parent: Object.FindObjectOfType<SolarSystemVisualManager>().transform, position: Vector3.zero, rotation: Quaternion.identity);
			StarSystemShip = go.GetComponentNonAlloc<StarSystemShip>();
		}
		StarSystemShip.gameObject.SetActive(value: true);
		AreaEnterPoint areaEnterPoint = ((EnterPointStartedFrom != null) ? EnterPointStartedFrom : AreaEnterPoint.FindAreaEnterPointOnScene((AreaEnterPoint point) => point.GetComponentNonAlloc<StarSystemObjectView>() == null));
		if (Game.Instance.Player.PreviousVisitedArea == Game.Instance.CurrentlyLoadedArea)
		{
			Vector3? lastPositionOnPreviousVisitedArea = Game.Instance.Player.LastPositionOnPreviousVisitedArea;
			StarSystemShip.Position = lastPositionOnPreviousVisitedArea ?? new Vector3(areaEnterPoint.transform.position.x, 0f, areaEnterPoint.transform.position.z);
		}
		else
		{
			StarSystemShip.Position = new Vector3(areaEnterPoint.transform.position.x, 0f, areaEnterPoint.transform.position.z);
		}
		StarSystemsState starSystemsState = Game.Instance.Player.StarSystemsState;
		EntityPool<StarSystemObjectEntity> starSystemObjects = Game.Instance.State.StarSystemObjects;
		foreach (BlueprintPointOfInterest exploredPoint in starSystemsState.PointsExploredOutsideSystemMap.ToList())
		{
			foreach (StarSystemObjectEntity item in starSystemObjects)
			{
				BasePointOfInterest basePointOfInterest = Enumerable.FirstOrDefault(item.PointOfInterests, (BasePointOfInterest p) => p.Blueprint == exploredPoint);
				if (basePointOfInterest != null)
				{
					basePointOfInterest.SetInteracted();
					starSystemsState.PointsExploredOutsideSystemMap.Remove(exploredPoint);
				}
			}
		}
		List<BlueprintAnomaly> anomalySetToNonInteractable = starSystemsState.AnomalySetToNonInteractable;
		foreach (StarSystemObjectEntity item2 in starSystemObjects)
		{
			if (!Enumerable.Any(anomalySetToNonInteractable))
			{
				break;
			}
			if (item2 is AnomalyEntityData anomalyEntityData && anomalySetToNonInteractable.Contains(anomalyEntityData.Blueprint))
			{
				anomalyEntityData.SetNonInteractable();
				anomalySetToNonInteractable.Remove(anomalyEntityData.Blueprint);
			}
		}
		m_PreviousMap = Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap;
		Game.Instance.Player.PreviousVisitedArea = m_PreviousMap;
		Game.Instance.Player.LastPositionOnPreviousVisitedArea = StarSystemShip.Position;
	}

	public void LandOnAnomaly(AnomalyEntityData anomaly)
	{
		ClickedAnomaly = anomaly;
	}

	public float RecalculateResearchProgress(BlueprintStarSystemMap areaBlueprint = null)
	{
		float value = ResearchPercent(areaBlueprint);
		EventBus.RaiseEvent(delegate(IStarSystemMapResearchProgress h)
		{
			h.HandleResearchPercentRecalculate(areaBlueprint, value);
		});
		return value;
	}

	private float ResearchPercent(BlueprintStarSystemMap areaBlueprint)
	{
		if (areaBlueprint == null)
		{
			areaBlueprint = Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap;
		}
		if (areaBlueprint == null)
		{
			return 0f;
		}
		List<PoiToCondition> list = (from poi in areaBlueprint.PointsForResearchProgress?.EmptyIfNull()
			where poi.Conditions?.Get()?.Check() ?? true
			select poi).EmptyIfNull().ToList();
		List<AnomalyToCondition> list2 = areaBlueprint.AnomaliesResearchProgress?.Where((AnomalyToCondition anom) => anom.Conditions?.Get()?.Check() ?? true).EmptyIfNull().ToList();
		int num = list.Count + list2.Count;
		int num2 = 0;
		if (Game.Instance.Player.StarSystemsState.InteractedAnomalies.TryGetValue(areaBlueprint, out var value))
		{
			foreach (AnomalyToCondition item in list2)
			{
				if (value.Contains(item.Anomaly.Get()))
				{
					num2++;
				}
			}
		}
		if (Game.Instance.Player.StarSystemsState.InteractedPoints.TryGetValue(areaBlueprint, out var value2))
		{
			foreach (PoiToCondition item2 in list)
			{
				if (value2.Contains(item2.Poi.Get()))
				{
					num2++;
				}
			}
		}
		if (num == 0)
		{
			return 100f;
		}
		return (float)num2 / (float)num * 100f;
	}

	public bool IsResearchedFully(BlueprintStarSystemMap areaBlueprint)
	{
		if (areaBlueprint == null)
		{
			return false;
		}
		IEnumerable<PoiToCondition> enumerable = areaBlueprint.PointsForResearchProgress?.EmptyIfNull().Where(delegate(PoiToCondition poi)
		{
			ConditionsReference conditions2 = poi.Conditions;
			return conditions2 != null && conditions2.Get()?.Check() == false;
		}).EmptyIfNull();
		IEnumerable<AnomalyToCondition> enumerable2 = areaBlueprint.AnomaliesResearchProgress?.Where(delegate(AnomalyToCondition anom)
		{
			ConditionsReference conditions = anom.Conditions;
			return conditions != null && conditions.Get()?.Check() == false;
		}).EmptyIfNull();
		if (Game.Instance.Player.StarSystemsState.InteractedPoints.TryGetValue(areaBlueprint, out var value))
		{
			foreach (PoiToCondition item in enumerable)
			{
				if (!value.Contains(item.Poi.Get()))
				{
					return false;
				}
			}
		}
		if (Game.Instance.Player.StarSystemsState.InteractedAnomalies.TryGetValue(areaBlueprint, out var value2))
		{
			foreach (AnomalyToCondition item2 in enumerable2)
			{
				if (!value2.Contains(item2.Anomaly.Get()))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void HandleAnomalyInteracted()
	{
		RecalculateResearchProgress();
	}

	public void HandlePointOfInterestInteracted(BasePointOfInterest pointOfInterest)
	{
		RecalculateResearchProgress();
	}
}

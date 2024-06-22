using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.View;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.View.Mechanics;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.SectorMap;

[KnowledgeDatabaseID("ecbda9837e2a4213bbe8a3932d142343")]
public class SectorMapObject : MechanicEntityView, INetPingEntity, ISubscriber
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintSectorMapPointReference m_Blueprint;

	private GameObject m_ShipMarkerVisual;

	private GameObject m_UnvisitedAndNoPathVisual;

	private GameObject m_UnvisitedVisual;

	private GameObject m_VisitedVisual;

	private GameObject m_AllActivitiesFinishedVisual;

	public bool IsSystem;

	public bool IsExploredOnStart;

	public bool IsVisitedOnStart;

	public bool Hidden;

	public int VisualVariant;

	[CanBeNull]
	public SectorMapVisualParameters.StarSystemPrefab OverrideVisualPrefab;

	public SystemNameController SystemNamePrefab;

	private SystemNameController m_SystemNameInstance;

	public bool m_LayerNameVisibility;

	public SystemPlanetDecalCanTravelController SystemPlanetDecalCanTravelPrefab;

	private SystemPlanetDecalCanTravelController m_SystemPlanetDecalCanTravelInstance;

	public bool m_LayerPlanetDecalCanTravelVisibility;

	public SystemPlanetDecalConsoleFocusController SystemPlanetDecalConsoleFocusPrefab;

	private SystemPlanetDecalConsoleFocusController m_SystemPlanetDecalConsoleFocusInstance;

	public SystemPlanetDecalCoopPingController SystemPlanetDecalCoopPingPrefab;

	private SystemPlanetDecalCoopPingController m_SystemPlanetDecalCoopPingInstance;

	private Tween m_PingTween;

	public BlueprintSectorMapPoint Blueprint => m_Blueprint.Get();

	public BlueprintSectorMapPointStarSystem StarSystemBlueprint
	{
		get
		{
			if (!IsSystem)
			{
				return null;
			}
			return m_Blueprint.Get() as BlueprintSectorMapPointStarSystem;
		}
	}

	public string Name => m_Blueprint.Get().Name;

	public BlueprintArea StarSystemToTransit => StarSystemBlueprint?.StarSystemToTransit?.Get();

	public BlueprintAreaEnterPoint StarSystemAreaPoint => StarSystemBlueprint?.StarSystemAreaPoint?.Get();

	public new SectorMapObjectEntity Data => (SectorMapObjectEntity)base.Data;

	private SectorMapController SectorMapController => Game.Instance.SectorMapController;

	public bool IsExploredOrHasQuests
	{
		get
		{
			if (!Data.IsExplored)
			{
				return CheckQuests();
			}
			return true;
		}
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		if (SystemNamePrefab != null)
		{
			m_SystemNameInstance = Object.Instantiate(SystemNamePrefab, base.transform);
			m_SystemNameInstance.Initialize(this);
		}
		if (SystemPlanetDecalCanTravelPrefab != null)
		{
			m_SystemPlanetDecalCanTravelInstance = Object.Instantiate(SystemPlanetDecalCanTravelPrefab, base.transform);
			m_SystemPlanetDecalCanTravelInstance.Initialize(this);
		}
		if (SystemPlanetDecalCoopPingPrefab != null && PhotonManager.Initialized)
		{
			m_SystemPlanetDecalCoopPingInstance = Object.Instantiate(SystemPlanetDecalCoopPingPrefab, base.transform);
			m_SystemPlanetDecalCoopPingInstance.PingEntity(state: false);
		}
		SectorMapVisualParameters single = ObjectRegistry<SectorMapVisualParameters>.Instance.Single;
		m_ShipMarkerVisual = Object.Instantiate((OverrideVisualPrefab?.ShipMarker != null) ? OverrideVisualPrefab.ShipMarker : single.StarSystemPrefabVariants[VisualVariant].ShipMarker, base.ViewTransform);
		m_UnvisitedAndNoPathVisual = Object.Instantiate((OverrideVisualPrefab?.UnvisitedAndNoPath != null) ? OverrideVisualPrefab.UnvisitedAndNoPath : single.StarSystemPrefabVariants[VisualVariant].UnvisitedAndNoPath, base.ViewTransform);
		m_UnvisitedVisual = Object.Instantiate((OverrideVisualPrefab?.Unvisited != null) ? OverrideVisualPrefab.Unvisited : single.StarSystemPrefabVariants[VisualVariant].Unvisited, base.ViewTransform);
		m_VisitedVisual = Object.Instantiate((OverrideVisualPrefab?.Visited != null) ? OverrideVisualPrefab.Visited : single.StarSystemPrefabVariants[VisualVariant].Visited, base.ViewTransform);
		m_AllActivitiesFinishedVisual = Object.Instantiate((OverrideVisualPrefab?.AllActivitiesFinished != null) ? OverrideVisualPrefab.AllActivitiesFinished : single.StarSystemPrefabVariants[VisualVariant].AllActivitiesFinished, base.ViewTransform);
		m_ShipMarkerVisual.SetActive(value: false);
		m_UnvisitedAndNoPathVisual.SetActive(value: false);
		m_UnvisitedVisual.SetActive(value: false);
		m_VisitedVisual.SetActive(value: false);
		m_AllActivitiesFinishedVisual.SetActive(value: false);
		SetNameVisibility();
		SetVisible(IsExploredOrHasQuests);
		SetDecalVisibility(state: false);
		SetPlanetVisualState();
	}

	public void InstanceSystemPlanetDecalConsoleFocus()
	{
		if (!(SystemPlanetDecalConsoleFocusPrefab == null) && Game.Instance.IsControllerGamepad)
		{
			m_SystemPlanetDecalConsoleFocusInstance = Object.Instantiate(SystemPlanetDecalConsoleFocusPrefab, base.transform);
			m_SystemPlanetDecalConsoleFocusInstance.SetFocusState(state: false);
		}
	}

	public void DestroySystemPlanetDecalConsoleFocus()
	{
		if (ObjectExtensions.Or(m_SystemPlanetDecalConsoleFocusInstance, null)?.gameObject != null)
		{
			Object.Destroy(m_SystemPlanetDecalConsoleFocusInstance.gameObject);
		}
	}

	public void SetPlanetVisualState()
	{
		Vector3 position = Game.Instance.SectorMapController.VisualParameters.PlayerShip.gameObject.transform.position;
		Vector3 position2 = base.gameObject.transform.position;
		if (IsExploredOrHasQuests && Mathf.Approximately(position.x, position2.x) && Mathf.Approximately(position.z, position2.z))
		{
			m_ShipMarkerVisual.SetActive(value: true);
			m_UnvisitedAndNoPathVisual.SetActive(value: false);
			m_UnvisitedVisual.SetActive(value: false);
			m_VisitedVisual.SetActive(value: false);
			m_AllActivitiesFinishedVisual.SetActive(value: false);
			return;
		}
		bool flag = SectorMapController.AllPassagesForSystem(Data).Any((SectorMapPassageEntity p) => p.IsExplored) || SectorMapController.GetCurrentStarSystem() == this;
		if (IsExploredOrHasQuests && !flag)
		{
			m_ShipMarkerVisual.SetActive(value: false);
			m_UnvisitedAndNoPathVisual.SetActive(value: true);
			m_UnvisitedVisual.SetActive(value: false);
			m_VisitedVisual.SetActive(value: false);
			m_AllActivitiesFinishedVisual.SetActive(value: false);
		}
		else if (IsExploredOrHasQuests && !Data.IsVisited)
		{
			m_ShipMarkerVisual.SetActive(value: false);
			m_UnvisitedAndNoPathVisual.SetActive(value: false);
			m_UnvisitedVisual.SetActive(value: true);
			m_VisitedVisual.SetActive(value: false);
			m_AllActivitiesFinishedVisual.SetActive(value: false);
		}
		else if (Data.IsVisited)
		{
			m_ShipMarkerVisual.SetActive(value: false);
			m_UnvisitedAndNoPathVisual.SetActive(value: false);
			m_UnvisitedVisual.SetActive(value: false);
			m_VisitedVisual.SetActive(value: true);
			m_AllActivitiesFinishedVisual.SetActive(value: false);
		}
	}

	protected override void OnWillDetachFromData()
	{
		base.OnWillDetachFromData();
		EventBus.Unsubscribe(this);
		if (ObjectExtensions.Or(m_SystemNameInstance, null)?.gameObject != null)
		{
			m_SystemNameInstance.Unsubscribe();
			Object.Destroy(m_SystemNameInstance.gameObject);
		}
		if (ObjectExtensions.Or(m_SystemPlanetDecalCanTravelInstance, null)?.gameObject != null)
		{
			Object.Destroy(m_SystemPlanetDecalCanTravelInstance.gameObject);
		}
		if (ObjectExtensions.Or(m_SystemPlanetDecalCoopPingInstance, null)?.gameObject != null)
		{
			Object.Destroy(m_SystemPlanetDecalCoopPingInstance.gameObject);
		}
		Object.Destroy(m_ShipMarkerVisual);
		Object.Destroy(m_UnvisitedAndNoPathVisual);
		Object.Destroy(m_UnvisitedVisual);
		Object.Destroy(m_VisitedVisual);
		Object.Destroy(m_AllActivitiesFinishedVisual);
	}

	public void SetNameVisibilityFromFilter(bool visible)
	{
		m_LayerNameVisibility = visible;
		SetNameVisibility();
	}

	private void SetNameVisibility()
	{
		ObjectExtensions.Or(m_SystemNameInstance, null)?.SetVisibility(IsExploredOrHasQuests);
	}

	public void SetDecalVisibility(bool state)
	{
		ObjectExtensions.Or(m_SystemPlanetDecalCanTravelInstance, null)?.SetVisibility(state && IsExploredOrHasQuests);
	}

	public void SetDecalColor(SectorMapPassageEntity.PassageDifficulty dif)
	{
		ObjectExtensions.Or(m_SystemPlanetDecalCanTravelInstance, null)?.SetDecalColor(dif);
	}

	public void SetConsoleFocusState(bool state)
	{
		if (!Game.Instance.IsControllerMouse)
		{
			if (m_SystemPlanetDecalConsoleFocusInstance == null && SystemPlanetDecalConsoleFocusPrefab != null)
			{
				m_SystemPlanetDecalConsoleFocusInstance = Object.Instantiate(SystemPlanetDecalConsoleFocusPrefab, base.transform);
			}
			m_SystemPlanetDecalConsoleFocusInstance.SetFocusState(state);
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		return CreateSectorMapPointEntityData(load);
	}

	private SectorMapObjectEntity CreateSectorMapPointEntityData(bool _)
	{
		return Entity.Initialize(new SectorMapObjectEntity(this));
	}

	public void SetExplored()
	{
		if (!Data.IsVisited)
		{
			SetVisible(visible: true);
			SetPlanetVisualState();
			SetNameVisibility();
		}
	}

	public void SetVisited()
	{
		SetVisible(visible: true);
		SetPlanetVisualState();
		SetNameVisibility();
	}

	public bool CheckQuests()
	{
		List<QuestObjective> questsForSystem = UIUtilitySpaceQuests.GetQuestsForSystem(this);
		List<QuestObjective> questsForSpaceSystem = UIUtilitySpaceQuests.GetQuestsForSpaceSystem(Data.StarSystemArea);
		if (questsForSystem == null || !questsForSystem.Any())
		{
			return questsForSpaceSystem?.Any() ?? false;
		}
		return true;
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		m_PingTween?.Kill();
		if (entity != Data)
		{
			return;
		}
		int index = player.Index - 1;
		Material material = BlueprintRoot.Instance.UIConfig.CoopPlayersPingsMaterials[index];
		m_SystemPlanetDecalCoopPingInstance.PingEntity(state: true, material);
		EventBus.RaiseEvent(delegate(INetAddPingMarker h)
		{
			h.HandleAddPingEntityMarker(entity);
		});
		m_PingTween = DOTween.To(() => 1f, delegate
		{
		}, 0f, 7.5f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			m_SystemPlanetDecalCoopPingInstance.PingEntity(state: false);
			EventBus.RaiseEvent(delegate(INetAddPingMarker h)
			{
				h.HandleRemovePingEntityMarker(entity);
			});
			m_PingTween = null;
		})
			.OnKill(delegate
			{
				m_SystemPlanetDecalCoopPingInstance.PingEntity(state: false);
				EventBus.RaiseEvent(delegate(INetAddPingMarker h)
				{
					h.HandleRemovePingEntityMarker(entity);
				});
				m_PingTween = null;
			});
	}
}

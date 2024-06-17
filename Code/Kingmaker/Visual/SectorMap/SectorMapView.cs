using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Globalmap.View;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Visual.SectorMap;

public class SectorMapView : MonoBehaviour, ISectorMapScanHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber
{
	[Header("Common")]
	public bool ShowAdditionalGizmos;

	public Transform HorizontalPointerLine;

	public Transform VerticalPointerLine;

	public Collider MapCollider;

	public ScanPulseController ScanPulse;

	public SystemNameController SystemNamePrefab;

	[Header("Warp Routes Visual")]
	public float distanceBetweenBezierPoints = 0.335f;

	public float bezierPointsCurveRangeSafe = 0.05f;

	public float bezierPointsCurveRangeUnsafe = 0.1f;

	public float bezierPointsCurveRangeDangerous = 0.15f;

	public float bezierPointsCurveRangeDeadly = 0.25f;

	[Space]
	public List<SectorMapObject> SectorMapObjects = new List<SectorMapObject>();

	public List<SectorMapPassageView> SectorMapPassages = new List<SectorMapPassageView>();

	private Vector3 m_PointerWorldPosition = Vector3.zero;

	public static SectorMapView Instance { get; private set; }

	public SystemMapLayer LayersMask { get; private set; } = SystemMapLayer.All;


	public bool NeedShowSectorMapMouseLines { get; set; } = true;


	private void Start()
	{
		if (!HorizontalPointerLine || !VerticalPointerLine)
		{
			UberDebug.LogError("No HorizontalPointer or VerticalPointer object");
			Object.DestroyImmediate(this);
		}
		UpdateLayersVisibility();
	}

	private void Update()
	{
		SetPointerLinesToMousePointerPos();
	}

	private void OnEnable()
	{
		Instance = this;
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
		Instance = null;
	}

	public void SetLayerVisibility(SystemMapLayer layer, bool visible)
	{
		if (visible)
		{
			LayersMask |= layer;
		}
		else
		{
			LayersMask &= ~layer;
		}
		UpdateLayersVisibility();
	}

	private void UpdateLayersVisibility()
	{
		bool systemNamesVisible = LayersMask.HasFlag(SystemMapLayer.Systems);
		SectorMapObjects.ForEach(delegate(SectorMapObject sector)
		{
			sector.SetNameVisibilityFromFilter(systemNamesVisible);
		});
		bool routesVisible = LayersMask.HasFlag(SystemMapLayer.Routes);
		Game.Instance.State.Entities.OfType<SectorMapPassageEntity>().ForEach(delegate(SectorMapPassageEntity passage)
		{
			passage.View.SetVisibilityFromFilter(routesVisible);
		});
		bool rumoursVisible = LayersMask.HasFlag(SystemMapLayer.Rumors);
		Game.Instance.State.SectorMapRumours.ForEach(delegate(SectorMapRumourEntity rumour)
		{
			rumour.View.SetVisibilityFromFilter(rumoursVisible);
		});
	}

	private void SetPointerLinesToMousePointerPos()
	{
		if (!(Game.GetCamera() == null) && NeedShowSectorMapMouseLines)
		{
			Vector3 position = HorizontalPointerLine.position;
			Vector3 position2 = VerticalPointerLine.position;
			Ray ray = Game.GetCamera().ScreenPointToRay(Game.Instance.CursorController.CursorPosition);
			if (MapCollider.Raycast(ray, out var hitInfo, 1000f))
			{
				m_PointerWorldPosition = hitInfo.point;
			}
			HorizontalPointerLine.position = new Vector3(position.x, position2.y, m_PointerWorldPosition.z);
			VerticalPointerLine.position = new Vector3(m_PointerWorldPosition.x, position2.y, position2.z);
		}
	}

	public void SetCustomPointerLinesToPos(Vector3 pos)
	{
		if (!NeedShowSectorMapMouseLines)
		{
			Vector3 position = HorizontalPointerLine.position;
			Vector3 position2 = VerticalPointerLine.position;
			HorizontalPointerLine.position = new Vector3(position.x, position2.y, pos.z);
			VerticalPointerLine.position = new Vector3(pos.x, position2.y, position2.z);
		}
	}

	public void HandleScanStarted(float range, float duration)
	{
		SectorMapObjectEntity sectorMapObjectEntity = EventInvokerExtensions.Entity as SectorMapObjectEntity;
		ScanPulse.transform.position = new Vector3(sectorMapObjectEntity.Position.x, ScanPulse.transform.position.y, sectorMapObjectEntity.Position.z);
		ScanPulse.Pulse(range, duration);
	}

	public void HandleSectorMapObjectScanned(SectorMapPassageView passageToStarSystem)
	{
	}

	public void HandleScanStopped()
	{
	}
}

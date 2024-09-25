using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Async;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.InGameCombat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.PointMarkers;

public class PointMarkersVM : CommonStaticComponentVM, ILineOfSightHandler, ISubscriber, INetAddPingMarker
{
	public readonly ReactiveCollection<PointMarkerVM> PointMarkers = new ReactiveCollection<PointMarkerVM>();

	protected readonly List<BaseUnitEntity> Units = new List<BaseUnitEntity>();

	protected readonly List<Entity> AnotherEntities = new List<Entity>();

	protected readonly List<GameObject> PingPositions = new List<GameObject>();

	private Coroutine m_DirtyUnitsCoroutine;

	public IEnumerable<PointMarkerVM> VisibleMarkers => PointMarkers.Where((PointMarkerVM vm) => vm.IsVisible.Value);

	protected virtual IEnumerable<BaseUnitEntity> UnitsSelector => new List<BaseUnitEntity>();

	protected virtual IEnumerable<Entity> AnotherEntitiesSelector => new List<Entity>();

	protected virtual IEnumerable<GameObject> PingPositionSelector => new List<GameObject>();

	protected PointMarkersVM()
	{
		SetEntities();
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			UpdateHandler();
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		if (m_DirtyUnitsCoroutine != null)
		{
			CoroutineRunner.Stop(m_DirtyUnitsCoroutine);
		}
		Clear();
	}

	protected void Clear()
	{
		PointMarkers.ForEach(delegate(PointMarkerVM markerVm)
		{
			markerVm.Dispose();
		});
		PointMarkers.Clear();
		Units.Clear();
		AnotherEntities.Clear();
		PingPositions.Clear();
	}

	private void UpdateHandler()
	{
		PointMarkers.ForEach(delegate(PointMarkerVM markerVm)
		{
			markerVm.Update();
		});
	}

	private void SetEntities()
	{
		Clear();
		foreach (BaseUnitEntity item in UnitsSelector)
		{
			PointMarkers.Add(new PointMarkerVM(item));
			Units.Add(item);
		}
		foreach (Entity item2 in AnotherEntitiesSelector)
		{
			PointMarkers.Add(new PointMarkerVM(item2));
			AnotherEntities.Add(item2);
		}
		foreach (GameObject item3 in PingPositionSelector)
		{
			PointMarkers.Add(new PointMarkerVM(item3));
			PingPositions.Add(item3);
		}
	}

	private IEnumerator DirtyUnits()
	{
		yield return null;
		yield return new WaitUntil(() => !ResourcesLibrary.Preloading);
		while (LoadingProcess.Instance.IsLoadingInProcess)
		{
			yield return null;
		}
		SetEntities();
		m_DirtyUnitsCoroutine = null;
	}

	protected async void UpdateUnits()
	{
		await Awaiters.UnityThread;
		if (m_DirtyUnitsCoroutine != null)
		{
			CoroutineRunner.Stop(m_DirtyUnitsCoroutine);
		}
		m_DirtyUnitsCoroutine = CoroutineRunner.Start(DirtyUnits());
	}

	public void OnLineOfSightCreated(LineOfSightVM los)
	{
		PointMarkerVM pointMarkerVM = PointMarkers.FirstOrDefault((PointMarkerVM m) => m?.Unit == los?.Owner);
		if (pointMarkerVM != null)
		{
			pointMarkerVM.LineOfSight.Value = los;
		}
	}

	public void OnLineOfSightDestroyed(LineOfSightVM los)
	{
		PointMarkerVM pointMarkerVM = PointMarkers.FirstOrDefault((PointMarkerVM m) => m?.Unit == los?.Owner);
		if (pointMarkerVM != null)
		{
			pointMarkerVM.LineOfSight.Value = null;
		}
	}

	private void AddPingEntity(Entity entity, GameObject pingPosition)
	{
		if (pingPosition != null)
		{
			PointMarkers.Add(new PointMarkerVM(pingPosition));
			PingPositions.Add(pingPosition);
		}
		else
		{
			PointMarkers.Add(new PointMarkerVM(entity, isPing: true));
			AnotherEntities.Add(entity);
		}
	}

	private void RemovePingEntity(Entity entity, GameObject pingPosition)
	{
		if (pingPosition != null)
		{
			PointMarkerVM pointMarkerVM = PointMarkers.FirstOrDefault((PointMarkerVM m) => m.PingPosition == pingPosition);
			pointMarkerVM?.Dispose();
			PointMarkers.Remove(pointMarkerVM);
			PingPositions.Remove(pingPosition);
		}
		else
		{
			PointMarkerVM pointMarkerVM2 = PointMarkers.FirstOrDefault((PointMarkerVM m) => m.AnotherEntity == entity);
			pointMarkerVM2?.Dispose();
			PointMarkers.Remove(pointMarkerVM2);
			AnotherEntities.Remove(entity);
		}
	}

	public void HandleAddPingEntityMarker(Entity entity)
	{
		AddPingEntity(entity, null);
	}

	public void HandleRemovePingEntityMarker(Entity entity)
	{
		RemovePingEntity(entity, null);
	}

	public void HandleAddPingPositionMarker(GameObject gameObject)
	{
		AddPingEntity(null, gameObject);
	}

	public void HandleRemovePingPositionMarker(GameObject gameObject)
	{
		RemovePingEntity(null, gameObject);
	}
}

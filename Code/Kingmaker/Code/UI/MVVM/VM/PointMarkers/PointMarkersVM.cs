using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Async;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.InGameCombat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.PointMarkers;

public class PointMarkersVM : CommonStaticComponentVM, ILineOfSightHandler, ISubscriber
{
	public readonly ReactiveCollection<PointMarkerVM> PointMarkers = new ReactiveCollection<PointMarkerVM>();

	protected readonly List<BaseUnitEntity> Units = new List<BaseUnitEntity>();

	private Coroutine m_DirtyUnitsCoroutine;

	public IEnumerable<PointMarkerVM> VisibleMarkers => PointMarkers.Where((PointMarkerVM vm) => vm.IsVisible.Value);

	protected virtual IEnumerable<BaseUnitEntity> UnitsSelector => new List<BaseUnitEntity>();

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
}

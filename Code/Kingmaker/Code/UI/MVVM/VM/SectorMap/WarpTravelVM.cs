using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SectorMap;

public class WarpTravelVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISectorMapWarpTravelHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber
{
	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>();

	public WarpTravelVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleWarpTravelBeforeStart()
	{
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
		IsVisible.Value = true;
	}

	public void HandleWarpTravelStopped()
	{
		IsVisible.Value = false;
	}

	public void HandleWarpTravelPaused()
	{
	}

	public void HandleWarpTravelResumed()
	{
	}
}

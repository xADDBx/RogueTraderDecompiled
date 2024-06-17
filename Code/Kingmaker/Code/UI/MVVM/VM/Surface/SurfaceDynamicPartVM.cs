using System;
using Kingmaker.Code.UI.MVVM.VM.Overtips;
using Kingmaker.Code.UI.MVVM.VM.PointMarkers;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Code.UI.MVVM.VM.VariativeInteraction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Surface;

public class SurfaceDynamicPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IVariativeInteractionUIHandler, ISubscriber, IAreaHandler, IAdditiveAreaSwitchHandler
{
	public readonly ReactiveProperty<VariativeInteractionVM> VariativeInteractionVM = new ReactiveProperty<VariativeInteractionVM>();

	public readonly ReactiveProperty<SurfaceOvertipsVM> SurfaceOvertipsVM = new ReactiveProperty<SurfaceOvertipsVM>();

	public readonly ReactiveProperty<PointMarkersVM> PointMarkersVM = new ReactiveProperty<PointMarkersVM>();

	public readonly UIVisibilityVM UIVisibilityVM;

	public SurfaceDynamicPartVM()
	{
		AddDisposable(UIVisibilityVM = new UIVisibilityVM());
		AddDisposable(EventBus.Subscribe(this));
		CreateOvertips();
	}

	private void DisposeOvertips()
	{
		SurfaceOvertipsVM.Value?.Dispose();
		SurfaceOvertipsVM.Value = null;
		PointMarkersVM.Value?.Dispose();
		PointMarkersVM.Value = null;
	}

	private void CreateOvertips()
	{
		DisposeOvertips();
		SurfaceOvertipsVM.Value = new SurfaceOvertipsVM();
		PointMarkersVM.Value = new SurfacePointMarkersVM();
	}

	public void OnAreaDidLoad()
	{
		CreateOvertips();
	}

	public void OnAreaBeginUnloading()
	{
		DisposeOvertips();
	}

	public void OnAdditiveAreaDidActivated()
	{
		CreateOvertips();
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		DisposeOvertips();
	}

	protected override void DisposeImplementation()
	{
		DisposeOvertips();
		DisposeLockpick();
	}

	public void HandleInteractionRequest(MapObjectView mapObjectView)
	{
		VariativeInteractionVM.Value = new VariativeInteractionVM(mapObjectView, DisposeLockpick);
	}

	private void DisposeLockpick()
	{
		VariativeInteractionVM.Value?.Dispose();
		VariativeInteractionVM.Value = null;
	}
}

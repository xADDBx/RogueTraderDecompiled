using System;
using Kingmaker.Code.UI.MVVM.VM.Overtips;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Code.UI.MVVM.VM.VariativeInteraction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Space;

public class SpaceDynamicPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IVariativeInteractionUIHandler, ISubscriber, IAreaHandler, IAdditiveAreaSwitchHandler
{
	public readonly ReactiveProperty<VariativeInteractionVM> VariativeInteractionVM = new ReactiveProperty<VariativeInteractionVM>();

	public readonly ReactiveProperty<SpaceOvertipsVM> SpaceOvertipsVM = new ReactiveProperty<SpaceOvertipsVM>();

	public readonly ReactiveProperty<SpaceCombatPointMarkersVM> SpaceCombatPointMarkersVM = new ReactiveProperty<SpaceCombatPointMarkersVM>();

	public readonly UIVisibilityVM UIVisibilityVM;

	public SpaceDynamicPartVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		CreateOvertips();
		CreateCombatPointMarkers();
		AddDisposable(UIVisibilityVM = new UIVisibilityVM());
	}

	public void OnAreaDidLoad()
	{
		CreateOvertips();
		CreateCombatPointMarkers();
	}

	public void OnAreaBeginUnloading()
	{
		DisposeOvertips();
		DisposeCombatPointMarkers();
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		DisposeOvertips();
		DisposeCombatPointMarkers();
	}

	public void OnAdditiveAreaDidActivated()
	{
		CreateOvertips();
		CreateCombatPointMarkers();
	}

	protected override void DisposeImplementation()
	{
		DisposeOvertips();
		DisposeCombatPointMarkers();
		DisposeLockpick();
	}

	public void HandleInteractionRequest(MapObjectView mapObjectView)
	{
		VariativeInteractionVM.Value = new VariativeInteractionVM(mapObjectView, DisposeLockpick);
	}

	private void CreateOvertips()
	{
		DisposeOvertips();
		SpaceOvertipsVM.Value = new SpaceOvertipsVM();
	}

	private void CreateCombatPointMarkers()
	{
		DisposeCombatPointMarkers();
		SpaceCombatPointMarkersVM.Value = new SpaceCombatPointMarkersVM();
	}

	private void DisposeLockpick()
	{
		VariativeInteractionVM.Value?.Dispose();
		VariativeInteractionVM.Value = null;
	}

	private void DisposeOvertips()
	{
		SpaceOvertipsVM.Value?.Dispose();
		SpaceOvertipsVM.Value = null;
	}

	private void DisposeCombatPointMarkers()
	{
		SpaceCombatPointMarkersVM.Value?.Dispose();
		SpaceCombatPointMarkersVM.Value = null;
	}
}

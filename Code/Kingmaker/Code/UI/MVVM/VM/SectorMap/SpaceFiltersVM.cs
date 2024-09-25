using System;
using Kingmaker.Visual.SectorMap;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SectorMap;

public class SpaceFiltersVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<bool> SystemsVisible = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> RoutesVisible = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> RumorsVisible = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> ColoniesActive = new ReactiveProperty<bool>();

	public SpaceFiltersVM()
	{
		UpdateVisibility();
	}

	protected override void DisposeImplementation()
	{
	}

	private void UpdateVisibility()
	{
		if (!(SectorMapView.Instance == null))
		{
			SystemsVisible.Value = SectorMapView.Instance.LayersMask.HasFlag(SystemMapLayer.Systems);
			RoutesVisible.Value = SectorMapView.Instance.LayersMask.HasFlag(SystemMapLayer.Routes);
			RumorsVisible.Value = SectorMapView.Instance.LayersMask.HasFlag(SystemMapLayer.Rumors);
			ColoniesActive.Value = Game.Instance.Player.ColoniesState.Colonies.Count > 0;
		}
	}

	public void SwitchNamesVisible()
	{
		SectorMapView.Instance.SetLayerVisibility(SystemMapLayer.Systems, !SystemsVisible.Value);
		UpdateVisibility();
	}

	public void SwitchRoutesVisible()
	{
		SectorMapView.Instance.SetLayerVisibility(SystemMapLayer.Routes, !RoutesVisible.Value);
		UpdateVisibility();
	}

	public void SwitchRumorsVisible()
	{
		SectorMapView.Instance.SetLayerVisibility(SystemMapLayer.Rumors, !RumorsVisible.Value);
		UpdateVisibility();
	}
}

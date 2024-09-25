using System;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization;

public class SpaceResourceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<PlanetEntity> CurrentPlanet = new ReactiveProperty<PlanetEntity>(null);

	public readonly ReactiveProperty<Colony> CurrentColony = new ReactiveProperty<Colony>(null);

	public readonly ReactiveProperty<string> SpaceResource = new ReactiveProperty<string>(string.Empty);

	public SpaceResourceVM()
	{
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
	}

	private void OnUpdateHandler()
	{
	}

	protected override void DisposeImplementation()
	{
	}
}

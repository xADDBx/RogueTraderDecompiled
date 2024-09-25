using System;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization;

public class PeopleResourceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<PlanetEntity> CurrentPlanet = new ReactiveProperty<PlanetEntity>(null);

	public readonly ReactiveProperty<Colony> CurrentColony = new ReactiveProperty<Colony>(null);

	public readonly ReactiveProperty<string> PeopleResource = new ReactiveProperty<string>(string.Empty);

	public PeopleResourceVM()
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

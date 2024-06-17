using System;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap.Collections;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;

public class SystemMapOvertipsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IInGameHandler, ISubscriber<IEntity>, ISubscriber
{
	public readonly SystemObjectOvertipsCollectionVM SystemObjectOvertipsCollectionVM;

	public readonly PlanetOvertipsCollectionVM PlanetOvertipsCollectionVM;

	public readonly AnomalyOvertipsCollectionVM AnomalyOvertipsCollectionVM;

	public SystemMapOvertipsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(SystemObjectOvertipsCollectionVM = new SystemObjectOvertipsCollectionVM());
		AddDisposable(PlanetOvertipsCollectionVM = new PlanetOvertipsCollectionVM());
		AddDisposable(AnomalyOvertipsCollectionVM = new AnomalyOvertipsCollectionVM());
	}

	protected override void DisposeImplementation()
	{
	}

	public void ForceRescan()
	{
		SystemObjectOvertipsCollectionVM.ForceRescan();
		PlanetOvertipsCollectionVM.ForceRescan();
		AnomalyOvertipsCollectionVM.ForceRescan();
	}

	public void HandleObjectInGameChanged()
	{
		ForceRescan();
	}
}

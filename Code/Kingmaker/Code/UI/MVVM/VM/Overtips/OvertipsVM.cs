using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips;

public abstract class OvertipsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IBarkHandler, ISubscriber<IEntity>, ISubscriber
{
	protected OvertipsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public abstract void HandleOnShowBark(string text);

	public abstract void HandleOnShowLinkedBark(string text, string encyclopediaLink);

	public abstract void HandleOnHideBark();
}

using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Visual.Sound;

public class AwarenessAsksController : IUnitAsksController, IDisposable, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber
{
	public AwarenessAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	void IAwarenessHandler.OnEntityNoticed(BaseUnitEntity spotter)
	{
		if (!(spotter.View == null))
		{
			spotter.View.Asks?.Discovery.Schedule();
		}
	}
}

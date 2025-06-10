using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;

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
		if (spotter.View == null)
		{
			return;
		}
		BaseUnitEntity master = spotter.Master;
		spotter.View.Asks?.Discovery.Schedule(is2D: false, delegate
		{
			if (master != null && !(master.View == null) && master.View.Asks != null)
			{
				PartLifeState lifeStateOptional = master.GetLifeStateOptional();
				if (lifeStateOptional != null && lifeStateOptional.IsConscious)
				{
					master.View.Asks?.ReactToPetDiscovery.Schedule();
				}
			}
		});
	}
}

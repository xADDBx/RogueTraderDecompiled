using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Visual.Sound;

public class ItemInteractionsAsksController : IUnitAsksController, IDisposable, IInsertItemFailHandler, ISubscriber<IItemEntity>, ISubscriber
{
	public ItemInteractionsAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	void IInsertItemFailHandler.HandleInsertFail(MechanicEntity owner)
	{
		owner?.View.Asks?.CantDo.Schedule(is2D: true);
	}
}

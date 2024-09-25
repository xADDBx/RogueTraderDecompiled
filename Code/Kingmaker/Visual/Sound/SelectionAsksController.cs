using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Visual.Sound;

public class SelectionAsksController : IUnitAsksController, IDisposable, ISelectionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IForceSelectHandler, ITrySelectNotControllableHandler
{
	public SelectionAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	void ISelectionHandler.OnUnitSelectionAdd(bool single, bool ask)
	{
		if (single && ask)
		{
			ScheduleSelected(EventInvokerExtensions.BaseUnitEntity);
		}
	}

	void ISelectionHandler.OnUnitSelectionRemove()
	{
	}

	void IForceSelectHandler.HandleForceSelect(bool single, bool ask)
	{
		if (single && ask)
		{
			ScheduleSelected(EventInvokerExtensions.BaseUnitEntity);
		}
	}

	void ITrySelectNotControllableHandler.HandleSelectNotControllable(bool single, bool ask)
	{
		if (single && ask)
		{
			ScheduleSelected(EventInvokerExtensions.BaseUnitEntity);
		}
	}

	private static void ScheduleSelected(BaseUnitEntity unit)
	{
		if (!unit.IsInCombat && unit.LifeState.IsConscious)
		{
			unit.View.Asks?.Selected.Schedule();
		}
	}
}

using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventInterruptCurrentTurn : GameLogEvent<GameLogEventInterruptCurrentTurn>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IInterruptCurrentTurnHandler, ISubscriber<IMechanicEntity>, ISubscriber
	{
		public void HandleOnInterruptCurrentTurn()
		{
			if (EventInvokerExtensions.Entity is BaseUnitEntity actor)
			{
				AddEvent(new GameLogEventInterruptCurrentTurn(actor));
			}
		}
	}

	public UnitReference Actor { get; private set; }

	private GameLogEventInterruptCurrentTurn()
	{
	}

	private GameLogEventInterruptCurrentTurn(BaseUnitEntity actor)
	{
		Actor = actor.FromBaseUnitEntity();
	}
}

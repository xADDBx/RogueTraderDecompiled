using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventUnitMissedTurn : GameLogEvent<GameLogEventUnitMissedTurn>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IUnitMissedTurnHandler, ISubscriber<IEntity>, ISubscriber
	{
		public void HandleOnMissedTurn()
		{
			if (EventInvokerExtensions.Entity is IAbstractUnitEntity actor)
			{
				AddEvent(new GameLogEventUnitMissedTurn(actor));
			}
		}
	}

	public readonly UnitReference Actor;

	private GameLogEventUnitMissedTurn(IAbstractUnitEntity actor)
	{
		Actor = UnitReference.FromIAbstractUnitEntity(actor);
	}
}

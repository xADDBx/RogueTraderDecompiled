using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventPartyEncumbranceChanged : GameLogEvent<GameLogEventPartyEncumbranceChanged>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IPartyEncumbranceHandler, ISubscriber
	{
		public void ChangePartyEncumbrance(Encumbrance prevEncumbrance)
		{
			AddEvent(new GameLogEventPartyEncumbranceChanged(Game.Instance.Player.Encumbrance, prevEncumbrance));
		}
	}

	public readonly Encumbrance CurrentEncumbrance;

	public readonly Encumbrance PreviousEncumbrance;

	public GameLogEventPartyEncumbranceChanged(Encumbrance currentEncumbrance, Encumbrance previousEncumbrance)
	{
		CurrentEncumbrance = currentEncumbrance;
		PreviousEncumbrance = previousEncumbrance;
	}
}

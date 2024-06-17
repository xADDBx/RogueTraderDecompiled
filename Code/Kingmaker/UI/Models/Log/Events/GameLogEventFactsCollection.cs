using Kingmaker.EntitySystem;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventFactsCollection : GameLogEvent<GameLogEventFactsCollection>
{
	public enum EventType
	{
		Added,
		Removed
	}

	private class EventsHandler : GameLogController.GameEventsHandler
	{
		private void TryAddEvent(EventType type, EntityFact fact)
		{
			AddEvent(new GameLogEventFactsCollection(type, fact));
		}
	}

	public readonly EventType Event;

	public readonly EntityFact Fact;

	public GameLogEventFactsCollection(EventType @event, EntityFact fact)
	{
		Event = @event;
		Fact = fact;
	}
}

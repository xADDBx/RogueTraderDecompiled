using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventScrapCollection : GameLogEvent<GameLogEventScrapCollection>
{
	public enum EventType
	{
		ScrapGained,
		ScrapSpend
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IScrapChangedHandler, ISubscriber
	{
		private void TryAddEvent(EventType type, int count)
		{
			AddEvent(new GameLogEventScrapCollection(type, count));
		}

		public void HandleScrapGained(int scrap)
		{
			TryAddEvent(EventType.ScrapGained, scrap);
		}

		public void HandleScrapSpend(int scrap)
		{
			TryAddEvent(EventType.ScrapSpend, scrap);
		}
	}

	public readonly EventType Event;

	public readonly int Count;

	public GameLogEventScrapCollection(EventType @event, int count)
	{
		Event = @event;
		Count = count;
	}
}

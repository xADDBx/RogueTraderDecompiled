using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventNavigatorResource : GameLogEvent<GameLogEventNavigatorResource>
{
	private class EventsHandler : GameLogController.GameEventsHandler, INavigatorResourceCountChangedHandler, ISubscriber
	{
		private void AddEvent(float value)
		{
			AddEvent(new GameLogEventNavigatorResource(value));
		}

		public void HandleChaneNavigatorResourceCount(int count)
		{
			AddEvent(count);
		}
	}

	public readonly float Value;

	public GameLogEventNavigatorResource(float value)
	{
		Value = value;
	}
}

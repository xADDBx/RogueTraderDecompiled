using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventTimeAdvanced : GameLogEvent<GameLogEventTimeAdvanced>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IGameTimeAdvancedHandler, ISubscriber
	{
		public void HandleGameTimeAdvanced(TimeSpan deltaTime)
		{
			AddEvent(new GameLogEventTimeAdvanced(deltaTime));
		}
	}

	public readonly TimeSpan DeltaTime;

	public GameLogEventTimeAdvanced(TimeSpan deltaTime)
	{
		DeltaTime = deltaTime;
	}
}

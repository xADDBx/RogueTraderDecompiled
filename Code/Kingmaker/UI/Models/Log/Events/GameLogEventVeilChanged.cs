using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventVeilChanged : GameLogEvent<GameLogEventVeilChanged>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IPsychicPhenomenaUIHandler, ISubscriber
	{
		public void HandleVeilThicknessValueChanged(int delta, int value)
		{
			AddEvent(new GameLogEventVeilChanged(delta, value));
		}
	}

	public readonly int Delta = -1;

	public readonly int NewValue = -1;

	public GameLogEventVeilChanged(int delta, int newValue)
	{
		Delta = delta;
		NewValue = newValue;
	}
}

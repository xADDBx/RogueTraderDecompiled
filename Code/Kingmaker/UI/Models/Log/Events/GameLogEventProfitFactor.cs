using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventProfitFactor : GameLogEvent<GameLogEventProfitFactor>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IProfitFactorHandler, ISubscriber
	{
		private void AddEvent(float value)
		{
			AddEvent(new GameLogEventProfitFactor(value));
		}

		public void HandleProfitFactorModifierAdded(float max, ProfitFactorModifier modifier)
		{
			AddEvent(max);
		}

		public void HandleProfitFactorModifierRemoved(float max, ProfitFactorModifier modifier)
		{
			AddEvent(max);
		}
	}

	public readonly float Value;

	public GameLogEventProfitFactor(float value)
	{
		Value = value;
	}
}

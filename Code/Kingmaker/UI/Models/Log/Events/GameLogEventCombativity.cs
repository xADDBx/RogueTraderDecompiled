using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventCombativity : GameLogEvent<GameLogEventCombativity>
{
	private class EventsHandler : GameLogController.GameEventsHandler, ICombativityHandler, ISubscriber
	{
		private void AddEvent(float value)
		{
			AddEvent(new GameLogEventCombativity(value));
		}

		public void HandleCombativityModifierAdded(float max, CombativityModifier modifier)
		{
			AddEvent(max);
		}

		public void HandleCombativityModifierRemoved(float max, CombativityModifier modifier)
		{
			AddEvent(max);
		}
	}

	public readonly float Value;

	public GameLogEventCombativity(float value)
	{
		Value = value;
	}
}

using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventFactionReputation : GameLogEvent<GameLogEventFactionReputation>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IGainFactionReputationHandler, ISubscriber
	{
		public void HandleGainFactionReputation(FactionType factionTypeType, int count)
		{
			AddEvent(new GameLogEventFactionReputation(factionTypeType, count));
		}
	}

	public readonly FactionType FactionType;

	public readonly int Points;

	private GameLogEventFactionReputation(FactionType factionType, int reputationPoints)
	{
		FactionType = factionType;
		Points = reputationPoints;
	}
}

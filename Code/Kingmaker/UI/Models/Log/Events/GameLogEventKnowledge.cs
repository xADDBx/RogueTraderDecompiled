using Kingmaker.Inspect;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventKnowledge : GameLogEvent<GameLogEventKnowledge>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IKnowledgeHandler, ISubscriber
	{
		public void HandleKnowledgeUpdated(InspectUnitsManager.UnitInfo unitInfo)
		{
			AddEvent(new GameLogEventKnowledge(unitInfo));
		}
	}

	public readonly InspectUnitsManager.UnitInfo UnitInfo;

	private GameLogEventKnowledge(InspectUnitsManager.UnitInfo unitInfo)
	{
		UnitInfo = unitInfo;
	}
}

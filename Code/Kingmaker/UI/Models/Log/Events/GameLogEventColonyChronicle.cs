using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventColonyChronicle : GameLogEvent<GameLogEventColonyChronicle>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IColonizationChronicleHandler, ISubscriber
	{
		private void AddEvent(Colony colony, ColonyChronicle chronicle)
		{
			AddEvent(new GameLogEventColonyChronicle(colony, chronicle));
		}

		public void HandleChronicleStarted(Colony colony, BlueprintDialog chronicle)
		{
		}

		public void HandleChronicleFinished(Colony colony, ColonyChronicle chronicle)
		{
			AddEvent(colony, chronicle);
		}
	}

	public readonly Colony Colony;

	public readonly ColonyChronicle Chronicle;

	public GameLogEventColonyChronicle(Colony colony, ColonyChronicle chronicle)
	{
		Colony = colony;
		Chronicle = chronicle;
	}
}

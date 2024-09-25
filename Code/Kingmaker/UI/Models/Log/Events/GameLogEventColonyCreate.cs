using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventColonyCreate : GameLogEvent<GameLogEventColonyCreate>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IColonizationHandler, ISubscriber
	{
		private void AddEvent(Colony colony, PlanetEntity planet)
		{
			AddEvent(new GameLogEventColonyCreate(colony, planet));
		}

		public void HandleColonyCreated(Colony colony, PlanetEntity planetEntity)
		{
			AddEvent(colony, planetEntity);
		}
	}

	public readonly Colony Colony;

	public readonly PlanetEntity Planet;

	public GameLogEventColonyCreate(Colony colony, PlanetEntity entity)
	{
		Colony = colony;
		Planet = entity;
	}
}

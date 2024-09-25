using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventUnitGainExperience : GameLogEvent<GameLogEventUnitGainExperience>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IUnitGainExperienceHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
	{
		public void HandleUnitGainExperience(int gained, bool withSound = false)
		{
			AddEvent(new GameLogEventUnitGainExperience(EventInvokerExtensions.BaseUnitEntity, gained));
		}
	}

	public readonly BaseUnitEntity Unit;

	public readonly int Experience;

	public GameLogEventUnitGainExperience(BaseUnitEntity unit, int experience)
	{
		Unit = unit;
		Experience = experience;
	}
}

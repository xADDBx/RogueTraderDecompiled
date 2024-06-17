using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventPartyGainExperience : GameLogEvent<GameLogEventPartyGainExperience>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IPartyGainExperienceHandler, ISubscriber
	{
		public void HandlePartyGainExperience(int gained, bool isExperienceForDeath)
		{
			AddEvent(new GameLogEventPartyGainExperience(gained, isExperienceForDeath));
		}
	}

	public readonly int Experience;

	public readonly bool IsExperienceForDeath;

	public GameLogEventPartyGainExperience(int experience, bool isExperienceForDeath)
	{
		Experience = experience;
		IsExperienceForDeath = isExperienceForDeath;
	}
}

using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventSpaceCombatExperienceGainedPerArea : GameLogEvent<GameLogEventSpaceCombatExperienceGainedPerArea>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IUISpaceCombatExperienceGainedPerAreaHandler, ISubscriber<IStarshipEntity>, ISubscriber
	{
		public void HandlerOnSpaceCombatExperienceGainedPerArea(int exp)
		{
			StarshipEntity starshipEntity = EventInvokerExtensions.StarshipEntity;
			if (starshipEntity != null)
			{
				AddEvent(new GameLogEventSpaceCombatExperienceGainedPerArea(starshipEntity, exp));
			}
		}
	}

	public readonly EntityRef<StarshipEntity> Starship;

	public readonly int ExperienceGainedPerArea;

	private GameLogEventSpaceCombatExperienceGainedPerArea(StarshipEntity starship, int experienceGainedPerArea)
	{
		Starship = starship;
		ExperienceGainedPerArea = experienceGainedPerArea;
	}
}

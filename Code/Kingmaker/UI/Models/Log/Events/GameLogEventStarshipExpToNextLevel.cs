using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Warhammer.SpaceCombat;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventStarshipExpToNextLevel : GameLogEvent<GameLogEventStarshipExpToNextLevel>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IStarshipExpToNextLevelHandler, ISubscriber<IStarshipEntity>, ISubscriber
	{
		public void HandleStarshipExpToNextLevel(int currentLevel, int expToNextLevel, int gainedExp)
		{
			StarshipEntity starshipEntity = EventInvokerExtensions.StarshipEntity;
			if (starshipEntity != null)
			{
				AddEvent(new GameLogEventStarshipExpToNextLevel(starshipEntity, expToNextLevel, gainedExp));
			}
		}
	}

	public readonly EntityRef<StarshipEntity> Starship;

	public readonly int ExpCurrent;

	public readonly int ExpToNextLevel;

	public readonly int ExpGained;

	private GameLogEventStarshipExpToNextLevel(StarshipEntity starship, int expToNextLevel, int expGained)
	{
		Starship = starship;
		ExpCurrent = starship.Progression.Experience;
		ExpToNextLevel = expToNextLevel;
		ExpGained = expGained;
	}
}

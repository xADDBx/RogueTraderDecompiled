using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Warhammer.SpaceCombat;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventStarshipLevelUp : GameLogEvent<GameLogEventStarshipLevelUp>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IStarshipLevelUpHandler, ISubscriber<IStarshipEntity>, ISubscriber
	{
		public void HandleStarshipLevelUp(int newLevel, LevelUpManager manager)
		{
			AddEvent(new GameLogEventStarshipLevelUp(EventInvokerExtensions.StarshipEntity, newLevel));
		}
	}

	public readonly StarshipEntity Starship;

	public readonly int Level;

	public GameLogEventStarshipLevelUp(StarshipEntity starship, int level)
	{
		Starship = starship;
		Level = level;
	}
}

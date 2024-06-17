using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class FactsCollectionLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventFactsCollection>
{
	public void HandleEvent(GameLogEventFactsCollection evt)
	{
	}
}

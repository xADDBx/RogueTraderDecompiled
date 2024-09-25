using Kingmaker.UI.Models.Log.Events;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class UnitGainExperienceLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventUnitGainExperience>
{
	public void HandleEvent(GameLogEventUnitGainExperience evt)
	{
	}
}

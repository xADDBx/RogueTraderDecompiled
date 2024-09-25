using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class StarshipExpToNextLevelLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventStarshipExpToNextLevel>
{
	public void HandleEvent(GameLogEventStarshipExpToNextLevel evt)
	{
		StarshipEntity entity = evt.Starship.Entity;
		if (entity != null)
		{
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)entity;
			int expCurrent = evt.ExpCurrent;
			GameLogContext.Text = expCurrent.ToString();
			GameLogContext.Count = evt.ExpToNextLevel;
			AddMessage(new CombatLogMessage(LogThreadBase.Strings.StarshipExpToNextLevel.CreateCombatLogMessage(), null, hasTooltip: false));
		}
	}
}

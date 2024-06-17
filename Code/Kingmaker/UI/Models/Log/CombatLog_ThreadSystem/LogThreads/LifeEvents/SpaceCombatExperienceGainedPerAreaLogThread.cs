using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class SpaceCombatExperienceGainedPerAreaLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventSpaceCombatExperienceGainedPerArea>
{
	public void HandleEvent(GameLogEventSpaceCombatExperienceGainedPerArea evt)
	{
		StarshipEntity entity = evt.Starship.Entity;
		if (entity != null)
		{
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)entity;
			GameLogContext.Count = evt.ExperienceGainedPerArea;
			AddMessage(LogThreadBase.Strings.StarshipExperiencePerArea.CreateCombatLogMessage());
		}
	}
}

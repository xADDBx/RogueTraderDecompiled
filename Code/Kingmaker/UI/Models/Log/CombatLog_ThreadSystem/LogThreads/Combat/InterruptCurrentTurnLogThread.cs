using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class InterruptCurrentTurnLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventInterruptCurrentTurn>
{
	public void HandleEvent(GameLogEventInterruptCurrentTurn evt)
	{
		if (evt.Actor.Entity is BaseUnitEntity baseUnitEntity)
		{
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)baseUnitEntity;
			AddMessage(LogThreadBase.Strings.InterruptTurn.CreateCombatLogMessage());
		}
	}
}

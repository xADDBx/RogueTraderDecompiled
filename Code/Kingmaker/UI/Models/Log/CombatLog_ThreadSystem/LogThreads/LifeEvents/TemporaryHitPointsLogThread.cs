using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class TemporaryHitPointsLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventTemporaryHitPoints>
{
	public void HandleEvent(GameLogEventTemporaryHitPoints evt)
	{
		IAbstractUnitEntity entity = evt.Actor.Entity;
		if (entity != null && evt.Amount > 0)
		{
			GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)entity.ToBaseUnitEntity();
			GameLogContext.Text = evt.Buff.Name;
			GameLogContext.Count = evt.Amount;
			GameLogMessage gameLogMessage = ((evt.State == GameLogEventTemporaryHitPoints.States.Add) ? LogThreadBase.Strings.TemporaryHitPointsAdd : LogThreadBase.Strings.TemporaryHitPointsRemove);
			AddMessage(gameLogMessage.CreateCombatLogMessage(new TooltipTemplateBuff(evt.Buff), null, isPerformAttackMessage: false, evt.Buff.Owner));
		}
	}
}

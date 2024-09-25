using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class HealWoundOrTraumaLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventHealWoundOrTrauma>
{
	public void HandleEvent(GameLogEventHealWoundOrTrauma evt)
	{
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Actor.Entity.ToBaseUnitEntity();
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Actor.Entity.ToBaseUnitEntity();
		GameLogContext.Text = evt.Buff.Name;
		GameLogMessage gameLogMessage = null;
		if (evt.Buff.IsTraumas)
		{
			gameLogMessage = LogThreadBase.Strings.HealsTrauma;
		}
		else if (evt.Buff.IsWounds)
		{
			gameLogMessage = LogThreadBase.Strings.HealsWound;
		}
		if (gameLogMessage != null)
		{
			AddMessage(gameLogMessage.CreateCombatLogMessage(new TooltipTemplateBuff(evt.Buff), null, isPerformAttackMessage: false, evt.Buff.Owner));
		}
	}
}

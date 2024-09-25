using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class AnomalyCheckLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventAnomalyCheck>
{
	public void HandleEvent(GameLogEventAnomalyCheck evt)
	{
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Actor;
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)evt.Anomaly;
		GameLogContext.DC = 0;
		GameLogContext.Text = evt.Anomaly.Blueprint.Name;
		if (evt.Type == GameLogEventAnomalyCheck.CheckType.Checked)
		{
			RulePerformSkillCheck skillCheckRule = evt.SkillCheckRule;
			AddMessage(skillCheckRule.ResultIsSuccess ? new CombatLogMessage(LogThreadBase.Strings.AnomalyCheckSuccess.CreateCombatLogMessage(), null, hasTooltip: false) : new CombatLogMessage(LogThreadBase.Strings.AnomalyCheckFail.CreateCombatLogMessage(), null, hasTooltip: false));
		}
		else
		{
			bool flag = evt.Type == GameLogEventAnomalyCheck.CheckType.InteractionSucceed;
			AddMessage(flag ? new CombatLogMessage(LogThreadBase.Strings.AnomalyInteractionSuccess.CreateCombatLogMessage(), null, hasTooltip: false) : new CombatLogMessage(LogThreadBase.Strings.AnomalyInteractionFail.CreateCombatLogMessage(), null, hasTooltip: false));
		}
	}
}

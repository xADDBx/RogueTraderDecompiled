using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class MergeRuleCalculateCanApplyBuffLogThread : LogThreadBase, IGameLogEventHandler<MergeGameLogEvent<GameLogRuleEvent<RuleCalculateCanApplyBuff>>>
{
	public void HandleEvent(MergeGameLogEvent<GameLogRuleEvent<RuleCalculateCanApplyBuff>> evt)
	{
		IReadOnlyList<GameLogRuleEvent<RuleCalculateCanApplyBuff>> events = evt.GetEvents();
		RuleCalculateCanApplyBuff rule = events[0].Rule;
		if (events.Count == 1)
		{
			CombatLogMessage combatLogMessage = RulebookCanApplyBuffLogThread.GetCombatLogMessage(rule);
			AddMessage(combatLogMessage);
			return;
		}
		MechanicEntity caster = rule.Reason.Caster;
		GameLogContext.Text = rule.Context.Name;
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)caster;
		CombatLogMessage combatLogMessage2 = LogThreadBase.Strings.GroupStatusEffect.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, caster);
		if (combatLogMessage2?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			ITooltipBrick[] array = CollectExtraBricks(events).ToArray();
			if (array.Length == 0)
			{
				return;
			}
			if (array.Length == 1)
			{
				CombatLogMessage combatLogMessage3 = RulebookCanApplyBuffLogThread.GetCombatLogMessage(rule);
				AddMessage(combatLogMessage3);
				return;
			}
			tooltipTemplateCombatLogMessage.ExtraTooltipBricks = array;
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = array;
		}
		AddMessage(combatLogMessage2);
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(IEnumerable<GameLogRuleEvent<RuleCalculateCanApplyBuff>> events)
	{
		foreach (GameLogRuleEvent<RuleCalculateCanApplyBuff> @event in events)
		{
			CombatLogMessage combatLogMessage = RulebookCanApplyBuffLogThread.GetCombatLogMessage(@event.Rule);
			if (combatLogMessage != null)
			{
				yield return new TooltipBrickNestedMessage(combatLogMessage);
			}
		}
	}
}

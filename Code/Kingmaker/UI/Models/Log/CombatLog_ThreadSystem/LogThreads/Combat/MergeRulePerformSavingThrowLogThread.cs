using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class MergeRulePerformSavingThrowLogThread : LogThreadBase, IGameLogEventHandler<MergeGameLogEvent<GameLogRuleEvent<RulePerformSavingThrow>>>
{
	public void HandleEvent(MergeGameLogEvent<GameLogRuleEvent<RulePerformSavingThrow>> evt)
	{
		IReadOnlyList<GameLogRuleEvent<RulePerformSavingThrow>> events = evt.GetEvents();
		RulePerformSavingThrow rule = events[0].Rule;
		MechanicEntity caster = rule.Reason.Caster;
		GameLogContext.Text = UIUtility.GetStatText(rule.StatType);
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)caster;
		CombatLogMessage combatLogMessage = LogThreadBase.Strings.SavingThrowGroup.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, caster);
		if (combatLogMessage?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			ITooltipBrick[] array = CollectExtraBricks(events).ToArray();
			if (array.Length == 0)
			{
				return;
			}
			tooltipTemplateCombatLogMessage.ExtraTooltipBricks = array;
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = array;
		}
		AddMessage(combatLogMessage);
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(IEnumerable<GameLogRuleEvent<RulePerformSavingThrow>> events)
	{
		foreach (GameLogRuleEvent<RulePerformSavingThrow> @event in events)
		{
			CombatLogMessage combatLogMessage = RulebookSavingThrowLogThread.GetCombatLogMessage(@event.Rule, ignoreInitiatorDeath: true);
			if (combatLogMessage != null)
			{
				yield return new TooltipBrickNestedMessage(combatLogMessage);
			}
		}
	}
}

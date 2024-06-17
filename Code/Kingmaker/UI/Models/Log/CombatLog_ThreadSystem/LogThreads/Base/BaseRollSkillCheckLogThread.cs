using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Base;

public abstract class BaseRollSkillCheckLogThread : LogThreadBase, IGameLogRuleHandler<RulePerformPartySkillCheck>, IGameLogRuleHandler<RulePerformSkillCheck>
{
	public virtual void HandleEvent(RulePerformPartySkillCheck check)
	{
	}

	public virtual void HandleEvent(RulePerformSkillCheck evt)
	{
		if (!evt.Silent)
		{
			LogRuleSkillCheck(evt);
		}
	}

	private void LogRuleSkillCheck(RulePerformSkillCheck rule)
	{
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteInitiator;
		GameLogContext.Text = UIUtility.GetStatText(rule.StatType);
		CombatLogMessage combatLogMessage = (rule.ResultIsSuccess ? LogThreadBase.Strings.SkillCheckSuccess.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, rule.ConcreteInitiator) : LogThreadBase.Strings.SkillCheckFail.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, rule.ConcreteInitiator));
		if (combatLogMessage?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = (tooltipTemplateCombatLogMessage.ExtraTooltipBricks = CollectExtraBricks(rule).ToArray());
		}
		AddMessage(combatLogMessage);
	}

	public static IEnumerable<ITooltipBrick> CollectExtraBricks(RulePerformSkillCheck rule)
	{
		TooltipBrickStrings s = LogThreadBase.Strings.TooltipBrickStrings;
		int roleChance = Math.Clamp(rule.ResultChanceRule.Chance, 0, 100);
		int sufficientValue = rule.ResultChanceRule.RerollChance ?? roleChance;
		int value = ((rule.ResultChanceRule.RollHistory.Count > 1) ? rule.ResultChanceRule.RollHistory.LastOrDefault() : rule.ResultChanceRule.Result);
		yield return new TooltipBrickChance(s.CheckRoll.Text, sufficientValue, value, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		int nestedLevel = 1;
		if (rule.ResultChanceRule.RollHistory.Count > 1)
		{
			IEnumerable<ITooltipBrick> enumerable = LogThreadBase.ShowReroll(rule.ResultChanceRule, roleChance);
			foreach (ITooltipBrick item in enumerable)
			{
				yield return item;
			}
			nestedLevel++;
		}
		if (rule.BaseDifficulty != 0)
		{
			string value2 = rule.BaseDifficulty + "%";
			yield return new TooltipBrickTextValue(s.BaseModifier.Text, value2, nestedLevel);
		}
		string value3 = ((rule.BaseDifficulty != 0) ? UIUtility.AddSign(rule.StatValue) : rule.StatValue.ToString()) + "%";
		yield return new TooltipBrickTextValue(GameLogContext.Text, value3, nestedLevel);
		foreach (Modifier item2 in rule.DifficultyModifiers.List)
		{
			yield return LogThreadBase.CreateBrickModifier(item2, valueIsPercent: true, null, nestedLevel);
		}
		yield return new TooltipBrickIconTextValue(s.Result, rule.ResultIsSuccess ? s.Success : s.Failure);
	}
}

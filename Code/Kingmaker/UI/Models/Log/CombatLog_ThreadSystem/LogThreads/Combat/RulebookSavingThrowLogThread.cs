using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class RulebookSavingThrowLogThread : LogThreadBase, IGameLogRuleHandler<RulePerformSavingThrow>
{
	public void HandleEvent(RulePerformSavingThrow rule)
	{
		CombatLogMessage combatLogMessage = GetCombatLogMessage(rule, ignoreInitiatorDeath: true);
		if (combatLogMessage != null)
		{
			AddMessage(combatLogMessage);
		}
	}

	public static CombatLogMessage GetCombatLogMessage(RulePerformSavingThrow rule, bool ignoreInitiatorDeath = false)
	{
		if ((!ignoreInitiatorDeath && rule.ConcreteInitiator.IsDead) || rule.AutoPass)
		{
			return null;
		}
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteInitiator;
		GameLogContext.Text = UIUtility.GetStatText(rule.StatType);
		CombatLogMessage obj = (rule.IsPassed ? LogThreadBase.Strings.SavingThrowSuccess.CreateCombatLogMessage(rule, rule.ConcreteInitiator) : LogThreadBase.Strings.SavingThrowFail.CreateCombatLogMessage(rule, rule.ConcreteInitiator));
		if (obj?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = (tooltipTemplateCombatLogMessage.ExtraTooltipBricks = CollectExtraBricks(rule).ToArray());
		}
		return obj;
	}

	public static IEnumerable<ITooltipBrick> CollectExtraBricks(RulePerformSavingThrow rule)
	{
		TooltipBrickStrings s = LogThreadBase.Strings.TooltipBrickStrings;
		if (rule.Reason.Caster != null)
		{
			string value = "<b>" + rule.Reason.Caster.Name + "</b>";
			yield return new TooltipBrickIconTextValue(UIStrings.Instance.Tooltips.Source, value, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		}
		int sufficientValue = Math.Clamp(rule.StatValue + rule.DifficultyClass, 0, 100);
		int value2 = ((rule.D100.RollHistory.Count > 1) ? rule.D100.RollHistory.LastOrDefault() : ((int)rule.D100));
		yield return new TooltipBrickChance(s.CheckRoll.Text, sufficientValue, value2, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		bool alwaysSucceed = rule.AlwaysSucceed;
		bool alwaysFail = rule.AlwaysFail;
		if (alwaysSucceed || alwaysFail)
		{
			string triggeredAutoText = (alwaysSucceed ? UIStrings.Instance.CombatLog.AlwaysSucceed : UIStrings.Instance.CombatLog.AlwaysFail);
			yield return new TooltipBrickTriggeredAuto(triggeredAutoText, GetAlwaysSucceedOrAlwaysFailAssociatedFact(rule), isSuccess: true);
		}
		else
		{
			string value3 = rule.StatValue + "%";
			yield return new TooltipBrickTextValue(GameLogContext.Text, value3, 1);
			if (rule.DifficultyClass != 0)
			{
				yield return new TooltipBrickTextValue(s.BaseModifier.Text, UIUtility.AddSign(rule.DifficultyClass) + "%", 1);
			}
			foreach (Modifier valueModifiers in rule.ValueModifiersList)
			{
				yield return LogThreadBase.CreateBrickModifier(valueModifiers, valueIsPercent: true, null, 1);
			}
		}
		yield return new TooltipBrickIconTextValue(s.Result, rule.IsPassed ? s.Success : s.Failure);
	}

	private static IReadOnlyList<FeatureCountableFlag.BuffList.Element> GetAlwaysSucceedOrAlwaysFailAssociatedFact(RulePerformSavingThrow rule)
	{
		List<FeatureCountableFlag.BuffList.Element> list = new List<FeatureCountableFlag.BuffList.Element>();
		if ((rule.AlwaysSucceed || rule.AlwaysFail) && rule.AlwaysSource != null)
		{
			list.Add(new FeatureCountableFlag.BuffList.Element(rule.AlwaysSource));
		}
		return list;
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class RulebookPerformStarshipAttackLogThread : LogThreadBase, IGameLogRuleHandler<RuleStarshipPerformAttack>, IGameLogEventHandler<MergeGameLogEvent<GameLogRuleEvent<RuleStarshipPerformAttack>>>
{
	public void HandleEvent(RuleStarshipPerformAttack rule)
	{
		CombatLogMessage combatLogMessage = GetCombatLogMessage(rule);
		AddMessage(combatLogMessage);
	}

	private static CombatLogMessage GetCombatLogMessage(RuleStarshipPerformAttack rule, bool isLanceResult = false)
	{
		RuleStarshipRollAttack attackRollRule = rule.AttackRollRule;
		bool flag = !rule.ResultIsHit && attackRollRule.ResultTargetDisruptionMiss;
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.Initiator;
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.Target;
		if (flag)
		{
			GameLogContext.DC = attackRollRule.BonusTargetDisruptionChance;
			GameLogContext.D100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)attackRollRule.TargetDisruptionD100;
			GameLogContext.HitChance = attackRollRule.BonusTargetDisruptionChance;
			GameLogContext.HitD100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)attackRollRule.TargetDisruptionD100;
		}
		else
		{
			GameLogContext.DC = attackRollRule.HitChanceRule.ResultHitChance;
			GameLogContext.D100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)attackRollRule.HitD100;
			GameLogContext.HitChance = attackRollRule.HitChanceRule.ResultHitChance;
			GameLogContext.HitD100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)attackRollRule.HitD100;
		}
		GameLogContext.AttacksCount = attackRollRule.HitChanceRule.ResultCritChance;
		GameLogContext.Roll = rule.ResultDamageBeforeAbsorption;
		RuleRollD100 critD = attackRollRule.CritD100;
		GameLogContext.Rations = ((critD != null) ? ((int)critD) : 0);
		GameLogContext.ChanceDC = rule.ShieldAbsorptionRollRule?.ResultChance ?? 0;
		critD = rule.ShieldAbsorptionRollRule?.D100;
		GameLogContext.AttackNumber = ((critD != null) ? ((int)critD) : 0);
		GameLogContext.Modifier = rule.ResultAbsorbedDamage;
		GameLogContext.RoundNumber = rule.ResultDeflectedDamage;
		GameLogContext.Count = rule.ResultDamage;
		GameLogContext.Penetration = rule.ResultShieldStrengthLoss;
		GameLogContext.Text = rule.Weapon?.Name ?? "";
		bool resultIsCritical = rule.ResultIsCritical;
		bool flag2 = rule.ResultAbsorbedDamage > 0;
		bool flag3 = rule.ResultDamage > 0;
		CombatLogMessage combatLogMessage = ((rule.Weapon == null || !rule.Weapon.IsFocusedEnergyWeapon || rule.NextAttackInBurst == null) ? ((rule.ResultIsHit && resultIsCritical && flag2) ? LogThreadBase.Strings.WarhammerStarshipCriticalShieldHullHit.CreateCombatLogMessage() : ((rule.ResultIsHit && resultIsCritical) ? LogThreadBase.Strings.WarhammerStarshipCriticalHit.CreateCombatLogMessage() : ((rule.ResultIsHit && flag2 && flag3) ? LogThreadBase.Strings.WarhammerStarshipShieldHullHit.CreateCombatLogMessage() : ((rule.ResultIsHit && flag2 && !flag3) ? LogThreadBase.Strings.WarhammerStarshipShieldHit.CreateCombatLogMessage() : ((rule.ResultIsHit && flag3 && !flag2) ? LogThreadBase.Strings.WarhammerStarshipHit.CreateCombatLogMessage() : (flag ? LogThreadBase.Strings.WarhammerStarshipTargetDisruptionMiss.CreateCombatLogMessage() : (rule.IsTorpedoDirectHitAttempt ? LogThreadBase.Strings.WarhammerStarshipMissProximityMayFollow.CreateCombatLogMessage() : LogThreadBase.Strings.WarhammerStarshipMiss.CreateCombatLogMessage()))))))) : ((rule.ResultIsHit && resultIsCritical) ? LogThreadBase.Strings.WarhammerStarshipLanceCrit.CreateCombatLogMessage() : (rule.ResultIsHit ? LogThreadBase.Strings.WarhammerStarshipLanceHit.CreateCombatLogMessage() : (rule.IsTorpedoDirectHitAttempt ? LogThreadBase.Strings.WarhammerStarshipMissProximityMayFollow.CreateCombatLogMessage() : LogThreadBase.Strings.WarhammerStarshipLanceMiss.CreateCombatLogMessage()))));
		if (combatLogMessage?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			tooltipTemplateCombatLogMessage.ExtraTooltipBricks = CollectExtraBricks(rule, isInfotip: false, isLanceResult).ToArray();
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = CollectExtraBricks(rule, isInfotip: true, isLanceResult).ToArray();
		}
		return combatLogMessage;
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(RuleStarshipPerformAttack rule, bool isInfotip = false, bool isLanceResult = false)
	{
		RuleStarshipRollAttack rollRule = rule.AttackRollRule;
		TooltipBrickStrings s = LogThreadBase.Strings.TooltipBrickStrings;
		if (isLanceResult)
		{
			yield break;
		}
		if (GameLogContext.HitD100.Value != null)
		{
			int value = GameLogContext.HitChance.Value;
			int result = GameLogContext.HitD100.Value.Result;
			yield return new TooltipBrickChance(s.HitRoll.Text, value, result, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
			if (rollRule.Weapon.IsAEAmmo)
			{
				yield return new TooltipBrickTriggeredAuto(s.AutoHit.Text, null, isSuccess: true);
				yield return new TooltipBrickTextValue(s.AutoHitAoE.Text, null, 2);
			}
			else
			{
				int num = rollRule.HitChanceRule.CalculateInitiatorHitChances() + rollRule.HitChanceRule.BonusHitChance;
				yield return new TooltipBrickIconTextValue(s.SpaceHitChance.Text, num + "%", 2, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
				if (isInfotip && rollRule.HitChanceRule.BonusHitChance != 0)
				{
					yield return new TooltipBrickTextValue(s.BaseModifier.Text, rollRule.HitChanceRule.CalculateInitiatorHitChances() + "%", 3);
					yield return new TooltipBrickTextValue(s.BonusHit.Text, UIUtility.AddSign(rollRule.HitChanceRule.BonusHitChance).ToString() + "%", 3);
				}
				int num2 = 100 - rollRule.HitChanceRule.ResultEvasionChance;
				float num3 = (float)num2 / 100f;
				yield return new TooltipBrickIconTextValue(s.EffectiveEvasion.Text, "×" + num3.ToString(CultureInfo.InvariantCulture) + "(" + num2 + "%)", 2, isResultValue: false, null, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
				if (isInfotip && rollRule.HitChanceRule.ResultEvasionChance > 0)
				{
					yield return new TooltipBrickTextValue(s.BaseModifier.Text, "100%", 3);
					yield return new TooltipBrickTextValue(s.Evasion.Text, "-" + rollRule.HitChanceRule.ResultEvasionChance + "%", 3);
				}
			}
		}
		DamageData damage = rule.UIDamageData;
		if (rule.ResultIsHit)
		{
			yield return new TooltipBrickDamageRange(s.Damage.Text, rule.UIDamageResult, rule.UIDamageMin, rule.UIDamageMax, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: false, isBeigeBackground: false, isRedBackground: true);
			int effectiveRolledValue = rule.UIInitialDamage;
			yield return new TooltipBrickDamageRange(s.InitialDamage.Text, effectiveRolledValue, rule.UIInitialDamageMin, rule.UIInitialDamageMax, 2, isResultValue: true, "=" + effectiveRolledValue, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
			if (rule.UIIsFocusedEnergyWeapon || damage == null || rule.Target.IsSoftUnit)
			{
				if (isInfotip)
				{
					string value2 = $"{rule.UIBaseDamageMin} — {rule.UIBaseDamageMax}";
					yield return new TooltipBrickTextValue(s.BaseModifier.Text, value2, 3, isResultValue: true);
					if (rule.BonusDamage != 0)
					{
						string value3 = UIUtility.AddSign(rule.BonusDamage) ?? "";
						yield return new TooltipBrickTextValue(s.BonusDamage.Text, value3, 3, isResultValue: true);
					}
					if (rule.ExtraDamageMod != 0f)
					{
						int num4 = Mathf.RoundToInt((float)effectiveRolledValue * (1f + rule.ExtraDamageMod));
						string value4 = "×" + 1 + rule.ExtraDamageMod;
						yield return new TooltipBrickIconTextValue("<b>" + s.ExtraDamage.Text + "</b>", value4, 2, isResultValue: true, "=" + num4, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
					}
					if (Math.Abs(rule.UIDamageDifficultyModifier - 1f) > float.Epsilon)
					{
						int num5 = Mathf.RoundToInt((float)effectiveRolledValue * (1f + rule.ExtraDamageMod) * rule.UIDamageDifficultyModifier);
						string value5 = "×" + rule.UIDamageDifficultyModifier;
						yield return new TooltipBrickIconTextValue("<b>" + s.DamageDifficultyModifier.Text + "</b>", value5, 2, isResultValue: true, "=" + num5, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
					}
				}
				effectiveRolledValue = Mathf.RoundToInt((float)effectiveRolledValue * (1f + rule.ExtraDamageMod) * rule.UIDamageDifficultyModifier);
				if (rule.UIIsFocusedEnergyWeapon)
				{
					if (rule.ResultIsCritical)
					{
						effectiveRolledValue += effectiveRolledValue;
						yield return new TooltipBrickIconTextValue(s.CriticalDamageModifier.Text, "<b>+100%</b>", 2, isResultValue: true, "=" + effectiveRolledValue, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
					}
					yield return new TooltipBrickChance(s.CriticalHit.Text, Math.Clamp(rollRule.HitChanceRule.ResultCritChance, 0, 100), Math.Clamp(rollRule.CritD100.Result, 0, 100), rule.ResultIsCritical ? 3 : 2, isResultValue: true, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
				}
				bool isFullAbsorbed = false;
				RuleStarshipRollShieldAbsorption shieldAbsorptionRollRule = rule.ShieldAbsorptionRollRule;
				if (shieldAbsorptionRollRule != null && shieldAbsorptionRollRule.D100 != null)
				{
					isFullAbsorbed = effectiveRolledValue == rule.ResultAbsorbedDamage;
					yield return new TooltipBrickChance(s.Shield.Text, rule.ShieldAbsorptionRollRule.ResultChance, rule.ShieldAbsorptionRollRule.D100.Result, 2, isResultValue: false, null, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
					if (rule.ResultAbsorbedDamage != 0 && !isFullAbsorbed)
					{
						effectiveRolledValue -= rule.ResultAbsorbedDamage;
						string value6 = UIUtility.AddSign(-rule.ResultAbsorbedDamage);
						yield return new TooltipBrickIconTextValue("<b>" + s.ResultAbsorbedDamage.Text + "</b>", value6, 3, isResultValue: true, "=" + effectiveRolledValue, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
					}
				}
				if (rule.ResultIsCritical && !isFullAbsorbed)
				{
					effectiveRolledValue = effectiveRolledValue * 150 / 100;
					yield return new TooltipBrickIconTextValue(s.CriticalDamageModifier.Text, "<b>+50%</b>", 2, isResultValue: true, "=" + effectiveRolledValue, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
				}
				if (rule.Target.IsSoftUnit)
				{
					int num6 = rule.CalculateDamageRule?.ResultDeflection ?? 0;
					if (num6 != 0)
					{
						effectiveRolledValue -= num6;
						string value7 = UIUtility.AddSign(num6);
						yield return new TooltipBrickIconTextValue("<b>" + s.ResultDeflection.Text + "</b>", value7, 2, isResultValue: true, "=" + effectiveRolledValue, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
					}
				}
			}
		}
		string value8;
		if (damage == null)
		{
			GameLogContext.ResultDamage = rule.ResultDamageBeforeAbsorption;
			value8 = LogThreadBase.Strings.AttackResultStrings.GetAttackResultText(rule.Result);
		}
		else
		{
			value8 = ((rule.ResultIsHit && rule.ResultIsCritical) ? ((string)s.CriticalResultHit) : (rule.ResultIsHit ? ((string)s.ResultHit) : LogThreadBase.Strings.AttackResultStrings.GetAttackResultText(rule.Result)));
		}
		yield return new TooltipBrickIconTextValue(s.Result.Text, value8, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
	}

	public void HandleEvent(MergeGameLogEvent<GameLogRuleEvent<RuleStarshipPerformAttack>> evt)
	{
		IReadOnlyList<GameLogRuleEvent<RuleStarshipPerformAttack>> events = evt.GetEvents();
		if (events.Count <= 0)
		{
			return;
		}
		RuleStarshipPerformAttack rule = events[0].Rule;
		StarshipEntity initiator = rule.Initiator;
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)initiator;
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.Target;
		bool flag = false;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < events.Count; i++)
		{
			RuleStarshipPerformAttack rule2 = events[i].Rule;
			if (i == 0 && rule2.Weapon != null && rule2.Weapon.IsFocusedEnergyWeapon && rule2.NextAttackInBurst != null)
			{
				flag = true;
			}
			if (flag)
			{
				if (i == events.Count - 1)
				{
					num = rule2.ResultAbsorbedDamage;
					num2 = rule2.ResultDamage;
				}
			}
			else
			{
				num += rule2.ResultAbsorbedDamage;
				num2 += rule2.ResultDamage;
			}
		}
		GameLogContext.Count = (flag ? (events.Count - 1) : events.Count);
		GameLogContext.Text = num.ToString();
		GameLogContext.SecondText = num2.ToString();
		CombatLogMessage combatLogMessage = LogThreadBase.Strings.WarhammerStarshipAttackGroup.CreateCombatLogMessage(null, null, isPerformAttackMessage: false, initiator);
		if (combatLogMessage?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = (tooltipTemplateCombatLogMessage.ExtraTooltipBricks = CollectNestedExtraBricks(events, flag).ToArray());
		}
		AddMessage(combatLogMessage);
	}

	private static IEnumerable<ITooltipBrick> CollectNestedExtraBricks(IReadOnlyList<GameLogRuleEvent<RuleStarshipPerformAttack>> events, bool isLance)
	{
		for (int i = 0; i < events.Count; i++)
		{
			RuleStarshipPerformAttack rule = events[i].Rule;
			bool flag = isLance && i == events.Count - 1;
			CombatLogMessage message = GetCombatLogMessage(rule, flag);
			if (message != null)
			{
				if (flag)
				{
					yield return new TooltipBrickSpace(15f);
					yield return new TooltipBrickTitle(UIStrings.Instance.CombatLog.LanceResultTitle);
				}
				else
				{
					message.SetShotNumber(i + 1);
				}
				yield return new TooltipBrickNestedMessage(message);
			}
		}
	}
}

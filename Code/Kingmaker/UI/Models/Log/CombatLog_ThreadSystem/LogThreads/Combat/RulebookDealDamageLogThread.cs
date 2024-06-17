using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.TextTools;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class RulebookDealDamageLogThread : LogThreadBase, IGameLogRuleHandler<RuleDealDamage>, IGameLogEventHandler<GameLogEventDealDamage>
{
	public void HandleEvent(RuleDealDamage rule)
	{
		if (rule.Result != 0 || !(rule.Target is DestructibleEntity))
		{
			bool num = rule.ConcreteInitiator.IsTrap();
			int num2;
			if (rule.SourceAbility != null)
			{
				WarhammerAbilityParamsSource abilityParamsSource = rule.SourceAbility.Blueprint.AbilityParamsSource;
				num2 = ((abilityParamsSource == WarhammerAbilityParamsSource.PsychicPower || abilityParamsSource == WarhammerAbilityParamsSource.NavigatorPower) ? 1 : 0);
			}
			else
			{
				num2 = 0;
			}
			bool flag = (byte)num2 != 0;
			bool flag2 = rule.Reason.Fact is Buff;
			bool flag3 = rule.Reason.Ability != null && rule.Reason.Ability.AbilityGroups.Contains((BlueprintAbilityGroup ag) => ag.name == "PerilsOfWarpAbilityGroup");
			bool flag4 = (rule.SourceAbility != null && rule.SourceAbility.IsScatter) || (rule.Reason.Ability != null && rule.Reason.Ability.IsScatter);
			bool flag5 = (rule.SourceAbility != null && rule.SourceAbility.IsSingleShot) || (rule.Reason.Ability != null && rule.Reason.Ability.IsSingleShot);
			bool flag6 = (rule.SourceAbility != null && rule.SourceAbility.IsMelee) || (rule.Reason.Ability != null && rule.Reason.Ability.IsMelee);
			bool flag7 = (rule.SourceAbility != null && rule.SourceAbility.IsAOE) || (rule.Reason.Ability != null && rule.Reason.Ability.IsAOE);
			if (num || flag || flag2 || flag3 || !(flag4 || flag5 || flag6 || flag7))
			{
				CombatLogMessage newMessage = CreateMessage(rule);
				AddMessage(newMessage);
			}
		}
	}

	public void HandleEvent(GameLogEventDealDamage evt)
	{
		CombatLogMessage newMessage = CreateMessage(evt.Damage);
		AddMessage(newMessage);
	}

	public static CombatLogMessage CreateMessage(RuleDealDamage rule)
	{
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)(MechanicEntity)rule.Target;
		GameLogContext.Description = null;
		GameLogContext.ResultDamage = rule.Result;
		if (rule.Damage != null)
		{
			GameLogContext.DamageType = UIUtilityTexts.GetTextByKey(rule.Damage.Type);
		}
		GameLogContext.Absorption = rule.Damage?.AbsorptionPercentsWithoutPenetration ?? 0;
		GameLogContext.Deflection = rule.Damage?.Deflection.Value ?? 0;
		GameLogContext.Penetration = rule.Damage?.Penetration.Value ?? 0;
		CombatLogMessage combatLogMessage = GetMessage(rule).CreateCombatLogMessage(null, null, isPerformAttackMessage: false, rule.ConcreteTarget);
		if (combatLogMessage?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			tooltipTemplateCombatLogMessage.ExtraTooltipBricks = CollectExtraBricks(rule).ToArray();
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = CollectExtraBricks(rule, isInfotip: true).ToArray();
		}
		return combatLogMessage;
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(RuleDealDamage rule, bool isInfotip = false)
	{
		TooltipBrickStrings s = LogThreadBase.Strings.TooltipBrickStrings;
		DamageValue result = rule.ResultValue;
		DamageData damage = rule.Damage;
		if (rule.Reason.SourceEntity != null)
		{
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.Reason.SourceEntity;
			string value = TextTemplateEngine.Instance.Process("{source}");
			yield return new TooltipBrickIconTextValue(s.DamageSource.Text, value, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
		}
		yield return new TooltipBrickIconTextValue(s.DamageReason.Text, "<b>" + rule.Reason.Name + "</b>", 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		yield return new TooltipBrickDamageRange(s.Damage.Text, result.FinalValue, damage.MinValue, damage.MaxValue, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: false, isBeigeBackground: false, isRedBackground: true);
		int effectiveRolledValue = damage.InitialRolledValue;
		yield return new TooltipBrickDamageRange(s.BaseDamage.Text, effectiveRolledValue, damage.MinInitialValue, damage.MaxInitialValue, 2, isResultValue: true, "=" + effectiveRolledValue, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
		if (isInfotip)
		{
			string value2 = damage.MinValueBase + " — " + damage.MaxValueBase;
			yield return new TooltipBrickTextValue(s.BaseModifier.Text, value2, 2, isResultValue: true);
			IEnumerable<ITooltipBrick> damageModifiers = LogThreadBase.GetDamageModifiers(damage, 2, minMax: true, common: true);
			foreach (ITooltipBrick item in damageModifiers)
			{
				yield return item;
			}
		}
		if (damage.Overpenetrating && !damage.UnreducedOverpenetration)
		{
			effectiveRolledValue = Mathf.RoundToInt((float)result.RolledValue * damage.EffectiveOverpenetrationFactor);
			yield return new TooltipBrickIconTextValue(s.OverpenetrationModifier.Text, "<b>×" + damage.EffectiveOverpenetrationFactor.ToString(CultureInfo.InvariantCulture) + "</b>", 2, isResultValue: true, "=" + effectiveRolledValue, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
		}
		if (!rule.RollDamageRule.ArmorIgnore)
		{
			if (damage.Deflection.Value != 0)
			{
				effectiveRolledValue = Mathf.Max(0, effectiveRolledValue - damage.Deflection.Value);
				yield return new TooltipBrickIconTextValue(s.DamageDeflection.Text, "<b>-" + damage.Deflection.Value + "</b>", 2, isResultValue: true, "=" + effectiveRolledValue, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
				if (isInfotip)
				{
					IEnumerable<ITooltipBrick> enumerable = LogThreadBase.CreateBrickModifiers(damage.Deflection.AllModifiersList, valueIsPercent: false, null, 2, isResultValue: false, isFirstWithoutPlus: true);
					foreach (ITooltipBrick item2 in enumerable)
					{
						yield return item2;
					}
				}
			}
			if ((int)GameLogContext.Absorption > 0)
			{
				effectiveRolledValue = (int)((float)effectiveRolledValue * damage.AbsorptionFactorWithPenetration);
				yield return new TooltipBrickIconTextValue(s.EffectiveArmour.Text, "<b>×" + damage.AbsorptionFactorWithPenetration.ToString(CultureInfo.InvariantCulture) + " (" + damage.AbsorptionFactorWithPenetration * 100f + "%)</b>", 2, isResultValue: true, "=" + effectiveRolledValue, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
				if (isInfotip)
				{
					yield return new TooltipBrickIconTextValue("<b>" + s.BaseModifier.Text + "</b>", "<b>100%</b>", 3, isResultValue: true, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
					yield return new TooltipBrickIconTextValue("<b>" + s.Armor.Text + "</b>", "<b>-" + GameLogContext.Absorption.ToString() + "%</b>", 3, isResultValue: true, null, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
					IEnumerable<ITooltipBrick> enumerable2 = LogThreadBase.CreateBrickModifiers(damage.Absorption.AllModifiersList, valueIsPercent: true, null, 3, isResultValue: true, isFirstWithoutPlus: true);
					if (enumerable2.Count() > 1)
					{
						foreach (ITooltipBrick item3 in enumerable2)
						{
							yield return item3;
						}
					}
					yield return new TooltipBrickIconTextValue("<b>" + s.Penetration.Text + "</b>", "<b>+" + GameLogContext.Penetration.ToString() + "%</b>", 3, isResultValue: true, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
					enumerable2 = LogThreadBase.CreateBrickModifiers(damage.Penetration.AllModifiersList, valueIsPercent: true, null, 3, isResultValue: true, isFirstWithoutPlus: true);
					if (enumerable2.Count() > 1)
					{
						foreach (ITooltipBrick item4 in enumerable2)
						{
							yield return item4;
						}
					}
				}
			}
		}
		NullifyInformation nullifyInformation = rule.RollDamageRule.NullifyInformation;
		if (nullifyInformation != null && nullifyInformation.HasDamageChance)
		{
			yield return LogThreadBase.ShowTooltipBrickDamageNullifier(nullifyInformation, rule.Result);
		}
		else if (rule.RollDamageRule.MinimumDamageValue > result.FinalValue)
		{
			yield return new TooltipBrickMinimalAdmissibleDamage(rule.RollDamageRule.MinimumDamageValue);
		}
		yield return new TooltipBrickIconTextValue(value: (nullifyInformation == null || !nullifyInformation.HasDamageNullify) ? LogThreadBase.Strings.AttackResultStrings.GetAttackResultText(AttackResult.Hit) : GameLogStrings.Instance.TooltipBrickStrings.NullifierResultSuccess.Text, name: s.Result.Text, nestedLevel: 1, isResultValue: false, resultValue: null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
	}

	private static GameLogMessage GetMessage(RuleDealDamage rule)
	{
		return LogThreadBase.Strings.WarhammerDealDamage;
	}
}

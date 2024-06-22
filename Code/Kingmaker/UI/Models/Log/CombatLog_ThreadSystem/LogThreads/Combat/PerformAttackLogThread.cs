using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Interfaces;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.SurfaceCombatHUD;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;

public class PerformAttackLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventAttack>
{
	public void HandleEvent(GameLogEventAttack evt)
	{
		RulePerformAttack rule = evt.Rule;
		if (rule.Ability != null && rule.Ability.Blueprint.AbilityTag == AbilityTag.ThrowingGrenade)
		{
			return;
		}
		if (evt.TargetDamageList != null)
		{
			foreach (GameLogRuleEvent<RuleDealDamage> targetDamage in evt.TargetDamageList)
			{
				AddMessage(CreateMessage(evt, targetDamage.Rule));
			}
			return;
		}
		AddMessage(CreateMessage(evt));
	}

	public static CombatLogMessage CreateMessage(GameLogEventAttack evt, RuleDealDamage overrideDealDamage = null)
	{
		RulePerformAttack rule = evt.Rule;
		RuleDealDamage ruleDealDamage = overrideDealDamage ?? evt.Rule.ResultDamageRule;
		GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteInitiator;
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)rule.ConcreteTarget;
		GameLogContext.HitD100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)rule.RollPerformAttackRule.ResultChanceRule;
		GameLogContext.DodgeD100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)(rule.ResultDodgeRule?.ResultChanceRule);
		GameLogContext.ParryD100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)(rule.ResultParryRule?.RollChanceRule);
		RuleRollD100 ruleRollD = rule.RollPerformAttackRule.ResultRighteousFuryD100;
		if (ruleRollD == null && evt.Attacker != null)
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				ruleRollD = new RuleRollD100(evt.Attacker);
				Rulebook.Trigger(ruleRollD);
			}
		}
		GameLogContext.RfD100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)ruleRollD;
		GameLogContext.CoverHitD100 = (GameLogContext.Property<IRuleRollD100>)(IRuleRollD100)(rule.RollPerformAttackRule.ResultRollCoverHitRule?.ResultD100);
		GameLogContext.HitChance = rule.RollPerformAttackRule.HitChanceRule.ResultHitChance;
		GameLogContext.DodgeChance = rule.RollPerformAttackRule.ResultDodgeRule?.ChancesRule.Result ?? 0;
		GameLogContext.ParryChance = rule.RollPerformAttackRule.ResultParryRule?.ChancesRule.Result ?? 0;
		GameLogContext.RfChance = ((rule.RollPerformAttackRule.HitChanceRule.ResultRighteousFuryChance > 0) ? rule.RollPerformAttackRule.HitChanceRule.ResultRighteousFuryChance : rule.RollPerformAttackRule.HitChanceRule.RighteousFuryChanceRule.RawResult);
		GameLogContext.CoverHitChance = rule.RollPerformAttackRule.HitChanceRule.ResultCoverHitChanceRule?.ResultChance ?? 0;
		GameLogContext.TargetSuperiorityPenalty = rule.RollPerformAttackRule.HitChanceRule.ResultTargetSuperiorityPenalty * 2;
		GameLogContext.PreMitigationDamage = ruleDealDamage?.ResultWithoutReduction ?? 0;
		GameLogContext.Absorption = ruleDealDamage?.Damage.AbsorptionPercentsWithoutPenetration ?? 0;
		GameLogContext.Deflection = ruleDealDamage?.Damage.Deflection.Value ?? 0;
		GameLogContext.Penetration = ruleDealDamage?.Damage.Penetration.Value ?? 0;
		GameLogContext.AbsorptionWithPenetration = ruleDealDamage?.Damage.AbsorptionPercentsWithPenetration ?? 0;
		GameLogContext.ResultDamage = ruleDealDamage?.Result ?? rule.ResultDamageValue;
		if (ruleDealDamage != null)
		{
			GameLogContext.DamageType = UIUtilityTexts.GetTextByKey(ruleDealDamage.Damage.Type);
		}
		GameLogContext.AttackNumber = rule.BurstIndex + 1;
		AbilityTargetUIData orCreate = AbilityTargetUIDataCache.Instance.GetOrCreate(rule.Ability, evt.Target, evt.Attacker.Position);
		GameLogContext.TotalHitChance = Mathf.RoundToInt((orCreate.BurstHitChances != null && orCreate.BurstHitChances.Count > rule.BurstIndex && rule.BurstIndex >= 0) ? orCreate.BurstHitChances[rule.BurstIndex] : orCreate.HitWithAvoidanceChance);
		GameLogContext.Text = null;
		GameLogContext.Tooltip = null;
		GameLogContext.Description = (evt.IsOverpenetrationTrigger ? LogThreadBase.Strings.TooltipBrickStrings.TriggersOverpenetration.Text : null);
		CombatLogMessage combatLogMessage = GetMessage(evt, overrideDealDamage).CreateCombatLogMessage(null, null, isPerformAttackMessage: true, rule.ConcreteTarget);
		if (combatLogMessage?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			tooltipTemplateCombatLogMessage.ExtraTooltipBricks = CollectExtraBricks(evt, overrideDealDamage).ToArray();
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = CollectExtraBricks(evt, overrideDealDamage, isInfotip: true).ToArray();
		}
		_ = (int)GameLogContext.AttacksCount;
		int shotNumber = ((GameLogContext.AttacksCount.Value > 1) ? GameLogContext.AttackNumber.Value : 0);
		if (combatLogMessage != null)
		{
			combatLogMessage.SetShotNumber(shotNumber);
			return combatLogMessage;
		}
		return combatLogMessage;
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(GameLogEventAttack evt, RuleDealDamage overrideDealDamage = null, bool isInfotip = false)
	{
		RulePerformAttack rule = evt.Rule;
		RuleDealDamage resultDamage = overrideDealDamage ?? evt.Rule.ResultDamageRule;
		TooltipBrickStrings s = LogThreadBase.Strings.TooltipBrickStrings;
		bool isDodge = GameLogContext.DodgeD100.Value != null && rule.ResultDodgeRule != null;
		bool isGrenade = rule.Ability.Blueprint.AbilityTag == AbilityTag.ThrowingGrenade;
		int num = GameLogContext.TotalHitChance.Value;
		if (isGrenade)
		{
			num = ((!isDodge) ? 100 : (100 - GameLogContext.DodgeChance.Value));
		}
		yield return new TooltipBrickTextSignatureValue(s.HitChance.Text, s.HitChanceSignature.Text, "<b>" + num + "%</b>");
		if (GameLogContext.HitD100.Value != null)
		{
			int sufficientValue = rule.RollPerformAttackRule.ResultChanceRule.RerollChance ?? GameLogContext.HitChance.Value;
			if (isGrenade)
			{
				sufficientValue = 100;
			}
			int value = ((rule.RollPerformAttackRule.ResultChanceRule.RollHistory.Count > 1) ? rule.RollPerformAttackRule.ResultChanceRule.RollHistory.LastOrDefault() : GameLogContext.HitD100.Value.Result);
			yield return new TooltipBrickChance(s.HitRoll.Text, sufficientValue, value, 2, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
			if (isInfotip)
			{
				if (rule.RollPerformAttackRule.HitChanceRule.IsAutoHit && rule.InitiatorUnit != null)
				{
					yield return new TooltipBrickTriggeredAuto(s.AutoHit.Text, evt.GetAutoHitAssociatedBuffs(), isSuccess: true);
				}
				else if (rule.RollPerformAttackRule.HitChanceRule.IsAutoMiss && rule.InitiatorUnit != null)
				{
					yield return new TooltipBrickTriggeredAuto(s.AutoMiss.Text, evt.GetAutoMissAssociatedBuffs(), isSuccess: false);
				}
				else if (rule.RollPerformAttackRule.HitChanceRule.IsMelee)
				{
					yield return new TooltipBrickTriggeredAuto(s.AutoHit.Text, null, isSuccess: true);
					yield return new TooltipBrickTextValue(s.AutoHitMelee.Text, null, 2);
				}
				else if (!rule.Ability.IsBurstAttack && rule.Target is DestructibleEntity)
				{
					yield return new TooltipBrickTriggeredAuto(s.AutoHit.Text, null, isSuccess: true);
					yield return new TooltipBrickTextValue(s.AutoHitDestructible.Text, null, 2);
				}
				else if (rule.RollPerformAttackRule.HitChanceRule.IsScatter)
				{
					yield return new TooltipBrickTriggeredAuto(s.AutoHit.Text, null, isSuccess: true);
					yield return new TooltipBrickTextValue(s.AutoHitScatter.Text, null, 2);
				}
				else if (isGrenade)
				{
					yield return new TooltipBrickTriggeredAuto(s.AutoHit.Text, null, isSuccess: true);
					yield return new TooltipBrickTextValue(s.AutoHitGrenade.Text, null, 2);
				}
				else
				{
					if (rule.RollPerformAttackRule.ResultChanceRule.RollHistory.Count > 1)
					{
						IEnumerable<ITooltipBrick> enumerable = LogThreadBase.ShowReroll(rule.RollPerformAttackRule.ResultChanceRule, GameLogContext.HitChance.Value, isTargetHitIcon: true);
						foreach (ITooltipBrick item in enumerable)
						{
							yield return item;
						}
					}
					if (rule.RollPerformAttackRule.HitChanceRule.ResultBallisticSkill > 0)
					{
						yield return new TooltipBrickTextValue(s.BaseModifier.Text, "30%", 2);
						yield return new TooltipBrickTextValue(LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerBallisticSkill), "+" + rule.RollPerformAttackRule.HitChanceRule.InitiatorBallisticSkill + "%", 2);
						if (rule.RollPerformAttackRule.HitChanceRule.BallisticSkillPenalty != 0)
						{
							yield return new TooltipBrickTextValue(s.BallisticSkillPenalty.Text, "-" + rule.RollPerformAttackRule.HitChanceRule.BallisticSkillPenalty + "%", 2);
						}
					}
					if (rule.RollPerformAttackRule.HitChanceRule.DistanceFactor < 1f)
					{
						yield return new TooltipBrickTextValue(s.DistanceFactor.Text, "×" + rule.RollPerformAttackRule.HitChanceRule.DistanceFactor.ToString(CultureInfo.InvariantCulture) + " (" + rule.RollPerformAttackRule.HitChanceRule.DistanceFactor * 100f + "%)", 2);
					}
					IEnumerable<ITooltipBrick> enumerable2 = LogThreadBase.CreateBrickModifiers(rule.RollPerformAttackRule.HitChanceRule.AllModifiersList, valueIsPercent: true, null, 2);
					foreach (ITooltipBrick item2 in enumerable2)
					{
						yield return item2;
					}
					if (rule.RollPerformAttackRule.HitChanceRule.OverpenetrationModifier < 1f)
					{
						yield return new TooltipBrickTextValue(s.OverpenetrationModifier.Text, "×" + rule.RollPerformAttackRule.HitChanceRule.OverpenetrationModifier.ToString(CultureInfo.InvariantCulture), 2);
					}
					yield return LogThreadBase.MinMaxChanceBorder(rule.RollPerformAttackRule.HitChanceRule.RawResult, BlueprintRoot.Instance.WarhammerRoot.CombatRoot.HitChanceOverkillBorder);
				}
			}
		}
		if (GameLogContext.CoverHitD100.Value != null && GameLogContext.CoverHitChance.Value > 0)
		{
			yield return new TooltipBrickChance(s.CoverHit.Text, GameLogContext.CoverHitChance.Value, GameLogContext.CoverHitD100.Value.Result, 2, isResultValue: false, null, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
		}
		bool isDodgeSuccess = false;
		if (isDodge)
		{
			int sufficientValue2 = rule.ResultDodgeRule.ResultChanceRule.RerollChance ?? GameLogContext.DodgeChance.Value;
			int value2 = ((rule.ResultDodgeRule.ResultChanceRule.RollHistory.Count > 1) ? rule.ResultDodgeRule.ResultChanceRule.RollHistory.LastOrDefault() : GameLogContext.DodgeD100.Value.Result);
			yield return new TooltipBrickChance(s.Dodge.Text, sufficientValue2, value2, 2, isResultValue: false, null, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
			if (rule.ResultDodgeRule.ChancesRule.IsAutoDodge && rule.TargetUnit != null)
			{
				yield return new TooltipBrickTriggeredAuto(s.AutoDodge.Text, evt.GetAutoDodgeAssociatedBuffs(), isSuccess: true);
			}
			else
			{
				isDodgeSuccess = rule.ResultDodgeRule.Result;
				if (isInfotip)
				{
					if (rule.ResultDodgeRule.ResultChanceRule.RollHistory.Count > 1)
					{
						IEnumerable<ITooltipBrick> enumerable3 = LogThreadBase.ShowReroll(rule.ResultDodgeRule.ResultChanceRule, GameLogContext.DodgeChance.Value, isTargetHitIcon: false, isProtectionIcon: true);
						foreach (ITooltipBrick item3 in enumerable3)
						{
							yield return item3;
						}
					}
					yield return new TooltipBrickTextValue(s.BaseModifier.Text, rule.ResultDodgeRule.ChancesRule.BaseValue + "%", 2);
					IEnumerable<ITooltipBrick> enumerable4 = CreateDogeBrickModifiers(rule.ResultDodgeRule.ChancesRule.AllModifiersList, rule.ResultDodgeRule.ChancesRule.WeaponDodgePenetrationModifiers, valueIsPercent: true, null, 2);
					foreach (ITooltipBrick item4 in enumerable4)
					{
						yield return item4;
					}
					yield return LogThreadBase.MinMaxChanceBorder(rule.ResultDodgeRule.ChancesRule.RawResult, BlueprintRoot.Instance.WarhammerRoot.CombatRoot.HitChanceOverkillBorder);
				}
			}
		}
		bool isParrySuccess = false;
		if (GameLogContext.ParryD100.Value != null && rule.ResultParryRule != null)
		{
			int sufficientValue3 = rule.ResultParryRule.RollChanceRule.RerollChance ?? GameLogContext.ParryChance.Value;
			int value3 = ((rule.ResultParryRule.RollChanceRule.RollHistory.Count > 1) ? rule.ResultParryRule.RollChanceRule.RollHistory.LastOrDefault() : GameLogContext.ParryD100.Value.Result);
			yield return new TooltipBrickChance(s.Parry.Text, sufficientValue3, value3, 2, isResultValue: false, null, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
			if (rule.ResultParryRule.ChancesRule.IsAutoParry && rule.TargetUnit != null)
			{
				yield return new TooltipBrickTriggeredAuto(s.AutoParry.Text, evt.GetAutoParryAssociatedBuffs(), isSuccess: true);
			}
			else
			{
				isParrySuccess = rule.ResultParryRule.Result;
				if (isInfotip)
				{
					if (rule.ResultParryRule.RollChanceRule.RollHistory.Count > 1)
					{
						IEnumerable<ITooltipBrick> enumerable5 = LogThreadBase.ShowReroll(rule.ResultParryRule.RollChanceRule, GameLogContext.ParryChance.Value, isTargetHitIcon: false, isProtectionIcon: true);
						foreach (ITooltipBrick item5 in enumerable5)
						{
							yield return item5;
						}
					}
					yield return new TooltipBrickTextValue(s.DefenderWeaponSkill.Text, rule.ResultParryRule.ChancesRule.DefenderSkill + "%", 2);
					yield return new TooltipBrickTextValue(s.AttackerWeaponSkill.Text, "-" + rule.ResultParryRule.ChancesRule.AttackerWeaponSkill + "%", 2);
					if (rule.RollPerformAttackRule.HitChanceRule.ResultSuperiorityNumber > 0)
					{
						yield return new TooltipBrickTextValue(s.Superiority.Text, "-" + rule.RollPerformAttackRule.HitChanceRule.ResultSuperiorityNumber + "x" + 10, 2);
					}
					yield return new TooltipBrickTextValue(s.BaseMultiplier.Text, "+" + 20.ToString(CultureInfo.InvariantCulture) + "%", 2);
					IEnumerable<ITooltipBrick> enumerable6 = LogThreadBase.CreateBrickModifiers(rule.ResultParryRule.ChancesRule.AllModifiersList, valueIsPercent: true, null, 2);
					foreach (ITooltipBrick item6 in enumerable6)
					{
						yield return item6;
					}
					yield return LogThreadBase.MinMaxChanceBorder(rule.ResultParryRule.ChancesRule.RawResult, BlueprintRoot.Instance.WarhammerRoot.CombatRoot.HitChanceOverkillBorder);
				}
			}
		}
		if (resultDamage != null && !resultDamage.Damage.CalculatedValue.HasValue)
		{
			bool autoCrit = rule.RollPerformAttackRule.HitChanceRule.AutoCrits.Value;
			int criticalNestedLevel = 1;
			yield return new TooltipBrickChance(s.CriticalHit.Text, Math.Clamp(autoCrit ? 100 : GameLogContext.RfChance.Value, 0, 100), Math.Clamp(GameLogContext.RfD100.Value.Result, 0, 100), criticalNestedLevel, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: false, isBeigeBackground: true);
			if (isInfotip)
			{
				if (autoCrit)
				{
					List<FeatureCountableFlag.BuffList.Element> list = new List<FeatureCountableFlag.BuffList.Element>();
					foreach (Modifier item7 in rule.RollPerformAttackRule.HitChanceRule.AutoCrits.List)
					{
						if (item7.Fact != null)
						{
							list.Add(new FeatureCountableFlag.BuffList.Element(item7.Fact));
						}
						else if (item7.Item != null)
						{
							list.Add(new FeatureCountableFlag.BuffList.Element(item7.Item));
						}
					}
					yield return new TooltipBrickTriggeredAuto(s.AutoCrit.Text, list, isSuccess: true);
				}
				else if (rule.RollPerformAttackRule.HitChanceRule.IsMelee)
				{
					IEnumerable<ITooltipBrick> enumerable7 = CreateMeleeCritHitModifiers(evt.InitiatorWeaponSkillModifierValues, evt.TargetWeaponSkillModifierValues, rule.RollPerformAttackRule.HitChanceRule, criticalNestedLevel + 1);
					foreach (ITooltipBrick item8 in enumerable7)
					{
						yield return item8;
					}
				}
				else
				{
					IEnumerable<ITooltipBrick> enumerable8 = LogThreadBase.CreateBrickModifiers(rule.RollPerformAttackRule.HitChanceRule.RighteousFuryChanceRule.AllModifiersList, valueIsPercent: true, null, criticalNestedLevel, isResultValue: false, isFirstWithoutPlus: true);
					foreach (ITooltipBrick item9 in enumerable8)
					{
						yield return item9;
					}
					yield return LogThreadBase.MinMaxChanceBorder(rule.RollPerformAttackRule.HitChanceRule.RighteousFuryChanceRule.RawResult, 100, criticalNestedLevel);
				}
			}
		}
		int? assassinLethality = evt.AssassinLethality;
		if (assassinLethality.HasValue && assassinLethality.GetValueOrDefault() > 0)
		{
			yield return new TooltipBrickIconTextValue("<b>" + s.AssassinLethality.Text + "</b>", "<b>" + assassinLethality.Value + "</b>", 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: false, isBeigeBackground: true);
		}
		if (resultDamage != null)
		{
			DamageValue result = resultDamage.ResultValue;
			DamageData damage = resultDamage.Damage;
			yield return new TooltipBrickDamageRange(s.Damage.Text, result.FinalValue, damage.MinValue, damage.MaxValue, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: false, isBeigeBackground: false, isRedBackground: true);
			yield return new TooltipBrickDamageRange(s.BaseDamage.Text, damage.BaseRolledValue, damage.MinValueBaseWithMinModifiers, damage.MaxValueBaseWithMaxModifiers, 2, isResultValue: true, "=" + damage.BaseRolledValue, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
			IEnumerable<ITooltipBrick> minMaxModifiers = LogThreadBase.GetDamageModifiers(resultDamage.Damage, 2, minMax: true, common: false);
			if (isInfotip && minMaxModifiers.Any())
			{
				yield return new TooltipBrickTextValue(s.BaseModifier.Text, damage.MinValueBase + " — " + damage.MaxValueBase, 2, isResultValue: true);
				foreach (ITooltipBrick item10 in minMaxModifiers)
				{
					yield return item10;
				}
			}
			if (!damage.CalculatedValue.HasValue && damage.IsCritical)
			{
				int num2 = damage.BaseRolledValue + damage.CriticalRolledValue;
				yield return new TooltipBrickIconTextValue(s.CriticalDamageModifier.Text, "<b>+" + damage.CriticalRolledValue + "</b>", 2, isResultValue: true, "=" + num2, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
				if (isInfotip)
				{
					IEnumerable<ITooltipBrick> enumerable9 = LogThreadBase.CreateBrickModifiers(damage.CriticalDamageModifiers.AllModifiersList, valueIsPercent: false, null, 2, isResultValue: true);
					foreach (ITooltipBrick item11 in enumerable9)
					{
						yield return item11;
					}
				}
			}
			int criticalNestedLevel = damage.InitialRolledValue + damage.CriticalRolledValue;
			IEnumerable<ITooltipBrick> initialDamageModifiers = LogThreadBase.GetDamageModifiers(resultDamage.Damage, 2, minMax: false, common: true);
			if (initialDamageModifiers.Any())
			{
				int num3 = damage.InitialRolledValue - damage.BaseRolledValue;
				yield return new TooltipBrickIconTextValue(s.AdditionalDamage.Text, "<b>" + UIUtility.AddSign(num3) + "</b>", 2, isResultValue: true, "=" + criticalNestedLevel, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
				if (isInfotip)
				{
					foreach (ITooltipBrick item12 in initialDamageModifiers)
					{
						yield return item12;
					}
				}
			}
			if (damage.Overpenetrating && !damage.UnreducedOverpenetration)
			{
				criticalNestedLevel = Mathf.RoundToInt((float)result.RolledValue * damage.EffectiveOverpenetrationFactor);
				yield return new TooltipBrickIconTextValue(s.Overpenetration.Text, "<b>×" + damage.EffectiveOverpenetrationFactor.ToString(CultureInfo.InvariantCulture) + "</b>", 2, isResultValue: true, "=" + criticalNestedLevel, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
			}
			if (!resultDamage.RollDamageRule.ArmorIgnore)
			{
				bool flag = damage.Deflection.AllModifiersList.Any((Modifier m) => m.Value != 0);
				if (damage.Deflection.Value != 0 || flag)
				{
					criticalNestedLevel = Mathf.Max(0, criticalNestedLevel - damage.Deflection.Value);
					yield return new TooltipBrickIconTextValue(s.DamageDeflection.Text, "<b>" + UIUtility.AddSign(damage.Deflection.Value) + "</b>", 2, isResultValue: true, "=" + criticalNestedLevel, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
					if (isInfotip)
					{
						IEnumerable<ITooltipBrick> enumerable10 = LogThreadBase.CreateBrickModifiers(damage.Deflection.AllModifiersList, valueIsPercent: false, null, 2, isResultValue: true, isFirstWithoutPlus: true);
						foreach (ITooltipBrick item13 in enumerable10)
						{
							yield return item13;
						}
					}
				}
				if ((int)GameLogContext.Absorption > 0)
				{
					criticalNestedLevel = (int)((float)criticalNestedLevel * damage.AbsorptionFactorWithPenetration);
					yield return new TooltipBrickIconTextValue(s.EffectiveArmour.Text, "<b>×" + damage.AbsorptionFactorWithPenetration.ToString(CultureInfo.InvariantCulture) + " (" + damage.AbsorptionFactorWithPenetration * 100f + "%)</b>", 2, isResultValue: true, "=" + criticalNestedLevel, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
					if (isInfotip && (rule.Ability.Weapon != null || isGrenade))
					{
						yield return new TooltipBrickIconTextValue("<b>" + s.BaseModifier.Text + "</b>", "<b>100%</b>", 3, isResultValue: true, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
						yield return new TooltipBrickIconTextValue("<b>" + s.Armor.Text + "</b>", "<b>-" + GameLogContext.Absorption.ToString() + "%</b>", 3, isResultValue: true, null, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
						IEnumerable<ITooltipBrick> enumerable11 = LogThreadBase.CreateBrickModifiers(damage.Absorption.AllModifiersList, valueIsPercent: true, null, 3, isResultValue: true, isFirstWithoutPlus: true);
						if (enumerable11.Count() > 1)
						{
							foreach (ITooltipBrick item14 in enumerable11)
							{
								yield return item14;
							}
						}
						yield return new TooltipBrickIconTextValue("<b>" + s.Penetration.Text + "</b>", "<b>+" + GameLogContext.Penetration.ToString() + "%</b>", 3, isResultValue: true, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
						enumerable11 = LogThreadBase.CreateBrickModifiers(damage.Penetration.AllModifiersList, valueIsPercent: true, null, 3, isResultValue: true, isFirstWithoutPlus: true);
						if (enumerable11.Count() > 1)
						{
							foreach (ITooltipBrick item15 in enumerable11)
							{
								yield return item15;
							}
						}
						int num4 = 100 - (int)GameLogContext.Absorption + (int)GameLogContext.Penetration;
						int minValue = 10;
						yield return LogThreadBase.AddMinMaxValue(num4, 3, minValue, isResultValue: true);
					}
				}
			}
		}
		NullifyInformation nullifyInformation = resultDamage?.RollDamageRule.NullifyInformation ?? null;
		if (nullifyInformation != null && nullifyInformation.HasDamageChance)
		{
			yield return LogThreadBase.ShowTooltipBrickDamageNullifier(nullifyInformation, resultDamage.ResultValue.FinalValue);
		}
		else if (resultDamage != null && resultDamage.RollDamageRule.UIMinimumDamageValue > resultDamage.ResultValue.FinalValue)
		{
			yield return new TooltipBrickMinimalAdmissibleDamage(resultDamage.RollDamageRule.UIMinimumDamageValue, resultDamage.RollDamageRule.UIMinimumDamageValue.ToString());
		}
		else if (resultDamage != null && resultDamage.Result > resultDamage.ResultValue.FinalValue && resultDamage.RollDamageRule.UIMinimumDamagePercent > 0)
		{
			yield return new TooltipBrickMinimalAdmissibleDamage(resultDamage.Result, ">" + resultDamage.RollDamageRule.UIMinimumDamagePercent + "%");
		}
		string value4;
		if (nullifyInformation != null && nullifyInformation.HasDamageNullify)
		{
			value4 = GameLogStrings.Instance.TooltipBrickStrings.NullifierResultSuccess.Text;
		}
		else if (isDodgeSuccess && isParrySuccess)
		{
			value4 = LogThreadBase.Strings.AttackResultStrings.AttackResultDodgeParried;
		}
		else
		{
			AttackResult result2 = rule.Result;
			if (rule.ResultIsCoverHit && !(rule.ConcreteTarget is DestructibleEntity))
			{
				result2 = AttackResult.Hit;
			}
			value4 = LogThreadBase.Strings.AttackResultStrings.GetAttackResultText(result2);
		}
		yield return new TooltipBrickIconTextValue(s.Result.Text, value4, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
	}

	private static IEnumerable<ITooltipBrick> CreateMeleeCritHitModifiers(GameLogEventAttack.WeaponSkillModifierValues initiator, GameLogEventAttack.WeaponSkillModifierValues target, RuleCalculateHitChances rule, int nestedLevel)
	{
		bool isFirstOccurrence = true;
		foreach (Modifier modifier in rule.RighteousFuryChanceRule.AllModifiersList)
		{
			if (modifier.Stat == StatType.WarhammerWeaponSkill)
			{
				string name = "<b>" + LogThreadBase.Strings.TooltipBrickStrings.AttackerWeaponSkill.Text + "</b>";
				int resultValue = initiator.ResultValue;
				yield return new TooltipBrickIconTextValue(name, "<b>" + resultValue + "%</b>", nestedLevel, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
				bool needPlus = false;
				string item;
				foreach (Tuple<string, int> modifier2 in initiator.GetModifiers())
				{
					modifier2.Deconstruct(out item, out resultValue);
					string text = item;
					int num = resultValue;
					string value = ((num <= 0) ? "-" : (needPlus ? "+" : "")) + Mathf.Abs(num).ToString(CultureInfo.InvariantCulture) + "%";
					needPlus = true;
					yield return new TooltipBrickTextValue(text, value, nestedLevel);
				}
				if (rule.ResultWeaponSkillPenalty != 0)
				{
					yield return new TooltipBrickTextValue(LogThreadBase.Strings.TooltipBrickStrings.WeaponSkillPenalty.Text, UIUtility.AddSign(-rule.ResultWeaponSkillPenalty).ToString(CultureInfo.InvariantCulture) + "%", nestedLevel, isResultValue: true);
				}
				yield return new TooltipBrickIconTextValue("<b>" + LogThreadBase.Strings.TooltipBrickStrings.DefenderWeaponSkill.Text + "</b>", "<b>" + UIUtility.AddSign(-target.ResultValue) + "%</b>", nestedLevel, isResultValue: false, null, isProtectionIcon: true, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
				needPlus = false;
				foreach (Tuple<string, int> modifier3 in target.GetModifiers())
				{
					modifier3.Deconstruct(out item, out resultValue);
					string text2 = item;
					int num2 = resultValue;
					string value2 = ((num2 <= 0) ? "-" : (needPlus ? "+" : "")) + Mathf.Abs(num2).ToString(CultureInfo.InvariantCulture) + "%";
					needPlus = true;
					yield return new TooltipBrickTextValue(text2, value2, nestedLevel);
				}
				if (rule.ResultTargetSuperiorityPenalty != 0)
				{
					yield return new TooltipBrickIconTextValue("<b>" + LogThreadBase.Strings.TooltipBrickStrings.Superiority.Text + "</b>", "<b>" + UIUtility.AddSign(rule.ResultTargetSuperiorityPenalty).ToString(CultureInfo.InvariantCulture) + "%</b>", nestedLevel, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
				}
			}
			else
			{
				if (isFirstOccurrence)
				{
					int num3 = initiator.ResultValue - target.ResultValue + rule.ResultTargetSuperiorityPenalty;
					int num4 = rule.RighteousFuryChanceRule.RawResult - num3;
					yield return new TooltipBrickIconTextValue("<b>" + LogThreadBase.Strings.TooltipBrickStrings.FeaturesAndEquipmentsModifiers.Text + "</b>", "<b>" + UIUtility.AddSign(num4) + "%</b>", nestedLevel, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: true);
				}
				yield return LogThreadBase.CreateBrickModifier(modifier, valueIsPercent: true, null, nestedLevel, isResultValue: false, isFirstOccurrence);
				isFirstOccurrence = false;
			}
		}
		yield return LogThreadBase.AddMinMaxValue(rule.RighteousFuryChanceRule.RawResult, nestedLevel);
	}

	public static GameLogMessage GetMessage(GameLogEventAttack evt, RuleDealDamage overrideDealDamage = null)
	{
		RulePerformAttack rule = evt.Rule;
		RuleDealDamage ruleDealDamage = overrideDealDamage ?? evt.Rule.ResultDamageRule;
		bool isScatter = rule.Ability.IsScatter;
		bool resultIsRighteousFury = rule.RollPerformAttackRule.ResultIsRighteousFury;
		bool flag = rule.RollPerformAttackRule.HitChanceRule.ResultTargetSuperiorityPenalty > 0;
		bool flag2 = ruleDealDamage?.RollDamageRule.NullifyInformation.HasDamageNullify ?? false;
		if (isScatter)
		{
			if (rule.ResultIsHit)
			{
				if (ruleDealDamage == null)
				{
					return LogThreadBase.Strings.WarhammerHitNoDamage;
				}
				if (flag2)
				{
					return LogThreadBase.Strings.WarhammerDamageNegated;
				}
				if (resultIsRighteousFury)
				{
					return LogThreadBase.Strings.WarhammerRFHit;
				}
				if (!LogThreadBase.IsPreviousMessageUseSomething)
				{
					return LogThreadBase.Strings.WarhammerSourceDealDamage;
				}
				return LogThreadBase.Strings.WarhammerDealDamage;
			}
			RulePerformDodge resultDodgeRule = rule.ResultDodgeRule;
			if (resultDodgeRule != null && resultDodgeRule.Result)
			{
				return LogThreadBase.Strings.WarhammerDodge;
			}
			return LogThreadBase.Strings.WarhammerMiss;
		}
		if (rule.ResultIsHit)
		{
			if (ruleDealDamage == null)
			{
				return LogThreadBase.Strings.WarhammerHitNoDamage;
			}
			if (flag2)
			{
				return LogThreadBase.Strings.WarhammerDamageNegated;
			}
			if (resultIsRighteousFury)
			{
				if (!flag)
				{
					return LogThreadBase.Strings.WarhammerRFHit;
				}
				return LogThreadBase.Strings.WarhammerMeleeRFHitSuperiority;
			}
			if (flag)
			{
				return LogThreadBase.Strings.WarhammerMeleeHitSuperiority;
			}
			if (!LogThreadBase.IsPreviousMessageUseSomething)
			{
				return LogThreadBase.Strings.WarhammerSourceDealDamage;
			}
			return LogThreadBase.Strings.WarhammerDealDamage;
		}
		RulePerformDodge resultDodgeRule2 = rule.ResultDodgeRule;
		if (resultDodgeRule2 != null && resultDodgeRule2.Result)
		{
			RuleRollParry resultParryRule = rule.ResultParryRule;
			if (resultParryRule == null || !resultParryRule.Result)
			{
				return LogThreadBase.Strings.WarhammerDodge;
			}
			return LogThreadBase.Strings.WarhammerDodgeAndParry;
		}
		RuleRollParry resultParryRule2 = rule.ResultParryRule;
		if (resultParryRule2 != null && resultParryRule2.Result)
		{
			if (!flag)
			{
				return LogThreadBase.Strings.WarhammerParry;
			}
			return LogThreadBase.Strings.WarhammerParrySuperiority;
		}
		return LogThreadBase.Strings.WarhammerMiss;
	}

	private static IEnumerable<ITooltipBrick> CreateDogeBrickModifiers(IEnumerable<Modifier> allModifiers, CompositeModifiersManager weaponModifiers, bool valueIsPercent = false, string additionText = null, int nestedLevel = 0, bool isResultValue = false, bool isFirstWithoutPlus = false)
	{
		List<Modifier> list = allModifiers.ToList();
		int num = list.FindLastIndex((Modifier o) => o.Descriptor == ModifierDescriptor.ArmorPenalty);
		int num2 = list.FindLastIndex((Modifier o) => o.Descriptor == ModifierDescriptor.Weapon);
		if (num != -1 && num2 != -1)
		{
			Modifier item = list[num2];
			Modifier item2 = list[num];
			list.RemoveAt(num);
			list.Insert(list.IndexOf(item), item2);
		}
		bool printWeaponModifiers = false;
		foreach (Modifier modifier in list)
		{
			if (modifier.Descriptor == ModifierDescriptor.Difficulty && SettingsHelper.CalculateCRModifier() < 1f)
			{
				additionText = additionText + " (" + UIStrings.Instance.Tooltips.DifficultyReduceDescription.Text + ")";
			}
			ITooltipBrick tooltipBrick = LogThreadBase.CreateBrickModifier(modifier, valueIsPercent, additionText, nestedLevel, isResultValue, isFirstWithoutPlus);
			if (tooltipBrick == null)
			{
				continue;
			}
			isFirstWithoutPlus = false;
			yield return tooltipBrick;
			if (modifier.Descriptor != ModifierDescriptor.Weapon || printWeaponModifiers)
			{
				continue;
			}
			printWeaponModifiers = true;
			if (weaponModifiers == null)
			{
				continue;
			}
			isFirstWithoutPlus = true;
			foreach (Modifier valueModifiers in weaponModifiers.ValueModifiersList)
			{
				ITooltipBrick tooltipBrick2 = LogThreadBase.CreateBrickModifier(valueModifiers, valueIsPercent, additionText, nestedLevel + 1, isResultValue, isFirstWithoutPlus);
				if (tooltipBrick2 != null)
				{
					isFirstWithoutPlus = false;
					yield return tooltipBrick2;
				}
			}
		}
	}
}

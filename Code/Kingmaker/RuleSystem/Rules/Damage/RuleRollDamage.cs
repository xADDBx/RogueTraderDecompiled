using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleRollDamage : RulebookTargetEvent, IDamageHolderRule
{
	public DamageData Damage { get; private set; }

	public DamageValue Result { get; private set; }

	public int ResultValue { get; private set; }

	public int ResultValueWithoutReduction { get; private set; }

	public int ResultValueBeforeDifficulty { get; private set; }

	[CanBeNull]
	public DamageData ResultOverpenetration { get; private set; }

	public NullifyInformation NullifyInformation { get; private set; }

	public int UIMinimumDamageValue { get; private set; }

	public int UIMinimumDamagePercent { get; private set; }

	public bool IgnoreDeflection { get; set; }

	public bool IgnoreArmourAbsorption { get; set; }

	public RuleRollDamage([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] DamageData damage)
		: this((MechanicEntity)initiator, (MechanicEntity)target, damage)
	{
	}

	public RuleRollDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] DamageData damage)
		: base(initiator, target)
	{
		Damage = damage;
		IgnoreDeflection = false;
		IgnoreArmourAbsorption = false;
		NullifyInformation = NullifyInformation.Create();
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!Damage.IsCalculated)
		{
			RuleCalculateDamage ruleCalculateDamage = new CalculateDamageParams((MechanicEntity)base.Initiator, (MechanicEntity)Target, base.Reason.Ability, (base.Reason.Rule as RulePerformAttack)?.RollPerformAttackRule, Damage).Trigger();
			Damage = ruleCalculateDamage.ResultDamage;
		}
		Result = RollDamage(Damage, IgnoreDeflection, IgnoreArmourAbsorption);
		ResultOverpenetration = CalculateOverpenetration(Result.RolledValue);
		int num = ((Result.Source.Type == DamageType.Direct) ? Result.FinalValue : 0);
		ResultValue = Result.FinalValue;
		ResultValueWithoutReduction = Result.ValueWithoutReduction;
		ResultValueBeforeDifficulty = ResultValue;
		int num2 = 0;
		UIMinimumDamageValue = ResultValue;
		UIMinimumDamagePercent = 0;
		if (Result.Source.Type != DamageType.Direct)
		{
			(num2, UIMinimumDamageValue, UIMinimumDamagePercent) = ApplyDifficultyModifiers(ResultValue - num, Result.ValueWithoutReduction);
		}
		ResultValue = num2 + num;
		TryNullifyDamage();
	}

	public static DamageValue RollDamage(DamageData damage, bool ignoreDeflection = false, bool ignoreArmourAbsorption = false)
	{
		int num = (damage.CalculatedValue.HasValue ? damage.Modifiers.ApplyPctMulExtra(damage.CalculatedValue.Value) : RollWithoutArmorReduction(damage));
		int val = ((damage.Overpenetrating && !damage.UnreducedOverpenetration) ? Mathf.RoundToInt((float)num * damage.EffectiveOverpenetrationFactor) : num);
		int num2 = ((!damage.Immune) ? Math.Max(0, val) : 0);
		int num3 = ((!ignoreDeflection) ? damage.Deflection.Value : 0);
		float num4 = (ignoreArmourAbsorption ? 1f : damage.AbsorptionFactorWithPenetration);
		int val2 = (int)((float)(num2 - num3) * num4);
		int num5 = Math.Max(0, val2);
		int reduction = Math.Max(0, num2 - num5);
		return new DamageValue(damage, num5, num, reduction);
	}

	private static int RollWithoutArmorReduction(DamageData damage)
	{
		damage.SetRoll(Dice.D100);
		return damage.InitialRolledValue + damage.CriticalRolledValue;
	}

	private (int damage, int minDamageValue, int minDamagePercent) ApplyDifficultyModifiers(int damage, int damageBeforeReductions)
	{
		int num = ((damage > 0) ? Math.Max(1, damage) : 0);
		int num2 = num;
		int num3 = 0;
		if (base.Initiator.IsPlayerFaction && !Target.IsPlayerFaction)
		{
			num2 = ((!(base.Initiator is StarshipEntity)) ? ((int)SettingsRoot.Difficulty.MinPartyDamage) : ((Target as StarshipEntity).IsSoftUnit ? 1 : ((int)SettingsRoot.Difficulty.MinPartyStarshipDamage)));
			num3 = ((!(base.Initiator is StarshipEntity)) ? ((int)SettingsRoot.Difficulty.MinPartyDamageFraction) : ((Target as StarshipEntity).IsSoftUnit ? 1 : ((int)SettingsRoot.Difficulty.MinPartyStarshipDamageFraction)));
			num = Math.Max(num2, num);
			num = Math.Max(Mathf.CeilToInt((float)(damageBeforeReductions * num3) / 100f), num);
		}
		return (damage: num, minDamageValue: num2, minDamagePercent: num3);
	}

	[CanBeNull]
	private DamageData CalculateOverpenetration(int damageRoll)
	{
		if (damageRoll == 0 && Target is DestructibleEntity)
		{
			return null;
		}
		if (Target is UnitEntity entity && entity.HasMechanicFeature(MechanicsFeatureType.BlockOverpenetration))
		{
			return null;
		}
		int overpenetrationFactorPercents = Damage.OverpenetrationFactorPercents;
		int num = Math.Max(0, overpenetrationFactorPercents - 30);
		if (num <= 0)
		{
			return null;
		}
		return new DamageData(Damage.Type, damageRoll)
		{
			OverpenetrationFactorPercents = num,
			Overpenetrating = true,
			CalculatedValue = damageRoll
		};
	}

	public void NullifyDamage(EntityFact source)
	{
		if (base.IsTriggered)
		{
			Result = new DamageValue(Result.Source, 0, Result.RolledValue, 0);
		}
		Damage.Modifiers.Add(ModifierType.PctMul_Extra, 0, source);
	}

	private void TryNullifyDamage()
	{
		if (NullifyInformation.HasDamageChance)
		{
			NullifyInformation.DamageNegationRoll = Dice.D100;
			if (NullifyInformation.DamageNegationRoll <= NullifyInformation.DamageChance)
			{
				NullifyInformation.HasDamageNullify = true;
				ResultValue = 0;
			}
		}
	}
}

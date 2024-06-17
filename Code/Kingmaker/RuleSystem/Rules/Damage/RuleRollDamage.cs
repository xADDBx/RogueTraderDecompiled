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

	public int MinimumDamageValue { get; private set; }

	public bool ArmorIgnore { get; set; }

	public RuleRollDamage([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] DamageData damage)
		: this((MechanicEntity)initiator, (MechanicEntity)target, damage)
	{
	}

	public RuleRollDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] DamageData damage)
		: base(initiator, target)
	{
		Damage = damage;
		ArmorIgnore = false;
		NullifyInformation = NullifyInformation.Create();
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!Damage.IsCalculated)
		{
			RuleCalculateDamage evt = new RuleCalculateDamage((MechanicEntity)base.Initiator, (MechanicEntity)Target, base.Reason.Ability, (base.Reason.Rule as RulePerformAttack)?.RollPerformAttackRule, Damage);
			Damage = Rulebook.Trigger(evt).ResultDamage;
		}
		Result = RollDamage(Damage, ArmorIgnore);
		ResultOverpenetration = CalculateOverpenetration(Result.RolledValue);
		int num = ((Result.Source.Type == DamageType.Direct) ? Result.FinalValue : 0);
		ResultValue = Result.FinalValue;
		ResultValueWithoutReduction = Result.ValueWithoutReduction;
		ResultValueBeforeDifficulty = ResultValue;
		int num2 = 0;
		MinimumDamageValue = ResultValue;
		if (Result.Source.Type != DamageType.Direct)
		{
			(num2, MinimumDamageValue) = ApplyDifficultyModifiers(ResultValue - num, Result.ValueWithoutReduction);
		}
		ResultValue = num2 + num;
		TryNullifyDamage();
	}

	public static DamageValue RollDamage(DamageData damage, bool armorIgnore = false)
	{
		int num = (damage.CalculatedValue.HasValue ? damage.Modifiers.ApplyPctMulExtra(damage.CalculatedValue.Value) : RollWithoutArmorReduction(damage));
		int val = ((damage.Overpenetrating && !damage.UnreducedOverpenetration) ? Mathf.RoundToInt((float)num * damage.EffectiveOverpenetrationFactor) : num);
		int num2 = ((!damage.Immune) ? Math.Max(0, val) : 0);
		int val2 = (armorIgnore ? num2 : ((int)((float)(num2 - damage.Deflection.Value) * damage.AbsorptionFactorWithPenetration)));
		int num3 = Math.Max(0, val2);
		int reduction = Math.Max(0, num2 - num3);
		return new DamageValue(damage, num3, num, reduction);
	}

	private static int RollWithoutArmorReduction(DamageData damage)
	{
		damage.SetRoll(Dice.D100);
		return damage.InitialRolledValue + damage.CriticalRolledValue;
	}

	private (int damage, int minDamage) ApplyDifficultyModifiers(int damage, int damageBeforeReductions)
	{
		int num = ((damage > 0) ? Math.Max(1, damage) : 0);
		int num2 = num;
		if (base.Initiator.IsPlayerFaction)
		{
			num2 = ((!(base.Initiator is StarshipEntity)) ? ((int)SettingsRoot.Difficulty.MinPartyDamage) : ((Target as StarshipEntity).IsSoftUnit ? 1 : ((int)SettingsRoot.Difficulty.MinPartyStarshipDamage)));
			int num3 = ((!(base.Initiator is StarshipEntity)) ? ((int)SettingsRoot.Difficulty.MinPartyDamageFraction) : ((Target as StarshipEntity).IsSoftUnit ? 1 : ((int)SettingsRoot.Difficulty.MinPartyStarshipDamageFraction)));
			num = Math.Max(num2, num);
			num = Math.Max(Mathf.CeilToInt((float)(damageBeforeReductions * num3) / 100f), num);
		}
		return (damage: num, minDamage: num2);
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

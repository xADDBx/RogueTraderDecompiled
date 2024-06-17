using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Starships;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.RuleSystem.Rules.Starships;

public class RuleStarshipPerformAttack : RulebookTargetEvent<StarshipEntity, StarshipEntity>
{
	private float m_DamageRoll;

	private int m_DamageMin;

	private int m_DamageMax;

	private int m_BaseDamageMin;

	private int m_BaseDamageMax;

	private float m_DamageDifficultyModifier;

	public AbilityData Ability { get; }

	public ItemEntityStarshipWeapon Weapon { get; }

	public RuleStarshipRollAttack AttackRollRule { get; }

	public RuleStarshipCalculateHitLocation HitLocationRule { get; private set; }

	public RuleStarshipRollShieldAbsorption ShieldAbsorptionRollRule { get; private set; }

	public RuleStarshipCalculateDamageForTarget CalculateDamageRule { get; private set; }

	public RuleDealDamage DamageRule { get; private set; }

	public int BonusDamage { get; set; }

	public float ExtraDamageMod { get; set; }

	public AttackResult Result { get; private set; }

	public int ResultDamage { get; private set; }

	public int ResultAbsorbedDamage { get; private set; }

	public int ResultShieldStrengthLoss { get; private set; }

	public int ResultDeflectedDamage { get; private set; }

	public int ResultDamageBeforeAbsorption { get; private set; }

	public int ResultDamageBeforeDeflection { get; private set; }

	public RuleStarshipPerformAttack FirstAttackInBurst { get; set; }

	public RuleStarshipPerformAttack NextAttackInBurst { get; set; }

	public bool IsTorpedoDirectHitAttempt { get; set; }

	public bool IsPredictionOnly { get; set; }

	public StarshipHitLocation ResultHitLocation
	{
		get
		{
			if (!HitLocationRule.IsTriggered)
			{
				Rulebook.Trigger(HitLocationRule);
			}
			return HitLocationRule.ResultHitLocation;
		}
	}

	public bool ResultIsHit
	{
		get
		{
			if (Result != AttackResult.Hit)
			{
				return Result == AttackResult.RighteousFury;
			}
			return true;
		}
	}

	public bool ResultIsCritical => Result == AttackResult.RighteousFury;

	public RuleRollD100 ResultAttackRollD100 => AttackRollRule.HitD100;

	public int UIDamageResult
	{
		get
		{
			if (Weapon.IsFocusedEnergyWeapon)
			{
				return ResultDamageBeforeAbsorption;
			}
			int num = 0;
			if (ResultAbsorbedDamage > 0)
			{
				num += ResultShieldStrengthLoss;
			}
			if (ResultDamage > 0)
			{
				num += ResultDamage;
			}
			return num;
		}
	}

	public int UIDamageMin => UIDamageData?.MinValue ?? m_DamageMin;

	public int UIDamageMax => UIDamageData?.MaxValue ?? m_DamageMax;

	public int UIInitialDamage => UIDamageData?.InitialRolledValue ?? (UIInitialDamageMin + Mathf.RoundToInt((float)(UIInitialDamageMax - UIInitialDamageMin) * m_DamageRoll));

	public int UIInitialDamageMin => UIDamageData?.MinInitialValue ?? (UIBaseDamageMin + BonusDamage);

	public int UIInitialDamageMax => UIDamageData?.MaxInitialValue ?? (UIBaseDamageMax + BonusDamage);

	public float UIDamageDifficultyModifier => m_DamageDifficultyModifier;

	public int UIBaseDamageMin => UIDamageData?.MinValueBase ?? m_BaseDamageMin;

	public int UIBaseDamageMax => UIDamageData?.MaxValueBase ?? Mathf.Max(m_BaseDamageMin, m_BaseDamageMax);

	public DamageData UIDamageData
	{
		get
		{
			object obj = DamageRule?.Damage;
			if (obj == null)
			{
				RuleStarshipCalculateDamageForTarget calculateDamageRule = CalculateDamageRule;
				if (calculateDamageRule == null)
				{
					return null;
				}
				obj = calculateDamageRule.ResultDamage;
			}
			return (DamageData)obj;
		}
	}

	public bool UIIsFocusedEnergyWeapon { get; private set; }

	public RuleStarshipPerformAttack([NotNull] StarshipEntity initiator, [NotNull] StarshipEntity target, [NotNull] AbilityData ability, [NotNull] ItemEntityStarshipWeapon weapon, [CanBeNull] RuleStarshipCalculateHitLocation hitLocationRule = null)
		: base(initiator, target)
	{
		Ability = ability;
		Weapon = weapon;
		AttackRollRule = new RuleStarshipRollAttack(base.Initiator, base.Target, Weapon);
		HitLocationRule = hitLocationRule ?? new RuleStarshipCalculateHitLocation(base.Initiator, base.Target);
	}

	public void EvaluatePrediction()
	{
		IsPredictionOnly = true;
		OnTriggerInternal(null);
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		OnTriggerInternal(context);
		EventBus.RaiseEvent(delegate(IStarshipAttackHandler h)
		{
			h.HandleAttack(this);
		});
	}

	public void OnTriggerInternal(RulebookEventContext context)
	{
		if (!AttackRollRule.IsTriggered)
		{
			AttackRollRule.Trigger(IsPredictionOnly, IsTorpedoDirectHitAttempt);
		}
		Result = (AttackRollRule.ResultIsCrit ? AttackResult.RighteousFury : (AttackRollRule.ResultIsHit ? AttackResult.Hit : AttackResult.Miss));
		m_BaseDamageMin = Weapon.Ammo.Blueprint.Damage;
		m_BaseDamageMax = Weapon.Ammo.Blueprint.MaxDamage;
		m_DamageDifficultyModifier = SpacecombatDifficultyHelper.StarshipDamageMod(base.Initiator);
		m_DamageMin = CalcDmg(m_BaseDamageMin);
		m_DamageMax = Math.Max(m_DamageMin, CalcDmg(m_BaseDamageMax));
		if (!IsPredictionOnly)
		{
			ResultDamageBeforeAbsorption = Roll(m_DamageMin, m_DamageMax);
		}
		else
		{
			ResultDamageBeforeAbsorption = (m_DamageMin + m_DamageMax) / 2;
		}
		if (Weapon.IsFocusedEnergyWeapon)
		{
			if (NextAttackInBurst != null)
			{
				if (ResultIsCritical)
				{
					ResultDamageBeforeAbsorption += ResultDamageBeforeAbsorption;
				}
				UIIsFocusedEnergyWeapon = true;
				return;
			}
			AttackResult result = AttackResult.Miss;
			ResultDamageBeforeAbsorption = 0;
			RuleStarshipPerformAttack ruleStarshipPerformAttack = FirstAttackInBurst;
			while (ruleStarshipPerformAttack.NextAttackInBurst != null)
			{
				if (ruleStarshipPerformAttack.ResultIsHit)
				{
					ResultDamageBeforeAbsorption += ruleStarshipPerformAttack.ResultDamageBeforeAbsorption;
					result = AttackResult.Hit;
				}
				ruleStarshipPerformAttack = ruleStarshipPerformAttack.NextAttackInBurst;
			}
			Result = result;
		}
		if (Result == AttackResult.Miss)
		{
			return;
		}
		ShieldAbsorptionRollRule = new RuleStarshipRollShieldAbsorption(base.Initiator, base.Target, ResultDamageBeforeAbsorption, Weapon.DamageType, ResultHitLocation)
		{
			IsFirstAttack = (Weapon.IsFocusedEnergyWeapon || FirstAttackInBurst == this)
		};
		ShieldAbsorptionRollRule.Trigger(IsPredictionOnly);
		if (ShieldAbsorptionRollRule.ResultAbsorbedDamage > 0)
		{
			(int, int) tuple = base.Target.Shields.Absorb(ShieldAbsorptionRollRule.ResultHitLocation, ShieldAbsorptionRollRule.ResultAbsorbedDamage, Weapon.DamageType, IsPredictionOnly);
			(ResultAbsorbedDamage, ResultShieldStrengthLoss) = tuple;
		}
		ResultDamageBeforeDeflection = ResultDamageBeforeAbsorption - ResultAbsorbedDamage;
		if (ResultDamageBeforeDeflection == 0)
		{
			Result = AttackResult.Hit;
			return;
		}
		if (Result == AttackResult.RighteousFury)
		{
			ResultDamageBeforeDeflection = ResultDamageBeforeAbsorption * 150 / 100 - ResultAbsorbedDamage;
		}
		CalculateDamageRule = Rulebook.Trigger(new RuleStarshipCalculateDamageForTarget(base.Initiator, base.Target, ResultDamageBeforeDeflection, Weapon.DamageType, Weapon.IsAEAmmo, ResultHitLocation));
		if (IsPredictionOnly)
		{
			return;
		}
		DamageRule = Rulebook.Trigger(new RuleDealDamage(base.Initiator, base.Target, CalculateDamageRule.ResultDamage)
		{
			DisableGameLog = true
		});
		ResultDamage = (base.Target.IsSoftUnit ? (ResultDamageBeforeDeflection - CalculateDamageRule.ResultDeflection) : DamageRule.Result);
		ResultDeflectedDamage = CalculateDamageRule.ResultDeflection;
		BlueprintBuffReference[] hitEffects = Weapon.Ammo.Blueprint.HitEffects;
		foreach (BlueprintBuffReference blueprintBuffReference in hitEffects)
		{
			base.Target.Buffs.Add(blueprintBuffReference, base.Initiator);
		}
		if (Result == AttackResult.RighteousFury && Weapon.Ammo.Blueprint.CriticalHitEffects != null)
		{
			hitEffects = Weapon.Ammo.Blueprint.CriticalHitEffects;
			foreach (BlueprintBuffReference blueprintBuffReference2 in hitEffects)
			{
				base.Target.Buffs.Add(blueprintBuffReference2, base.Initiator);
			}
		}
		int CalcDmg(int srcDmg)
		{
			return Mathf.RoundToInt((float)(srcDmg + BonusDamage) * (1f + ExtraDamageMod) * m_DamageDifficultyModifier);
		}
	}

	private int Roll(int minDamage, int maxDamage)
	{
		m_DamageRoll = (float)(int)Dice.D100 / 100f;
		int num = maxDamage - minDamage;
		return minDamage + Mathf.RoundToInt((float)num * m_DamageRoll);
	}
}

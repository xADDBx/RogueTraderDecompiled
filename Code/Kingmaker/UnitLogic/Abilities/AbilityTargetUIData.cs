using System;
using System.Collections.Generic;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View.Covers;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Abilities;

public struct AbilityTargetUIData : IEquatable<AbilityTargetUIData>
{
	public AbilityData Ability { get; }

	public MechanicEntity Target { get; }

	public Vector3 CasterPosition { get; }

	public float HitWithAvoidanceChance { get; private set; }

	public float InitialHitChance { get; private set; }

	public int MinDamage { get; private set; }

	public int MaxDamage { get; private set; }

	public int Lines { get; }

	public int BurstIndex { get; private set; }

	public float DodgeChance { get; private set; }

	public float ParryChance { get; private set; }

	public float CoverChance { get; private set; }

	public float EvasionChance { get; private set; }

	public bool CanPush { get; private set; }

	public List<float> BurstHitChances { get; }

	public bool HitAlways { get; }

	public AbilityTargetUIData(AbilityData ability, MechanicEntity target, Vector3 casterPosition, bool hitAlways, float initialHitChance, float hitWithAvoidanceChance, int minDamage, int maxDamage, int lines, int burstIndex, List<float> burstHitChances, float dodgeChance, float coverChance, float evasionChance)
	{
		Ability = ability.Clone(isPreview: true);
		Target = target;
		CasterPosition = casterPosition;
		HitAlways = hitAlways;
		HitWithAvoidanceChance = hitWithAvoidanceChance;
		InitialHitChance = initialHitChance;
		MinDamage = minDamage;
		MaxDamage = maxDamage;
		Lines = lines;
		BurstIndex = burstIndex;
		BurstHitChances = burstHitChances;
		DodgeChance = dodgeChance;
		CoverChance = coverChance;
		EvasionChance = evasionChance;
		ParryChance = 0f;
		CanPush = false;
	}

	public AbilityTargetUIData(AbilityData ability, MechanicEntity target, Vector3 casterPosition)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			Ability = ability.Clone(isPreview: true);
			Target = target;
			CasterPosition = casterPosition;
			Lines = 1;
			BurstIndex = Ability.BurstAttacksCount;
			BurstHitChances = ((Ability.BurstAttacksCount > 1) ? new List<float>() : null);
			HitWithAvoidanceChance = 0f;
			InitialHitChance = 0f;
			MinDamage = 0;
			MaxDamage = 0;
			DodgeChance = 0f;
			CoverChance = 0f;
			ParryChance = 0f;
			EvasionChance = 0f;
			CanPush = false;
			HitAlways = Ability.IsAOE || Ability.IsCharge;
			SetAbilityWeapon(Ability);
			ItemEntityWeapon weapon = Ability.Weapon;
			ItemEntityStarshipWeapon starshipWeapon = Ability.StarshipWeapon;
			if (target != null && starshipWeapon != null && target is StarshipEntity target2)
			{
				UpdateWithStarshipWeapon(Ability, target2, casterPosition, starshipWeapon);
				return;
			}
			if (target != null && weapon != null)
			{
				UpdateWithWeapon(Ability, target, casterPosition, weapon);
			}
			else
			{
				bool num = Ability.IsValid(target, casterPosition);
				float num2 = (Ability.IsValid(target, casterPosition) ? 100f : (-1f));
				if (num)
				{
					HitAlways = true;
				}
				float hitWithAvoidanceChance = (InitialHitChance = num2);
				HitWithAvoidanceChance = hitWithAvoidanceChance;
			}
			DamagePredictionData damagePrediction = Ability.GetDamagePrediction(target, casterPosition);
			HealPredictionData healPrediction = Ability.GetHealPrediction(target);
			MinDamage = damagePrediction?.MinDamage ?? healPrediction?.MinValue ?? 0;
			MaxDamage = damagePrediction?.MaxDamage ?? healPrediction?.MaxValue ?? 0;
		}
	}

	public void UpdateDamage()
	{
		ItemEntityWeapon weapon = Ability.Weapon;
		ItemEntityStarshipWeapon starshipWeapon = Ability.StarshipWeapon;
		if (Target != null && starshipWeapon != null && Target is StarshipEntity target)
		{
			UpdateWithStarshipWeapon(Ability, target, CasterPosition, starshipWeapon);
			return;
		}
		if (Target != null && weapon != null)
		{
			UpdateWithWeapon(Ability, Target, CasterPosition, weapon);
		}
		DamagePredictionData damagePrediction = Ability.GetDamagePrediction(Target, CasterPosition);
		HealPredictionData healPrediction = Ability.GetHealPrediction(Target);
		MinDamage = damagePrediction?.MinDamage ?? healPrediction?.MinValue ?? 0;
		MaxDamage = damagePrediction?.MaxDamage ?? healPrediction?.MaxValue ?? 0;
	}

	private void UpdateWithWeapon(AbilityData ability, MechanicEntity target, Vector3 casterPosition, ItemEntityWeapon weapon)
	{
		MechanicEntity caster = ability.Caster;
		float[] array = new float[BurstIndex];
		float[] array2 = new float[BurstIndex];
		float num = 0f;
		float num2 = 0f;
		Vector3 bestShootingPosition = LosCalculations.GetBestShootingPosition(casterPosition + LosCalculations.EyeShift, caster.SizeRect, target.Position, target.SizeRect);
		LosCalculations.CoverType warhammerLos = LosCalculations.GetWarhammerLos(caster, casterPosition, target);
		LosDescription warhammerLos2 = LosCalculations.GetWarhammerLos(bestShootingPosition, caster.SizeRect, target.Position, target.SizeRect);
		LosDescription warhammerLos3 = LosCalculations.GetWarhammerLos(bestShootingPosition, caster.SizeRect, target);
		CoverChance = Rulebook.Trigger(new RuleCalculateCoverHitChance(ability.Caster, target, ability, warhammerLos3, null)).ResultChance;
		for (int i = 0; i < BurstIndex; i++)
		{
			RuleCalculateHitChances ruleCalculateHitChances = GameHelper.TriggerRule(new RuleCalculateHitChances(caster, target, ability, i, bestShootingPosition, target.Position));
			array[i] = Math.Max(ruleCalculateHitChances.ResultHitChance, 0);
			float num3 = Math.Max(ruleCalculateHitChances.ResultHitChance, 0);
			DodgeChance = 0f;
			ParryChance = 0f;
			if (target is UnitEntity defender)
			{
				RuleCalculateDodgeChance ruleCalculateDodgeChance = Rulebook.Trigger(new RuleCalculateDodgeChance(defender, caster, ability, warhammerLos2, i));
				DodgeChance = Mathf.Min(Math.Max(ruleCalculateDodgeChance.Result, 0f), 100f);
				RuleCalculateParryChance ruleCalculateParryChance = Rulebook.Trigger(new RuleCalculateParryChance(defender, caster, ability, ruleCalculateHitChances.ResultSuperiorityNumber));
				ParryChance = (weapon.Blueprint.IsMelee ? Mathf.Min(Math.Max(ruleCalculateParryChance.Result, 0f), 100f) : 0f);
			}
			array2[i] = num3 * (1f - DodgeChance / 100f) * (1f - ParryChance / 100f) * (1f - CoverChance / 100f);
			num += array[i];
			num2 += array2[i];
			BurstHitChances?.Add(array2[i]);
		}
		float initialHitChance = num / (float)BurstIndex;
		float hitWithAvoidanceChance = num2 / (float)BurstIndex;
		if ((LosCalculations.CoverType)warhammerLos2 != LosCalculations.CoverType.Invisible || warhammerLos != LosCalculations.CoverType.Invisible)
		{
			HitWithAvoidanceChance = hitWithAvoidanceChance;
			InitialHitChance = initialHitChance;
		}
		else
		{
			float hitWithAvoidanceChance2 = (InitialHitChance = -1f);
			HitWithAvoidanceChance = hitWithAvoidanceChance2;
		}
	}

	private void UpdateWithStarshipWeapon(AbilityData ability, StarshipEntity target, Vector3 casterPosition, ItemEntityStarshipWeapon weapon)
	{
		if (ability.Caster is StarshipEntity initiator)
		{
			RuleStarshipCalculateHitChances ruleStarshipCalculateHitChances = Rulebook.Trigger(new RuleStarshipCalculateHitChances(initiator, target, weapon));
			float hitWithAvoidanceChance = (InitialHitChance = ruleStarshipCalculateHitChances.ResultHitChance);
			HitWithAvoidanceChance = hitWithAvoidanceChance;
			EvasionChance = ruleStarshipCalculateHitChances.ResultEvasionChance;
			DamagePredictionData damagePrediction = ability.GetDamagePrediction(target, casterPosition);
			MinDamage = damagePrediction.MinDamage;
			MaxDamage = damagePrediction.MaxDamage;
			BurstIndex = weapon.Blueprint.DamageInstances;
		}
	}

	private void SetAbilityWeapon(AbilityData ability)
	{
		if (ability.Weapon != null || !ability.IsCharge)
		{
			return;
		}
		GameAction[] actions = (ability.Blueprint.PatternSettings as AbilityCustomDirectMovement).ActionsOnEncounteredTarget.Actions;
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i] is WarhammerContextActionPerformAttack { UseCurrentWeapon: not false })
			{
				ability.OverrideWeapon = ability.Caster.GetFirstWeapon();
				break;
			}
		}
	}

	public bool Equals(AbilityTargetUIData other)
	{
		if (object.Equals(Ability, other.Ability) && object.Equals(Target, other.Target) && CasterPosition.Equals(other.CasterPosition) && object.Equals(HitWithAvoidanceChance, other.HitWithAvoidanceChance) && object.Equals(InitialHitChance, other.InitialHitChance) && MaxDamage == other.MaxDamage)
		{
			return MinDamage == other.MinDamage;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is AbilityTargetUIData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Ability, Target, CasterPosition);
	}

	public static bool operator ==(AbilityTargetUIData left, AbilityTargetUIData right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(AbilityTargetUIData left, AbilityTargetUIData right)
	{
		return !left.Equals(right);
	}
}

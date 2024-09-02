using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.HitSystem;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.CutsceneAttack;

[TypeId("f867a3ae47474b39b6b540154fc40cbd")]
public class AbilityCutsceneAttack : AbilityCustomLogic
{
	public bool IsBurst;

	public bool UseCustomDamageForFX;

	[ShowIf("UseCustomDamageForFX")]
	public DamageDescription DamageForFX;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		int count = ((!IsBurst) ? 1 : Math.Max(context.Ability.RateOfFire, 1));
		IEnumerator<AbilityDeliveryTarget>[] attacks = (from i in Enumerable.Range(0, count)
			select DeliverAttack(context, target, i)).ToArray();
		while (attacks.HasItem((IEnumerator<AbilityDeliveryTarget> i) => i != null))
		{
			int i = 0;
			while (i < Math.Min(context.ActionIndex, attacks.Length))
			{
				IEnumerator<AbilityDeliveryTarget> line = attacks[i];
				if (line != null)
				{
					bool flag;
					while ((flag = line.MoveNext()) && line.Current != null)
					{
						yield return line.Current;
					}
					if (!flag)
					{
						attacks[i] = null;
					}
				}
				int num = i + 1;
				i = num;
			}
			yield return null;
		}
		if (context.KillTarget)
		{
			KillTarget(context, target);
			yield return null;
		}
	}

	private static void KillTarget(AbilityExecutionContext context, TargetWrapper target)
	{
		if (target.Entity is BaseUnitEntity baseUnitEntity)
		{
			if (context.DisableLog)
			{
				baseUnitEntity.GetOrCreate<Kill.SilentDeathUnitPart>();
			}
			baseUnitEntity.GiveExperienceOnDeath = false;
			GameHelper.KillUnit(baseUnitEntity, context.Caster as BaseUnitEntity, 1);
		}
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverAttack(AbilityExecutionContext context, TargetWrapper target, int index)
	{
		ItemEntityWeapon weapon = context.Ability.Weapon;
		if (weapon == null || !weapon.Blueprint.IsMelee)
		{
			return DeliverRangedAttack(context, target, index);
		}
		return DeliverMeleeAttack(context, target, index);
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverMeleeAttack(AbilityExecutionContext context, TargetWrapper target, int index)
	{
		if (AttackHitPolicyContextData.Current switch
		{
			AttackHitPolicyType.Default => PFStatefulRandom.CutsceneAttack.YesOrNo, 
			AttackHitPolicyType.AutoHit => true, 
			AttackHitPolicyType.AutoMiss => false, 
			_ => throw new ArgumentOutOfRangeException(), 
		})
		{
			PlayDamageFX(context, target, null);
		}
		yield break;
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverRangedAttack(AbilityExecutionContext context, TargetWrapper target, int index)
	{
		bool isHit = AttackHitPolicyContextData.Current switch
		{
			AttackHitPolicyType.Default => PFStatefulRandom.CutsceneAttack.YesOrNo, 
			AttackHitPolicyType.AutoHit => true, 
			AttackHitPolicyType.AutoMiss => false, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		Vector3 position = context.Caster.Position;
		Vector3 vector = target.Point + Vector3.up;
		Vector3 normalized = (vector - position).normalized;
		float num = Math.Max(100f, context.Ability.RangeCells.CellsFromMeters().Meters);
		Vector3 vector2 = new Vector3(normalized.z, 0f, normalized.x);
		float num2 = 1.Cells().Meters / 10f;
		float num3 = (isHit ? ((float)PFStatefulRandom.CutsceneAttack.Range(-3, 4) * num2) : ((float)(PFStatefulRandom.CutsceneAttack.Range(4, 7) * PFStatefulRandom.CutsceneAttack.Sign) * num2));
		vector += vector2 * num3;
		position += Vector3.up * ((float)PFStatefulRandom.CutsceneAttack.Range(10, 17) * num2);
		normalized = (vector - position).normalized;
		BlueprintProjectile blueprint = context.Ability.ProjectileVariants.Random(PFStatefulRandom.CutsceneAttack);
		Vector3 vector3 = (isHit ? vector : (position + normalized.normalized * num));
		TargetWrapper projectileTarget = ((!isHit || target.Entity == null) ? ((TargetWrapper)vector3) : target);
		Projectile projectile = new ProjectileLauncher(blueprint, context.Caster, projectileTarget).Ability(context.Ability).MaxRangeCells(context.Ability.RangeCells).Index(index)
			.Launch();
		float distance = projectile.Distance(target.Point, position);
		while (!projectile.IsEnoughTimePassedToTraverseDistance(distance))
		{
			yield return null;
		}
		if (isHit)
		{
			PlayDamageFX(context, target, projectile);
			HitFXPlayer.PlayProjectileHit(projectile, projectileTarget);
		}
	}

	private void PlayDamageFX(AbilityExecutionContext context, TargetWrapper target, [CanBeNull] Projectile projectile)
	{
		DamageData baseDamageOverride = (UseCustomDamageForFX ? DamageForFX.CreateDamage() : null);
		DamageData resultDamage = new CalculateDamageParams(context.Caster, target.Entity, context.Ability, null, baseDamageOverride).Trigger().ResultDamage;
		resultDamage.CalculatedValue = resultDamage.AverageValueWithoutArmorReduction;
		DamageValue damage = RuleRollDamage.RollDamage(resultDamage);
		if (context.DamagePolicy == DamagePolicyType.Default && target.Entity != null)
		{
			Rulebook.Trigger(new RuleDealDamage(context.Caster, target.Entity, resultDamage));
		}
		HitFXPlayer.PlayDamageHit(context.Caster, target.Entity, context.MainTarget, projectile, context.Ability, damage, isCritical: false, isDot: false);
	}
}

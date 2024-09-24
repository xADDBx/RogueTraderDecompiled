using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("529f134a929e46e9ac7434d280348b1f")]
public class AbilityMeleeBurst : AbilityCustomLogic, IAbilityAoEPatternProviderHolder
{
	public bool UseVariableRateOfAttack;

	[ShowIf("UseVariableRateOfAttack")]
	public ContextValue RateOfAttack;

	[Space(2f)]
	public bool IsAoe;

	[SerializeField]
	[ShowIf("IsAoe")]
	private AbilityAoEPatternSettings m_PatternSettings;

	[Space(2f)]
	public bool UseSpecificWeapon;

	[ShowIf("UseCustomWeapon")]
	public bool UseOnSourceWeapon;

	[ShowIf("CustomSourceWeapon")]
	public bool UseSecondWeapon;

	private bool m_FirstAttackCalculated;

	private bool m_FirstAttackHit;

	[UsedImplicitly]
	private bool CustomSourceWeapon
	{
		get
		{
			if (UseOnSourceWeapon)
			{
				return UseSpecificWeapon;
			}
			return false;
		}
	}

	public IAbilityAoEPatternProvider PatternProvider
	{
		get
		{
			if (!IsAoe)
			{
				return null;
			}
			return m_PatternSettings;
		}
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		if (!(context.Caster is UnitEntity caster))
		{
			PFLog.Default.Error("Caster unit is missing");
			yield break;
		}
		if (caster.GetThreatHandMelee() == null)
		{
			PFLog.Default.Error("Invalid caster's weapon");
			yield break;
		}
		if (!IsAoe && !targetWrapper.HasEntity)
		{
			PFLog.Default.Error("Invalid target");
			yield break;
		}
		int burstCounter = 0;
		ItemEntityWeapon weapon = context.Ability.Weapon;
		ItemEntityWeapon maybeWeapon = caster.Body.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon maybeWeapon2 = caster.Body.SecondaryHand.MaybeWeapon;
		if (UseSpecificWeapon && !UseOnSourceWeapon)
		{
			weapon = (UseSecondWeapon ? (maybeWeapon2 ?? maybeWeapon) : (maybeWeapon ?? maybeWeapon2));
		}
		int maxCount = context.Ability.RateOfFire;
		m_FirstAttackHit = false;
		m_FirstAttackCalculated = false;
		while (burstCounter < maxCount && caster.CanAct)
		{
			if (burstCounter + 1 != context.ActionIndex)
			{
				yield return null;
				continue;
			}
			context.Ability.OverrideWeapon = weapon;
			if (IsAoe)
			{
				IEnumerator<AbilityDeliveryTarget> attackEnum = DeliverAOEDirectly(context);
				while (attackEnum.MoveNext())
				{
					yield return attackEnum.Current;
				}
			}
			else
			{
				yield return TriggerAttackRule(context, targetWrapper.Entity, burstCounter, weapon);
			}
			burstCounter++;
		}
		context.Ability.OverrideWeapon = null;
	}

	public ItemEntityWeapon GetWeaponToUse(AbilityExecutionContext context)
	{
		ItemEntityWeapon result = context.Ability.Weapon;
		if (!(context.Caster is UnitEntity unitEntity))
		{
			PFLog.Default.Error("Caster unit is missing");
			return null;
		}
		if (unitEntity.GetThreatHandMelee() == null)
		{
			PFLog.Default.Error("Invalid caster's weapon");
			return null;
		}
		ItemEntityWeapon maybeWeapon = unitEntity.Body.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon maybeWeapon2 = unitEntity.Body.SecondaryHand.MaybeWeapon;
		if (UseSpecificWeapon && !UseOnSourceWeapon)
		{
			result = (UseSecondWeapon ? (maybeWeapon2 ?? maybeWeapon) : (maybeWeapon ?? maybeWeapon2));
		}
		return result;
	}

	public int GetRateOfFire(AbilityExecutionContext context)
	{
		WarhammerOverrideRateOfFire component = context.AbilityBlueprint.GetComponent<WarhammerOverrideRateOfFire>();
		if (component != null)
		{
			return component.RateOfFire;
		}
		ItemEntityWeapon weaponToUse = GetWeaponToUse(context);
		if (!UseVariableRateOfAttack)
		{
			return weaponToUse?.Blueprint.RateOfFire ?? 0;
		}
		return RateOfAttack.Calculate(context);
	}

	private AbilityDeliveryTarget TriggerAttackRule(AbilityExecutionContext context, MechanicEntity target, int burstIndex, ItemEntityWeapon weapon)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			return null;
		}
		AbilityData ability = context.Ability;
		bool firstAttackCalculated = m_FirstAttackCalculated;
		AttackHitPolicyType attackHitPolicy = (m_FirstAttackHit ? AttackHitPolicyType.AutoHit : AttackHitPolicyType.AutoMiss);
		RulePerformAttack rulePerformAttack = new RulePerformAttack(maybeCaster, target, ability, burstIndex, disableWeaponAttackDamage: false, disableDodgeForAlly: false, null, null, firstAttackCalculated, attackHitPolicy);
		context.TriggerRule(rulePerformAttack);
		if (!m_FirstAttackCalculated)
		{
			m_FirstAttackHit = rulePerformAttack.ResultIsHit;
			m_FirstAttackCalculated = true;
		}
		if (maybeCaster is BaseUnitEntity attacker && target is BaseUnitEntity baseUnitEntity && baseUnitEntity.View != null && baseUnitEntity.View.HitFxManager != null)
		{
			baseUnitEntity.View.HitFxManager.HandleMeleeAttackHit(attacker, AttackResult.Hit, crit: false, weapon);
		}
		return new AbilityDeliveryTarget(target)
		{
			AttackRule = rulePerformAttack
		};
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverAOEDirectly(AbilityExecutionContext context)
	{
		OrientedPatternData pattern = context.Pattern;
		foreach (MechanicEntity mechanicEntity in Game.Instance.State.MechanicEntities)
		{
			if (IsValidTarget(context, mechanicEntity))
			{
				yield return DeliverHit(context, mechanicEntity, pattern);
			}
		}
	}

	private bool IsValidTarget(AbilityExecutionContext context, MechanicEntity entity)
	{
		if (!context.Ability.IsValidTargetForAttack(entity))
		{
			return false;
		}
		if ((context.Ability.IsMelee || context.Ability.IsScatter) && entity == context.Caster)
		{
			return false;
		}
		if (!AoEPatternHelper.WouldTargetEntity(context.Pattern, entity))
		{
			return false;
		}
		return true;
	}

	private AbilityDeliveryTarget DeliverHit(AbilityExecutionContext context, MechanicEntity target, OrientedPatternData pattern, Vector3? effectiveCasterPosition = null)
	{
		RulePerformAttack rulePerformAttack = new RulePerformAttack(context.Caster, target, context.Ability, 0, disableWeaponAttackDamage: false, disableDodgeForAlly: false, effectiveCasterPosition)
		{
			Reason = context
		};
		rulePerformAttack.RollPerformAttackRule.DangerArea.UnionWith(pattern.Nodes);
		context.TriggerRule(rulePerformAttack);
		return new AbilityDeliveryTarget(target)
		{
			AttackRule = rulePerformAttack,
			Projectile = null
		};
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}
}

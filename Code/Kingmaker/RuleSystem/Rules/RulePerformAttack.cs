using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformAttack : RulebookTargetEvent
{
	[CanBeNull]
	private readonly RuleRollDamage m_OverrideDamageRoll;

	private readonly bool m_DisableWeaponAttackDamage;

	private readonly bool m_DisableDodgeForAlly;

	public AbilityData Ability { get; }

	public RulePerformAttackRoll RollPerformAttackRule { get; }

	public RuleRollDamage RuleRollDamage => m_OverrideDamageRoll;

	public bool IsOverpenetration { get; set; }

	public Projectile Projectile { get; set; }

	public int AdditionalDamageInstancesCount { get; set; }

	[CanBeNull]
	public RuleDealDamage ResultDamageRule { get; private set; }

	[CanBeNull]
	public RuleDealDamage[] ResultAdditionalDamageRules { get; private set; }

	public int ResultDamageValue
	{
		get
		{
			RuleDealDamage[] resultAdditionalDamageRules = ResultAdditionalDamageRules;
			if (resultAdditionalDamageRules == null || resultAdditionalDamageRules.Length <= 0)
			{
				return ResultDamageRule?.Result ?? 0;
			}
			return (ResultDamageRule?.Result ?? 0) + ResultAdditionalDamageRules.Sum((RuleDealDamage i) => i.Result);
		}
	}

	[CanBeNull]
	public DamageData ResultOverpenetrationDamage => ResultDamageRule?.RollDamageRule.ResultOverpenetration;

	public RuleRollD100 ResultAttackRollD100 => RollPerformAttackRule.ResultChanceRule;

	[CanBeNull]
	public RulePerformDodge ResultDodgeRule => RollPerformAttackRule.ResultDodgeRule;

	[CanBeNull]
	public RuleRollParry ResultParryRule => RollPerformAttackRule.ResultParryRule;

	public HitLocation ResultHitLocation => RollPerformAttackRule.ResultHitLocation;

	public AttackResult Result => RollPerformAttackRule.Result;

	public bool ResultIsHit => RollPerformAttackRule.ResultIsHit;

	public bool ResultIsCoverHit => RollPerformAttackRule.ResultIsCoverHit;

	public int BurstIndex => RollPerformAttackRule.BurstIndex;

	public bool IsMelee => Ability.Weapon?.Blueprint.IsMelee ?? false;

	public RulePerformAttack([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex, bool disableWeaponAttackDamage, bool disableDodgeForAlly, [CanBeNull] RulePerformAttackRoll performAttackRoll, [CanBeNull] RuleRollDamage overrideDamageRoll)
		: base(initiator, target)
	{
		Ability = ability;
		RollPerformAttackRule = performAttackRoll ?? new RulePerformAttackRoll(initiator, target, ability, burstIndex, disableDodgeForAlly, null, null);
		m_DisableWeaponAttackDamage = disableWeaponAttackDamage;
		m_OverrideDamageRoll = overrideDamageRoll;
	}

	public RulePerformAttack([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex, bool disableWeaponAttackDamage = false, bool disableDodgeForAlly = false, Vector3? effectiveCasterPosition = null, Vector3? abilityTargetPosition = null, bool useSpecificAttackHitPolicy = false, AttackHitPolicyType attackHitPolicy = AttackHitPolicyType.Default)
		: base(initiator, target)
	{
		Ability = ability;
		RollPerformAttackRule = new RulePerformAttackRoll(initiator, target, ability, burstIndex, disableDodgeForAlly, effectiveCasterPosition, abilityTargetPosition)
		{
			OverrideAttackHitPolicy = useSpecificAttackHitPolicy,
			AttackHitPolicyType = attackHitPolicy
		};
		m_DisableWeaponAttackDamage = disableWeaponAttackDamage;
	}

	public RulePerformAttack([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] AbilityData ability, int burstIndex, bool disableWeaponAttackDamage, bool disableDodgeForAlly, [CanBeNull] RulePerformAttackRoll performAttackRoll, [CanBeNull] RuleRollDamage overrideDamageRoll)
		: this((MechanicEntity)initiator, (MechanicEntity)target, ability, burstIndex, disableWeaponAttackDamage, disableDodgeForAlly, performAttackRoll, overrideDamageRoll)
	{
	}

	public RulePerformAttack([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] AbilityData ability, int burstIndex, bool disableWeaponAttackDamage = false, bool disableDodgeForAlly = false, Vector3? effectiveCasterPosition = null, Vector3? abilityTargetPosition = null)
		: this((MechanicEntity)initiator, (MechanicEntity)target, ability, burstIndex, disableWeaponAttackDamage, disableDodgeForAlly, effectiveCasterPosition, abilityTargetPosition)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(RollPerformAttackRule);
		if (RollPerformAttackRule.ResultIsHit)
		{
			OnHit();
		}
		EventBus.RaiseEvent(delegate(IWarhammerAttackHandler h)
		{
			h.HandleAttack(this);
		});
	}

	private void OnHit()
	{
		if (!m_DisableWeaponAttackDamage)
		{
			ResultDamageRule = DealDamage(m_OverrideDamageRoll);
		}
		List<RuleDealDamage> list = TempList.Get<RuleDealDamage>();
		for (int i = 0; i < AdditionalDamageInstancesCount; i++)
		{
			list.Add(DealDamage(null));
		}
		if (list.Count > 0)
		{
			ResultAdditionalDamageRules = list.ToArray();
		}
	}

	[NotNull]
	private RuleDealDamage DealDamage([CanBeNull] RuleRollDamage overrideDamageRoll)
	{
		RuleRollDamage rollDamage = overrideDamageRoll;
		if (overrideDamageRoll == null)
		{
			DamageData damageData = new CalculateDamageParams(base.ConcreteInitiator, base.ConcreteTarget, Ability, RollPerformAttackRule).Trigger().ResultDamage;
			if (AdditionalDamageInstancesCount > 0)
			{
				damageData = damageData.Copy();
				int value = Mathf.RoundToInt(1f / (float)(AdditionalDamageInstancesCount + 1) * 100f);
				damageData.Modifiers.Add(ModifierType.PctMul_Extra, value, this, ModifierDescriptor.Weapon);
			}
			rollDamage = new RuleRollDamage(base.ConcreteInitiator, base.ConcreteTarget, damageData);
		}
		RuleDealDamage obj = new RuleDealDamage(base.ConcreteInitiator, base.ConcreteTarget, rollDamage)
		{
			DisableGameLog = false,
			FromRuleWarhammerAttackRoll = true,
			SourceAbility = Ability,
			Projectile = Projectile,
			ResultIsCritical = RollPerformAttackRule.ResultIsRighteousFury
		};
		Rulebook.Trigger(obj);
		return obj;
	}
}

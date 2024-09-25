using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.WeaponStats;

[Serializable]
[TypeId("06c886b0298c417a982b70b9df35ba6d")]
public abstract class WarhammerWeaponStatsModifier : MechanicEntityFactComponentDelegate, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor Descriptor;

	public ContextValueModifierWithType Damage;

	public ContextValueModifier DamageMin;

	public ContextValueModifier DamageMax;

	public ContextValueModifierWithType Penetration;

	public ContextValueModifierWithType Recoil;

	public ContextValueModifierWithType AdditionalHitChance;

	public ContextValueModifierWithType DodgePenetration;

	public ContextValueModifierWithType MaxDistance;

	public ContextValueModifierWithType RateOfFire;

	public ContextValueModifierWithType OverpenetrationFactor;

	protected void TryApply(RuleCalculateStatsWeapon rule)
	{
		if (Restrictions.IsPassed(base.Fact, rule, rule.Ability))
		{
			Apply(rule);
		}
	}

	protected void Apply(RuleCalculateStatsWeapon rule)
	{
		Damage.TryApply(rule.BaseDamage.Modifiers, base.Fact, Descriptor);
		DamageMin.TryApply(rule.BaseDamage.MinValueModifiers, base.Fact, Descriptor);
		DamageMax.TryApply(rule.BaseDamage.MaxValueModifiers, base.Fact, Descriptor);
		Penetration.TryApply(rule.BaseDamage.Penetration, base.Fact, Descriptor);
		Recoil.TryApply(rule.RecoilModifiers, base.Fact, Descriptor);
		AdditionalHitChance.TryApply(rule.AdditionalHitChanceModifiers, base.Fact, Descriptor);
		DodgePenetration.TryApply(rule.DodgePenetrationModifiers, base.Fact, Descriptor);
		MaxDistance.TryApply(rule.MaxDistanceModifiers, base.Fact, Descriptor);
		RateOfFire.TryApply(rule.RateOfFireModifiers, base.Fact, Descriptor);
		OverpenetrationFactor.TryApply(rule.OverpenetrationFactorModifiers, base.Fact, Descriptor);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

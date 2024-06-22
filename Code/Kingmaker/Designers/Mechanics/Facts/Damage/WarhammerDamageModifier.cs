using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("b05381e4f203a76418913b0f6b5323f8")]
public abstract class WarhammerDamageModifier : MechanicEntityFactComponentDelegate, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifier MinimumDamage = new ContextValueModifier();

	public ContextValueModifier MaximumDamage = new ContextValueModifier();

	public ContextValueModifier PenetrationMod = new ContextValueModifier();

	public ContextValueModifier PercentDamageModifier = new ContextValueModifier();

	public ContextValueModifier AbsorptionPenetration = new ContextValueModifier();

	public ContextValueModifier DeflectionPenetration = new ContextValueModifier();

	public ContextValueModifier UnmodifiableFlatDamageModifier = new ContextValueModifier();

	public ContextValueModifier UnmodifiablePercentDamageModifier = new ContextValueModifier();

	public ModifierDescriptor ModifierDescriptor;

	public bool ModifyEvenDirectDamage;

	public bool ModifyEvenDamageOverTime;

	protected void TryApply(RuleCalculateDamage rule)
	{
		using EntityFactComponentLoopGuard entityFactComponentLoopGuard = base.Runtime.RequestLoopGuard();
		if (entityFactComponentLoopGuard.Blocked || !Restrictions.IsPassed(base.Fact, rule, rule.Ability) || (rule.DamageType == DamageType.Direct && !ModifyEvenDirectDamage))
		{
			return;
		}
		if (!ModifyEvenDamageOverTime && rule.Reason.Fact != null)
		{
			BlueprintBuff blueprintBuff = ((rule.Reason.Fact.Blueprint is BlueprintBuff) ? (rule.Reason.Fact.Blueprint as BlueprintBuff) : null);
			if (blueprintBuff != null && blueprintBuff.AbilityGroups.Contains(BlueprintWarhammerRoot.Instance.CombatRoot.DamageOverTimeAbilityGroup))
			{
				return;
			}
		}
		using (ContextData<PropertyContextData>.Request().Setup(new PropertyContext(rule.ConcreteInitiator, base.Fact, rule.MaybeTarget, base.Context, rule, rule.Ability)))
		{
			if (PercentDamageModifier.Enabled)
			{
				int num = PercentDamageModifier.Calculate(base.Context);
				if (rule.MaybeTarget == base.Owner && base.Context.MaybeCaster != null)
				{
					IEnumerable<WarhammerMultiplyIncomingDamageBonus> components = base.Context.MaybeCaster.Facts.GetComponents<WarhammerMultiplyIncomingDamageBonus>();
					float num2 = (components.Any() ? (components.Sum((WarhammerMultiplyIncomingDamageBonus p) => p.PercentIncreaseMultiplier - 1f) + 1f) : 1f);
					num = (int)((float)num * num2);
				}
				rule.ValueModifiers.Add(ModifierType.PctAdd, num, base.Fact, ModifierDescriptor);
			}
			if (MinimumDamage.Enabled)
			{
				rule.MinValueModifiers.Add(MinimumDamage.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
			if (MaximumDamage.Enabled)
			{
				rule.MaxValueModifiers.Add(MaximumDamage.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
			if (PenetrationMod.Enabled)
			{
				rule.Penetration.Add(ModifierType.ValAdd, PenetrationMod.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
			if (AbsorptionPenetration.Enabled)
			{
				rule.Absorption.Add(ModifierType.ValAdd, AbsorptionPenetration.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
			if (DeflectionPenetration.Enabled)
			{
				rule.Deflection.Add(ModifierType.ValAdd, DeflectionPenetration.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
			if (UnmodifiableFlatDamageModifier.Enabled)
			{
				rule.ValueModifiers.Add(ModifierType.ValAdd_Extra, UnmodifiableFlatDamageModifier.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
			if (UnmodifiablePercentDamageModifier.Enabled)
			{
				rule.ValueModifiers.Add(ModifierType.PctMul_Extra, UnmodifiablePercentDamageModifier.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
		}
		OnApply();
	}

	protected virtual void OnApply()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

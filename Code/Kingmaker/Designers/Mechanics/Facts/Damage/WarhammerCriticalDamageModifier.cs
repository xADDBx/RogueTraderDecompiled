using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Serializable]
[AllowMultipleComponents]
[TypeId("9ea0953a7bcd4081863ab5c9f8d89e99")]
public abstract class WarhammerCriticalDamageModifier : UnitFactComponentDelegate, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifier PercentCriticalDamageModifier = new ContextValueModifier();

	public ContextValueModifier BonusCriticalDamageModifier = new ContextValueModifier();

	public ContextValueModifier BonusCriticalDamageMultipliers = new ContextValueModifier();

	public ContextValueModifier purePercentCriticalDamageModifier = new ContextValueModifier();

	public ModifierDescriptor ModifierDescriptor;

	protected void TryApply(RuleCalculateDamage rule)
	{
		if (Restrictions.IsPassed(base.Fact, rule, rule.Ability))
		{
			if (PercentCriticalDamageModifier.Enabled)
			{
				rule.CriticalDamageModifiers.Add(ModifierType.PctAdd, PercentCriticalDamageModifier.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
			if (BonusCriticalDamageModifier.Enabled)
			{
				rule.CriticalDamageModifiers.Add(ModifierType.ValAdd, BonusCriticalDamageModifier.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
			if (BonusCriticalDamageMultipliers.Enabled)
			{
				rule.CriticalDamageModifiers.Add(ModifierType.PctMul, BonusCriticalDamageMultipliers.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
			if (purePercentCriticalDamageModifier.Enabled)
			{
				rule.PureCriticalDamageModifiers.Add(ModifierType.PctAdd, purePercentCriticalDamageModifier.Calculate(base.Context), base.Fact, ModifierDescriptor);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

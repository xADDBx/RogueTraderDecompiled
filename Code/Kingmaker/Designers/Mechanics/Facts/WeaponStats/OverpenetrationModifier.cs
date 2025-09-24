using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.WeaponStats;

[Serializable]
[TypeId("f8c6ba4c7f6d4d25ac8c56aac2fecbec")]
public abstract class OverpenetrationModifier : MechanicEntityFactComponentDelegate, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ModifierDescriptor m_Descriptor;

	[SerializeField]
	private ContextValueModifierWithType m_OverpenetrationFactor;

	[SerializeField]
	private bool ApplyIgnoreOverpenetrationDamageDecreament;

	public void Apply(RuleCalculateStatsWeapon rule)
	{
		if (m_Restrictions.IsPassed(base.Fact, rule, rule.Ability))
		{
			m_OverpenetrationFactor.TryApply(rule.OverpenetrationFactorModifiers, base.Fact, m_Descriptor);
			if (ApplyIgnoreOverpenetrationDamageDecreament)
			{
				rule.BaseDamage.UnreducedOverpenetration = true;
			}
		}
	}

	public void ApplyOverpenIgnoreDecreament(RuleCalculateDamage rule)
	{
		if (m_Restrictions.IsPassed(base.Fact, rule, rule.Ability) && ApplyIgnoreOverpenetrationDamageDecreament)
		{
			rule.ResultDamage.UnreducedOverpenetration = true;
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

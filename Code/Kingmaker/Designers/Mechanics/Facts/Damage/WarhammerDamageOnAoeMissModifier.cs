using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[TypeId("6170069e54404f30bd5e0fbb19127e2a")]
public abstract class WarhammerDamageOnAoeMissModifier : UnitFactComponentDelegate, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifierWithType AoeMissDamageModifier;

	public ModifierDescriptor ModifierDescriptor;

	protected void TryApply(RuleCalculateDamage rule)
	{
		if (Restrictions.IsPassed(base.Fact, rule, rule.Ability) && AoeMissDamageModifier.Enabled)
		{
			rule.AoeMissDamageModifier = (Modifier: AoeMissDamageModifier, Fact: base.Fact, Descriptor: ModifierDescriptor);
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

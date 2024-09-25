using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("92afe7375fcc46cea53424f91e0480b5")]
public class CurrentBurstIndexAbilityGetter : PropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current burst index";
	}

	protected override int GetBaseValue()
	{
		if (base.PropertyContext.Rule is RuleCalculateDamage { RollPerformAttackRule: not null } ruleCalculateDamage)
		{
			return ruleCalculateDamage.RollPerformAttackRule.BurstIndex;
		}
		return 0;
	}
}

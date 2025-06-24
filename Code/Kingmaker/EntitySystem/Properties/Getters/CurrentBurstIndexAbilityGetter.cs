using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
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
		RulebookEvent rule = base.PropertyContext.Rule;
		return ((rule is RuleCalculateDamage ruleCalculateDamage) ? ruleCalculateDamage.RollPerformAttackRule : ((!(rule is RulePerformAttack rulePerformAttack)) ? null : rulePerformAttack.RollPerformAttackRule))?.BurstIndex ?? 0;
	}
}

using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Components;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("11c26869806dd4345b36424af209e8ce")]
public class CheckDamageFromDOTGetter : PropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		RulebookEvent rule = this.GetRule();
		if (rule is RulePerformAttack)
		{
			return 0;
		}
		if (rule is RuleDealDamage { Reason: { Fact: not null }, Reason: var reason2 } && reason2.Fact.Blueprint.HasLogic<DOTLogic>())
		{
			return 1;
		}
		if (rule is RuleCalculateDamage ruleCalculateDamage && rule.Reason.Fact != null && ruleCalculateDamage.Reason.Fact.Blueprint.HasLogic<DOTLogic>())
		{
			return 1;
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if damage comes from a Damage over Time";
	}
}

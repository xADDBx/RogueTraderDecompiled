using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("094b1e1bbfc048d9adf77c7b85e83358")]
public class CheckIsRighteousFuryGetter : PropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		RulebookEvent rule = this.GetRule();
		if (rule is RulePerformAttack rulePerformAttack)
		{
			if (!rulePerformAttack.ResultIsHit || !rulePerformAttack.RollPerformAttackRule.ResultIsRighteousFury)
			{
				return 0;
			}
			return 1;
		}
		if (rule is RuleDealDamage ruleDealDamage)
		{
			if (!ruleDealDamage.ResultIsCritical)
			{
				return 0;
			}
			return 1;
		}
		if (rule is RuleRollDamage ruleRollDamage)
		{
			if (!ruleRollDamage.Damage.IsCritical)
			{
				return 0;
			}
			return 1;
		}
		throw new ElementLogicException(this);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is Righteous Fury";
	}
}

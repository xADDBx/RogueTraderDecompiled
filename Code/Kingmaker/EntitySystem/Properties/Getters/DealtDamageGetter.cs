using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("71215231c9644775bd7120d21371ff31")]
public class DealtDamageGetter : PropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		RulebookEvent rule = this.GetRule();
		if (rule is RulePerformAttack rulePerformAttack)
		{
			return rulePerformAttack.ResultDamageValue;
		}
		if (rule is RuleDealDamage ruleDealDamage)
		{
			return ruleDealDamage.Result;
		}
		throw new ElementLogicException(this);
	}

	protected override string GetInnerCaption()
	{
		return "Damage";
	}
}

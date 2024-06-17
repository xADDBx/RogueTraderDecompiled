using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.EntitySystem.Properties.Getters.Damage;

[Serializable]
[TypeId("3b4bc8d66f98411787f6ac106e02604e")]
public class CheckDealtDamageGetter : CheckDamageGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override bool Check(out DamageType type, out DamageData data, out RulebookEvent rule)
	{
		rule = this.GetRule();
		RuleDealDamage ruleDealDamage = (rule as RuleDealDamage) ?? throw new ElementLogicException(this);
		data = ruleDealDamage.Damage;
		type = data?.Type ?? DamageType.None;
		return true;
	}

	protected override string GetInnerCaption()
	{
		return base.GetInnerCaption() + " (Dealt Damage)";
	}
}

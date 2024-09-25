using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("ad4af3e83698e154cb571c381c5531a4")]
public class RuleDamageCriticalGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IRule
{
	public CriticalParameterType CriticalParameterType;

	public bool FinalCriticalChance;

	protected override int GetBaseValue()
	{
		if (this.GetRule() is RuleCalculateDamage ruleCalculateDamage)
		{
			switch (CriticalParameterType)
			{
			case CriticalParameterType.BonusCriticalDamage:
				return ruleCalculateDamage.CriticalDamageModifiers.Value;
			case CriticalParameterType.BonusCriticalHitChance:
				if (FinalCriticalChance)
				{
					return ruleCalculateDamage.RollPerformAttackRule?.HitChanceRule.RighteousFuryChanceRule.ResultChance ?? 0;
				}
				return ruleCalculateDamage.RollPerformAttackRule?.HitChanceRule.RighteousFuryChanceRule.BonusCriticalChance ?? 0;
			}
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return CriticalParameterType switch
		{
			CriticalParameterType.BonusCriticalHitChance => "Bonus Critical Hit Chance", 
			CriticalParameterType.BonusCriticalDamage => "Bonus Critical Damage", 
			_ => "", 
		};
	}
}

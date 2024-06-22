using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("07756321110b4928b14d4f0bb31478d5")]
public class CheckIsHeavyWeaponGetter : PropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		RulebookEvent rule = this.GetRule();
		if (rule is RuleCalculateDamage ruleCalculateDamage)
		{
			AbilityData ability = ruleCalculateDamage.Ability;
			if ((object)ability == null || ability.GetWeaponStats().Weapon?.Blueprint.Heaviness != WeaponHeaviness.Heavy)
			{
				return 0;
			}
			return 1;
		}
		if (rule is RuleDealDamage ruleDealDamage)
		{
			AbilityData sourceAbility = ruleDealDamage.SourceAbility;
			if ((object)sourceAbility == null || sourceAbility.GetWeaponStats().Weapon?.Blueprint.Heaviness != WeaponHeaviness.Heavy)
			{
				return 0;
			}
			return 1;
		}
		if (rule is RulePerformAttack rulePerformAttack)
		{
			AbilityData ability2 = rulePerformAttack.Ability;
			if ((object)ability2 == null || ability2.GetWeaponStats().Weapon?.Blueprint.Heaviness != WeaponHeaviness.Heavy)
			{
				return 0;
			}
			return 1;
		}
		if (rule is RuleCalculateRighteousFuryChance ruleCalculateRighteousFuryChance)
		{
			AbilityData ability3 = ruleCalculateRighteousFuryChance.Ability;
			if ((object)ability3 == null || ability3.GetWeaponStats().Weapon?.Blueprint.Heaviness != WeaponHeaviness.Heavy)
			{
				return 0;
			}
			return 1;
		}
		throw new ElementLogicException(this);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if attack weapon is heavy";
	}
}

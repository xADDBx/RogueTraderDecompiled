using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("a9527063b166443eb98d0ed049b9560d")]
public class AbilityResourceWounds : BlueprintComponent, IAbilityResourceLogic, IAbilityRestriction
{
	public ContextValue Cost;

	public BlueprintUnitFactReference HealInsteadOfDamageFact;

	public ContextValue Heal;

	public bool IsSpendResource()
	{
		return true;
	}

	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		if (CalculateCost(ability) > CalculateResourceAmount(ability))
		{
			return ability.Caster.Facts.Contains((BlueprintUnitFact)HealInsteadOfDamageFact);
		}
		return true;
	}

	public string GetAbilityRestrictionUIText()
	{
		return LocalizedTexts.Instance.Reasons.NoResourcesWounds;
	}

	public void Spend(AbilityData ability)
	{
		int num = CalculateCost(ability);
		MechanicEntity caster = ability.Caster;
		if (caster.Facts.Contains((BlueprintUnitFact)HealInsteadOfDamageFact))
		{
			int num2 = Heal.Calculate(ability.CreateExecutionContext(ability.Caster));
			Rulebook.Trigger(new RuleHealDamage(caster, caster, num2, num2, 0));
			return;
		}
		DamageTypeDescription damageTypeDescription = new DamageTypeDescription
		{
			Type = DamageType.Direct
		};
		DamageData resultDamage = new CalculateDamageParams(caster, caster, ability, null, damageTypeDescription.CreateDamage(num, num), 0, 0, null, forceCrit: false, calculatedOverpenetration: false, doNotUseCrModifier: true, unmodifiable: true).Trigger().ResultDamage;
		Rulebook.Trigger(new RuleDealDamage(caster, caster, resultDamage)
		{
			SourceAbility = ability
		});
	}

	public int CalculateCost(AbilityData ability)
	{
		if (!ability.Caster.Facts.Contains((BlueprintUnitFact)HealInsteadOfDamageFact))
		{
			return Cost.Calculate(ability.CreateExecutionContext(ability.Caster));
		}
		return 1;
	}

	public int CalculateResourceAmount(AbilityData ability)
	{
		return ability.Caster.GetHealthOptional()?.HitPointsLeft ?? (-1);
	}
}

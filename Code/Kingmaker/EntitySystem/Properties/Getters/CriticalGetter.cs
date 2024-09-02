using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("13d4b63ae0754698a750e0631e10e189")]
public class CriticalGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IOptionalRule
{
	public CriticalParameterType CriticalParameterType;

	protected override int GetBaseValue()
	{
		AbilityData abilityData = base.CurrentEntity.GetBodyOptional()?.PrimaryHand.MaybeWeapon?.Abilities[0].Data ?? base.CurrentEntity.GetBodyOptional()?.SecondaryHand.MaybeWeapon?.Abilities[0].Data;
		if (abilityData == null || this.GetRule() is RulebookOptionalTargetEvent { FakeRule: not false })
		{
			return 0;
		}
		switch (CriticalParameterType)
		{
		case CriticalParameterType.BonusCriticalHitChance:
			return Rulebook.Trigger(new RuleCalculateRighteousFuryChance(base.CurrentEntity, null, abilityData)
			{
				FakeRule = true
			}).BonusCriticalChance;
		case CriticalParameterType.BonusCriticalDamage:
		{
			CalculateDamageParams calculateDamageParams = new CalculateDamageParams(base.CurrentEntity, null, abilityData);
			calculateDamageParams.FakeRule = true;
			return calculateDamageParams.Trigger().CriticalDamageModifiers.AllModifiersList.Sum((Modifier p) => p.Value) + 50;
		}
		default:
			return 0;
		}
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return CriticalParameterType switch
		{
			CriticalParameterType.BonusCriticalHitChance => "Bonus Critical Hit Chance of " + FormulaTargetScope.Current, 
			CriticalParameterType.BonusCriticalDamage => "Bonus Critical Damage of " + FormulaTargetScope.Current, 
			_ => "", 
		};
	}
}

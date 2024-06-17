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
		return CriticalParameterType switch
		{
			CriticalParameterType.BonusCriticalHitChance => Rulebook.Trigger(new RuleCalculateRighteousFuryChance(base.CurrentEntity, null, abilityData)
			{
				FakeRule = true
			}).BonusCriticalChance, 
			CriticalParameterType.BonusCriticalDamage => Rulebook.Trigger(new RuleCalculateDamage(base.CurrentEntity, null, abilityData)
			{
				FakeRule = true
			}).CriticalDamageModifiers.AllModifiersList.Sum((Modifier p) => p.Value) + 50, 
			_ => 0, 
		};
	}

	protected override string GetInnerCaption()
	{
		return CriticalParameterType switch
		{
			CriticalParameterType.BonusCriticalHitChance => "Bonus Critical Hit Chance", 
			CriticalParameterType.BonusCriticalDamage => "Bonus Critical Damage", 
			_ => "", 
		};
	}
}

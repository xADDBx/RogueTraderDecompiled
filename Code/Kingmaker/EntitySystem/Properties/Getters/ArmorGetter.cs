using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("be189bcf59b8561448110efad9cf9e3d")]
public class ArmorGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IOptionalRule
{
	public bool Deflection;

	public bool AgainstTarget;

	[ShowIf("AgainstTarget")]
	public PropertyTargetType Attacker;

	[ShowIf("AgainstTarget")]
	public PropertyTargetType Defender;

	public bool OnlyBodyArmour;

	public bool OnlyNegativeModifiers;

	public bool ItemBonusOnly;

	public bool WithoutPenetration;

	protected override int GetBaseValue()
	{
		MechanicEntity mechanicEntity = (AgainstTarget ? ((MechanicEntity)this.GetTargetByType(Attacker)) : base.CurrentEntity);
		MechanicEntity mechanicEntity2 = (AgainstTarget ? ((MechanicEntity)this.GetTargetByType(Defender)) : base.CurrentEntity);
		if (OnlyBodyArmour)
		{
			return (mechanicEntity2.GetBodyOptional()?.Armor.MaybeArmor?.Blueprint.DamageAbsorption).GetValueOrDefault();
		}
		if (mechanicEntity == null || mechanicEntity2 == null)
		{
			return 0;
		}
		RuleCalculateStatsArmor triggeredArmorRule = GetTriggeredArmorRule(mechanicEntity, mechanicEntity2);
		if (ItemBonusOnly)
		{
			return (Deflection ? triggeredArmorRule?.ResultDeflection : triggeredArmorRule?.ResultAbsorption).GetValueOrDefault();
		}
		RuleCalculateDamage triggeredDamageRule = GetTriggeredDamageRule(mechanicEntity, mechanicEntity2);
		return GetArmorWithModifiers(triggeredDamageRule);
	}

	private int GetArmorWithModifiers(RuleCalculateDamage damageRule)
	{
		if (!OnlyNegativeModifiers)
		{
			if (damageRule.TargetArmorStatsRule == null)
			{
				return 0;
			}
			if (!Deflection)
			{
				if (WithoutPenetration)
				{
					return damageRule.ResultDamage.AbsorptionPercentsWithoutPenetration;
				}
				return damageRule.ResultDamage.AbsorptionPercentsWithPenetration;
			}
			return damageRule.ResultDamage.Deflection.Value;
		}
		if (damageRule.TargetArmorStatsRule?.AbsorptionCompositeModifiers.AllModifiersList == null)
		{
			return 0;
		}
		if (!Deflection)
		{
			return GetModifiersValue(damageRule.TargetArmorStatsRule.AbsorptionCompositeModifiers.AllModifiersList);
		}
		return GetModifiersValue(damageRule.TargetArmorStatsRule.DeflectionCompositeModifiers.AllModifiersList);
	}

	private RuleCalculateDamage GetTriggeredDamageRule(MechanicEntity initiator, MechanicEntity target)
	{
		if (this.GetRule() is RuleCalculateDamage ruleCalculateDamage && ruleCalculateDamage.Initiator == initiator && ruleCalculateDamage.MaybeTarget == target && ruleCalculateDamage.IsTriggered)
		{
			return ruleCalculateDamage;
		}
		AbilityData ability = base.PropertyContext.Ability ?? (base.PropertyContext.MechanicContext as AbilityExecutionContext)?.Ability;
		return Rulebook.Trigger(new RuleCalculateDamage(initiator, target, ability)
		{
			FakeRule = true
		});
	}

	private RuleCalculateStatsArmor GetTriggeredArmorRule(MechanicEntity initiator, MechanicEntity target)
	{
		if (this.GetRule() is RuleCalculateDamage ruleCalculateDamage && ruleCalculateDamage.Initiator == initiator && ruleCalculateDamage.MaybeTarget == target && ruleCalculateDamage.IsTriggered)
		{
			return ruleCalculateDamage.TargetArmorStatsRule;
		}
		if (this.GetRule() is RuleCalculateStatsArmor ruleCalculateStatsArmor && ruleCalculateStatsArmor.Initiator == target && ruleCalculateStatsArmor.IsTriggered)
		{
			return ruleCalculateStatsArmor;
		}
		if (this.GetRule() is RulebookOptionalTargetEvent { FakeRule: not false })
		{
			return null;
		}
		AbilityData ability = base.PropertyContext.Ability ?? (base.PropertyContext.MechanicContext as AbilityExecutionContext)?.Ability;
		return Rulebook.Trigger(new RuleCalculateDamage(initiator, target, ability)
		{
			FakeRule = true
		}).TargetArmorStatsRule;
	}

	private static int GetModifiersValue(IEnumerable<Modifier> modifiers)
	{
		int num = 0;
		foreach (Modifier modifier in modifiers)
		{
			if (modifier.Value <= 0)
			{
				num += modifier.Value;
			}
		}
		return num;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string obj = (Deflection ? "Deflection" : "Absorption");
		string text = (AgainstTarget ? Attacker.Colorized() : FormulaTargetScope.Current);
		string text2 = (AgainstTarget ? Defender.Colorized() : FormulaTargetScope.Current);
		string text3 = ((text == text2) ? ("of " + text) : ("of " + text + " against " + text2));
		return obj + " " + text3;
	}
}

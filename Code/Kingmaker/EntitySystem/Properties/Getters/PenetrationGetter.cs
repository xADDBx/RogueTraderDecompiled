using System;
using System.Linq;
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

[TypeId("c204f04abdb243cc881d158d91869f2a")]
public class PenetrationGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IOptionalRule
{
	public PenetrationParameterType PenetrationParameterType;

	public bool AgainstTarget;

	[ShowIf("AgainstTarget")]
	public PropertyTargetType Attacker;

	[ShowIf("AgainstTarget")]
	public PropertyTargetType Defender;

	protected override int GetBaseValue()
	{
		AbilityData abilityData = this.GetAbility() ?? base.CurrentEntity.GetBodyOptional()?.PrimaryHand.MaybeWeapon?.Abilities[0].Data ?? base.CurrentEntity.GetBodyOptional()?.SecondaryHand.MaybeWeapon?.Abilities[0].Data;
		MechanicEntity mechanicEntity = (AgainstTarget ? ((MechanicEntity)this.GetTargetByType(Attacker)) : base.CurrentEntity);
		MechanicEntity mechanicEntity2 = (AgainstTarget ? ((MechanicEntity)this.GetTargetByType(Defender)) : null);
		if (abilityData == null || mechanicEntity == null || this.GetRule() is RulebookOptionalTargetEvent { FakeRule: not false })
		{
			return 0;
		}
		AbilityData ability = abilityData;
		if (mechanicEntity != abilityData.Caster)
		{
			AbilityData abilityData2 = (mechanicEntity.GetFirstWeapon() ?? mechanicEntity.GetSecondaryHandWeapon())?.Abilities[0].Data;
			if (abilityData2 != null)
			{
				ability = abilityData2;
			}
		}
		CalculateDamageParams calculateDamageParams;
		switch (PenetrationParameterType)
		{
		case PenetrationParameterType.ArmorPenetration:
			calculateDamageParams = new CalculateDamageParams(mechanicEntity, mechanicEntity2, ability);
			calculateDamageParams.FakeRule = true;
			return calculateDamageParams.Trigger().ResultDamage.Penetration.Value;
		case PenetrationParameterType.DodgePenetration:
		{
			UnitEntity unitEntity2 = (mechanicEntity2 as UnitEntity) ?? (mechanicEntity as UnitEntity);
			if (unitEntity2 == null)
			{
				return 0;
			}
			return Rulebook.Trigger(new RuleCalculateDodgeChance(unitEntity2, mechanicEntity, abilityData)
			{
				FakeRule = true
			}).AllModifiersList.Sum((Modifier p) => (p.Value < 0) ? p.Value : 0);
		}
		case PenetrationParameterType.ArmorPenetrationOverArmor:
		{
			calculateDamageParams = new CalculateDamageParams(mechanicEntity, mechanicEntity2, abilityData);
			calculateDamageParams.FakeRule = true;
			RuleCalculateDamage ruleCalculateDamage = calculateDamageParams.Trigger();
			int value = ruleCalculateDamage.ResultDamage.Penetration.Value;
			int value2 = ruleCalculateDamage.ResultDamage.Absorption.Value;
			int val = value - value2;
			return Math.Max(0, val);
		}
		case PenetrationParameterType.DodgePenetrationOverDodge:
		{
			UnitEntity unitEntity = (mechanicEntity2 as UnitEntity) ?? (mechanicEntity as UnitEntity);
			if (unitEntity == null)
			{
				return 0;
			}
			RuleCalculateDodgeChance ruleCalculateDodgeChance = Rulebook.Trigger(new RuleCalculateDodgeChance(unitEntity, mechanicEntity, abilityData)
			{
				FakeRule = true
			});
			return Math.Max(0, -ruleCalculateDodgeChance.UncappedNegativesCount);
		}
		default:
			return 0;
		}
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = "";
		text = ((!AgainstTarget) ? (" of " + FormulaTargetScope.Current + " against abstract target") : (" of " + Attacker.Colorized() + " against " + Defender.Colorized()));
		return PenetrationParameterType switch
		{
			PenetrationParameterType.ArmorPenetration => "Armor Penetration" + text, 
			PenetrationParameterType.DodgePenetration => "Dodge Penetration" + text, 
			PenetrationParameterType.ArmorPenetrationOverArmor => "Armor Penetration minus Armor" + text, 
			PenetrationParameterType.DodgePenetrationOverDodge => "Dodge Penetration minus Dodge" + text, 
			_ => "", 
		};
	}
}

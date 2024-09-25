using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("4eaba0ad9abe479eae3618b5e97d5c71")]
public class WarhammerArmorBonusConditional : MechanicEntityFactComponentDelegate, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValue BonusDeflectionValue;

	public ContextValue BonusAbsorptionValue;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool SpecificDamageType;

	[ShowIf("SpecificDamageType")]
	public DamageType Type;

	[ShowIf("SpecificDamageType")]
	public bool AllDamageExceptThisType;

	public bool OnlyFromAlliedAttacks;

	public bool IgnoreArmour;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if ((OnlyFromAlliedAttacks && (evt.MaybeTarget == null || evt.MaybeTarget.IsEnemy(evt.Initiator))) || !Restrictions.IsPassed(base.Fact, evt, evt.Ability))
		{
			return;
		}
		ItemEntityWeapon itemEntityWeapon = evt.Ability?.Weapon;
		bool flag = evt.DamageType == Type || (itemEntityWeapon != null && ((itemEntityWeapon?.Blueprint.DamageType.Type == Type && !AllDamageExceptThisType) || (itemEntityWeapon?.Blueprint.DamageType.Type != Type && AllDamageExceptThisType)));
		if ((!SpecificRangeType || (itemEntityWeapon != null && WeaponRangeType.IsSuitableWeapon(itemEntityWeapon))) && (!SpecificDamageType || flag))
		{
			if (IgnoreArmour)
			{
				evt.Deflection.Add(ModifierType.PctMul_Extra, 0, base.Fact);
				evt.Absorption.Add(ModifierType.PctMul_Extra, 0, base.Fact);
			}
			else
			{
				evt.Deflection.Add(ModifierType.ValAdd, BonusDeflectionValue.Calculate(base.Context), base.Fact);
				evt.Absorption.Add(ModifierType.ValAdd, BonusAbsorptionValue.Calculate(base.Context), base.Fact);
			}
			OnApply();
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	protected virtual void OnApply()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

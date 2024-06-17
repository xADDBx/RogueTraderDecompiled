using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("1d90fd0202fe4c54c92d5a2f2b94ed0c")]
public class WarhammerModifyAttackWithChoosenWeapon : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ITargetRulebookHandler<RuleCalculateParryChance>, IRulebookHandler<RuleCalculateParryChance>, ITargetRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateBurstCount>, IRulebookHandler<RuleCalculateBurstCount>, IHashable
{
	public ContextValue AdditionalHitChances;

	public ContextValue AdditionalDamageMin;

	public ContextValue AdditionalDamageMax;

	public ContextValue AdditionalArmorPenetration;

	public ContextValue ParryNegation;

	public ContextValue RecoilBonusPercent;

	public ContextValue BurstCountPercent;

	public ContextValue BurstCountBonusMinimum;

	public bool IsApplicable(ItemEntityWeapon weapon)
	{
		if (weapon != null)
		{
			return weapon == base.Owner.GetOptional<WarhammerUnitPartChooseWeapon>()?.ChosenWeapon;
		}
		return false;
	}

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		if (IsApplicable(evt.Ability.Weapon))
		{
			evt.HitChanceValueModifiers.Add(AdditionalHitChances.Calculate(base.Context), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (IsApplicable(evt.Ability.Weapon))
		{
			evt.MinValueModifiers.Add(AdditionalDamageMin.Calculate(base.Context), base.Fact);
			evt.MaxValueModifiers.Add(AdditionalDamageMax.Calculate(base.Context), base.Fact);
			evt.Penetration.Add(ModifierType.ValAdd, AdditionalArmorPenetration.Calculate(base.Context), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateParryChance evt)
	{
		if (IsApplicable(evt.Ability?.Weapon))
		{
			evt.ParryValueModifiers.Add(-ParryNegation.Calculate(base.Context), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateParryChance evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateBurstCount evt)
	{
		if (IsApplicable(evt.Ability.Weapon))
		{
			int val = evt.BaseBurstCount * BurstCountPercent.Calculate(base.Context) / 100;
			evt.BonusCount += Math.Max(BurstCountBonusMinimum.Calculate(base.Context), val);
		}
	}

	public void OnEventDidTrigger(RuleCalculateBurstCount evt)
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

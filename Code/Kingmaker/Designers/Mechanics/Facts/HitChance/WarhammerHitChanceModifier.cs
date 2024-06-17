using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.HitChance;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("349f754fc8e340fc9ba122ad784ace6c")]
public abstract class WarhammerHitChanceModifier : MechanicEntityFactComponentDelegate, IHashable
{
	[Flags]
	public enum PropertyType
	{
		HitChance = 1,
		CoverPenetration = 4,
		CoverMagnitude = 8,
		RighteousFury = 0x10
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[EnumFlagsAsDropdown]
	public PropertyType Properties = PropertyType.HitChance;

	[ShowIf("ModifyHitChance")]
	public ContextValue HitChance;

	[ShowIf("ModifyRighteousFury")]
	public ContextValue RighteousFuryChance;

	[InfoBox("Used as (1 - rule.CoverPenetration) * (CoverPenetrationPercent / 100f)")]
	[ShowIf("ModifyCoverPenetration")]
	public ContextValue CoverPenetrationPercent;

	[InfoBox("Used as +CoverMagnitude% to hit cover")]
	[ShowIf("ModifyCoverMagnitude")]
	public ContextValue CoverMagnitude;

	public bool AutoCrit;

	private bool ModifyHitChance => (Properties & PropertyType.HitChance) != 0;

	public bool ModifyCoverPenetration => (Properties & PropertyType.CoverPenetration) != 0;

	private bool ModifyCoverMagnitude => (Properties & PropertyType.CoverMagnitude) != 0;

	private bool ModifyRighteousFury => (Properties & PropertyType.RighteousFury) != 0;

	protected void TryApply(RuleCalculateHitChances rule)
	{
		if (!ModifyHitChance)
		{
			if (AutoCrit && Restrictions.IsPassed(base.Fact, rule, rule.Ability))
			{
				rule.AutoCrits.Add(base.Fact);
			}
		}
		else if (Restrictions.IsPassed(base.Fact, rule, rule.Ability))
		{
			if (AutoCrit)
			{
				rule.AutoCrits.Add(base.Fact);
			}
			rule.HitChanceValueModifiers.Add(HitChance.Calculate(base.Context), base.Fact);
		}
	}

	protected void TryApply(RuleCalculateScatterShotHitDirectionProbability rule)
	{
		if (ModifyHitChance && Restrictions.IsPassed(base.Fact, rule, rule.Ability))
		{
			rule.EffectiveBSModifiers.Add(HitChance.Calculate(base.Context), base.Fact);
		}
	}

	protected void TryApply(RuleCalculateCoverHitChance rule)
	{
		if (Restrictions.IsPassed(base.Fact, rule, rule.Ability))
		{
			if (ModifyCoverMagnitude)
			{
				int value = CoverMagnitude.Calculate(base.Context);
				rule.ChanceValueModifiers.Add(value, base.Fact);
			}
			if (ModifyCoverPenetration)
			{
				int value2 = CoverPenetrationPercent.Calculate(base.Context);
				rule.ChancePercentModifiers.Add(value2, base.Fact);
			}
		}
	}

	protected void TryApply(RuleCalculateRighteousFuryChance rule)
	{
		if (ModifyRighteousFury && Restrictions.IsPassed(base.Fact, rule, rule.Ability))
		{
			int value = RighteousFuryChance.Calculate(base.Context);
			rule.ChanceModifiers.Add(value, base.Fact);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}

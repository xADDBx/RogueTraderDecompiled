using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.DodgeChance;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("aa5a2b1c16294395999aa0ac5f146f31")]
public abstract class WarhammerDodgeChanceModifier : MechanicEntityFactComponentDelegate, IHashable
{
	[Flags]
	public enum PropertyType
	{
		DodgeChance = 1
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[EnumFlagsAsDropdown]
	public PropertyType Properties = PropertyType.DodgeChance;

	[ShowIf("ModifyDodgeChance")]
	public ContextValue DodgeChance;

	public bool PercentDodgeModifier;

	public bool PercentMultiplierModifier;

	public bool SetMinimumDodgeChance;

	[ShowIf("SetMinimumDodgeChance")]
	public ContextValue MinimumDodgeChance;

	private bool ModifyDodgeChance => (Properties & PropertyType.DodgeChance) != 0;

	protected void TryApply(RuleCalculateDodgeChance rule)
	{
		if (base.Fact.ConcreteOwner.GetOptional<PartPreviewUnit>() != null || !Restrictions.IsPassed(base.Fact, rule, rule.Ability))
		{
			return;
		}
		if (ModifyDodgeChance)
		{
			if (PercentDodgeModifier)
			{
				rule.DodgePercentModifiers.Add(DodgeChance.Calculate(base.Context), base.Fact);
			}
			else if (PercentMultiplierModifier)
			{
				rule.DodgePercentMultiplierModifier.Add(ModifierType.PctMul_Extra, DodgeChance.Calculate(base.Context), base.Fact);
			}
			else
			{
				rule.DodgeValueModifiers.Add(DodgeChance.Calculate(base.Context), base.Fact);
			}
		}
		if (SetMinimumDodgeChance)
		{
			rule.MinimumDodgeValueModifier.Add(MinimumDodgeChance.Calculate(base.Context), base.Fact);
		}
		OnApply();
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

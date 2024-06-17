using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.ParryChance;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("a9480d21aeee4a940a478882bf1736fa")]
public abstract class WarhammerParryChanceModifier : MechanicEntityFactComponentDelegate, IHashable
{
	[Flags]
	public enum PropertyType
	{
		ParryChance = 1,
		AttackerWeaponSkillBonus = 2,
		DefenderWeaponSkillBonus = 4
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[EnumFlagsAsDropdown]
	public PropertyType Properties = PropertyType.ParryChance;

	[ShowIf("ModifyParryChance")]
	public ContextValue ParryChance;

	[ShowIf("ModifyAttackerWeaponSkillBonus")]
	public ContextValue AttackerWeaponSkillBonus;

	[ShowIf("ModifyDefenderWeaponSkillBonus")]
	public ContextValue DefenderWeaponSkillBonus;

	private bool ModifyParryChance => (Properties & PropertyType.ParryChance) != 0;

	private bool ModifyAttackerWeaponSkillBonus => (Properties & PropertyType.AttackerWeaponSkillBonus) != 0;

	private bool ModifyDefenderWeaponSkillBonus => (Properties & PropertyType.DefenderWeaponSkillBonus) != 0;

	protected void TryApply(RuleCalculateParryChance rule)
	{
		if (Restrictions.IsPassed(base.Fact, rule, rule.Ability))
		{
			if (ModifyParryChance)
			{
				rule.ParryValueModifiers.Add(ParryChance.Calculate(base.Context), base.Fact);
			}
			if (ModifyAttackerWeaponSkillBonus)
			{
				rule.AttackerWeaponSkillValueModifiers.Add(AttackerWeaponSkillBonus.Calculate(base.Context), base.Fact);
			}
			if (ModifyDefenderWeaponSkillBonus)
			{
				rule.DefenderCurrentAttackSkillValueModifiers.Add(DefenderWeaponSkillBonus.Calculate(base.Context), base.Fact);
			}
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

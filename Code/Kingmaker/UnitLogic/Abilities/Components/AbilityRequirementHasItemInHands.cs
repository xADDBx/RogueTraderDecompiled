using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("97314ad9d71c4080a029cbd00e961acb")]
public class AbilityRequirementHasItemInHands : BlueprintComponent, IAbilityRestriction
{
	private enum RequirementType
	{
		HasMeleeWeapon,
		HasRangedWeapon
	}

	[SerializeField]
	private RequirementType m_Type;

	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		return m_Type switch
		{
			RequirementType.HasMeleeWeapon => ability.Caster.GetThreatHandMelee() != null, 
			RequirementType.HasRangedWeapon => ability.Caster.GetThreatHandRanged() != null, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public string GetAbilityRestrictionUIText()
	{
		if (m_Type == RequirementType.HasMeleeWeapon)
		{
			return LocalizedTexts.Instance.Reasons.MeleeWeaponRequired;
		}
		return "<some weapon required>";
	}
}

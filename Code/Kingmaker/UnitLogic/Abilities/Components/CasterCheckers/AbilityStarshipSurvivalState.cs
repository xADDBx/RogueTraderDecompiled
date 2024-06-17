using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("06c3e1c03ba57754b85c5f3434e2ca54")]
public class AbilityStarshipSurvivalState : BlueprintComponent, IAbilityCasterRestriction
{
	public int healthPctEqualOrLess = 100;

	public int oneOfShieldsPctEqualOrLess = 100;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		PartHealth healthOptional = caster.GetHealthOptional();
		if (healthOptional == null)
		{
			return false;
		}
		if (Mathf.RoundToInt((float)(healthOptional.MaxHitPoints - healthOptional.Damage) * 100f / (float)healthOptional.MaxHitPoints) > healthPctEqualOrLess)
		{
			return false;
		}
		if ((caster as StarshipEntity).Shields.MinPercent() > oneOfShieldsPctEqualOrLess)
		{
			return false;
		}
		return true;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.TargetHPCondition;
	}
}

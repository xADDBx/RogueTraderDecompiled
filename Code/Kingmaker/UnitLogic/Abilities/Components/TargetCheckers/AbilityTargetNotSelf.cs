using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("830935facef790d4cb9e56c3869a31bb")]
public class AbilityTargetNotSelf : BlueprintComponent, IAbilityTargetRestriction
{
	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return target.Entity != ability.Caster;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetNotSelf;
	}
}

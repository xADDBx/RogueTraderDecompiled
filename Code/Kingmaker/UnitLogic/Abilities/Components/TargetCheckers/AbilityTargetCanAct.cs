using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[TypeId("61f3388875184a4cac4ac8164eea0557")]
public class AbilityTargetCanAct : BlueprintComponent, IAbilityTargetRestriction
{
	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return (target.Entity?.GetStateOptional()?.CanAct).GetValueOrDefault();
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetCanAct;
	}
}

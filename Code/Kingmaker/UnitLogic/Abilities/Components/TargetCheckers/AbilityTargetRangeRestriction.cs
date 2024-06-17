using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[TypeId("70218ee706994ffabcba56f6a149ed6a")]
public class AbilityTargetRangeRestriction : BlueprintComponent, IAbilityTargetRestriction
{
	public Feet Distance;

	public CompareOperation.Type CompareType;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		float checkValue = ability.Caster.DistanceTo(target.Point);
		return CompareType.CheckCondition(checkValue, Distance.Meters);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return (CompareType == CompareOperation.Type.Greater || CompareType == CompareOperation.Type.GreaterOrEqual) ? BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooClose : BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsTooFar;
	}
}

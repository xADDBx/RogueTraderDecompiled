using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target Empty Cell")]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("df9b68897c9844fcbd4540814e596952")]
public class AbilityTargetEmptyCell : BlueprintComponent, IAbilityTargetRestriction
{
	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return !WarhammerBlockManager.Instance.NodeContainsAny(target.NearestNode);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.NotAllowedCellToCast;
	}
}

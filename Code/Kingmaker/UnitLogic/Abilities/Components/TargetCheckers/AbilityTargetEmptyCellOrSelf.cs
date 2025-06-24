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

[ComponentName("Predicates/Target Empty Cell Or Self")]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("465f03bb1ded423c8d3c50b4ed18c484")]
public class AbilityTargetEmptyCellOrSelf : BlueprintComponent, IAbilityTargetRestriction
{
	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		CustomGridNodeBase nearestNode = target.NearestNode;
		if (!WarhammerBlockManager.Instance.NodeContainsAnyExcept(nearestNode, ability.Caster.MaybeMovementAgent.Blocker))
		{
			return !AbilityTargetEmptyCell.IsVirtualPositionBlockingCell(nearestNode);
		}
		return false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.NotAllowedCellToCast;
	}
}

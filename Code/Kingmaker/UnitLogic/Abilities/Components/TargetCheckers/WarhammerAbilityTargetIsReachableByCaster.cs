using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[TypeId("64b9b460b89091f43b1a76354b5ae77f")]
public class WarhammerAbilityTargetIsReachableByCaster : BlueprintComponent, IAbilityTargetRestriction
{
	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (ability.Caster is StarshipEntity starshipEntity)
		{
			ForcedPath forcedPath = starshipEntity.Navigation.FindPath(target.Point);
			bool result = forcedPath.path.Count > 0;
			PathPool.Pool(forcedPath);
			return result;
		}
		if (ability.Caster.MaybeMovementAgent == null)
		{
			return false;
		}
		ForcedPath forcedPath2 = PathfindingService.Instance.FindPathRT_Blocking(ability.Caster.MaybeMovementAgent, target.Point, 10000f);
		bool result2 = !forcedPath2.error;
		PathPool.Pool(forcedPath2);
		return result2;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return "<target is not reachable>";
	}
}

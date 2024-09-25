using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("03dfa0208161dbc44a0603b03369ff4b")]
public class StarshipOverrideCustomTeleportLocation : BlueprintComponent, IAbilityCasterRestriction, IAbilityTargetRestriction
{
	private enum OverrideMode
	{
		OverideForRamPosition
	}

	[SerializeField]
	private OverrideMode m_OverrideMode;

	[SerializeField]
	private int distanceLimit = -1;

	public Vector3? GetActionPosition(BaseUnitEntity caster, TargetWrapper target, Vector3 casterPosition)
	{
		return GetNodesToTp(caster as StarshipEntity, target).MinBy((Vector3 pos) => (casterPosition - pos).sqrMagnitude);
	}

	private AbilityCustomStarshipRam GetRamData(MechanicEntity caster)
	{
		return caster.Facts.GetComponents<AbilityCustomStarshipRam>().FirstOrDefault();
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		AbilityCustomStarshipRam ramData = GetRamData(caster);
		if (ramData == null)
		{
			return false;
		}
		float? num = (caster as BaseUnitEntity)?.CombatState?.ActionPointsBlue;
		if (num.HasValue)
		{
			float valueOrDefault = num.GetValueOrDefault();
			return valueOrDefault >= (float)ramData.MinDistance;
		}
		return false;
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return GetNodesToTp(ability.Caster as StarshipEntity, target).Any();
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	private List<Vector3> GetNodesToTp(StarshipEntity starship, TargetWrapper target)
	{
		AbilityCustomStarshipRam ramData;
		float blueAP;
		List<Vector3> result;
		CustomGridNodeBase startingNode;
		int lastDiagonalCount;
		if (starship != null && target.Entity is StarshipEntity starshipEntity)
		{
			ramData = GetRamData(starship);
			if (ramData != null)
			{
				float? num = starship.CombatState?.ActionPointsBlue;
				if (num.HasValue)
				{
					blueAP = num.GetValueOrDefault();
					result = new List<Vector3>();
					startingNode = (CustomGridNodeBase)AstarPath.active.GetNearest(target.Point).node;
					int num2 = CustomGraphHelper.GuessDirection(starshipEntity.Forward);
					lastDiagonalCount = starship.CombatState.LastDiagonalCount;
					int num3 = CustomGraphHelper.LeftNeighbourDirection[CustomGraphHelper.LeftNeighbourDirection[num2]];
					EvaluateDirection(num3);
					num3 = CustomGraphHelper.LeftNeighbourDirection[num3];
					EvaluateDirection(num3);
					num3 = CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[num2]];
					EvaluateDirection(num3);
					num3 = CustomGraphHelper.RightNeighbourDirection[num3];
					EvaluateDirection(num3);
					num3 = CustomGraphHelper.OppositeDirections[num2];
					EvaluateDirection(num3);
					return result;
				}
			}
		}
		return null;
		void EvaluateDirection(int direction)
		{
			CustomGridNodeBase customGridNodeBase = null;
			CustomGridNodeBase currentNode = startingNode;
			int diagonalCount = lastDiagonalCount;
			int rayLength = 0;
			while (!starship.Navigation.CanStand(currentNode, direction))
			{
				if (!AdvanceAlongDirection())
				{
					return;
				}
			}
			do
			{
				if (rayLength >= ramData.MinDistance)
				{
					customGridNodeBase = currentNode;
				}
			}
			while (AdvanceAlongDirection() && starship.Navigation.CanStand(currentNode, direction));
			if (customGridNodeBase != null)
			{
				result.Add(customGridNodeBase.Vector3Position);
			}
			bool AdvanceAlongDirection()
			{
				_ = currentNode;
				currentNode = currentNode.GetNeighbourAlongDirection(direction);
				diagonalCount += ((direction > 3) ? 1 : 0);
				rayLength += ((diagonalCount % 2 != 0 || direction <= 3) ? 1 : 2);
				if ((float)rayLength <= blueAP + (float)ramData.BonusDistanceOnAttackAttempt(starship))
				{
					if (distanceLimit >= 0)
					{
						return rayLength <= distanceLimit;
					}
					return true;
				}
				return false;
			}
		}
	}
}

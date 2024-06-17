using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Formations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class FollowersFormationController : BaseUnitController, IControllerEnable, IController, IControllerDisable
{
	private const float DestinationDiffTolerance = 1f;

	private const int FollowersFormationCapacity = 20;

	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		UnitPartFollowedByUnits optional = unit.GetOptional<UnitPartFollowedByUnits>();
		if (optional == null || optional.Followers.Count == 0)
		{
			return;
		}
		float repathCooldownSec = Game.Instance.BlueprintRoot.Formations.FollowersFormation.RepathCooldownSec;
		if (!optional.ForceRefresh && Game.Instance.TimeController.GameTime.TotalSeconds < optional.LastRefreshTime + (double)repathCooldownSec)
		{
			return;
		}
		optional.LastRefreshTime = Game.Instance.TimeController.GameTime.TotalSeconds;
		Vector3 unitDestination = GetUnitDestination(unit);
		float num = GeometryUtils.SqrMechanicsDistance(unitDestination, optional.LastKnownDestination);
		if (!optional.ForceRefresh && num < 1f)
		{
			return;
		}
		optional.ForceRefresh = false;
		optional.LastKnownDestination = unitDestination;
		optional.FollowersActionTypesTemp.Clear();
		List<AbstractUnitEntity> list = TempList.Get<AbstractUnitEntity>();
		uint area = ObstacleAnalyzer.GetArea(unit.Position);
		foreach (AbstractUnitEntity follower in optional.Followers)
		{
			if (!ShouldSkipProcessing(follower))
			{
				FollowerAction? followerAction = optional.GetFollowerAction(follower);
				if (!followerAction.HasValue || followerAction.Value.Type != FollowerActionType.Teleport || !(followerAction.Value.Position != follower.Position))
				{
					uint area2 = ObstacleAnalyzer.GetArea(follower.Position);
					list.Add(follower);
					optional.FollowersActionTypesTemp[follower] = ((area != area2) ? FollowerActionType.Teleport : FollowerActionType.Move);
				}
			}
		}
		Vector3 followersFrontPosition = GetFollowersFrontPosition(unit);
		foreach (List<AbstractUnitEntity> item in list.Slice(20))
		{
			PrepareFormation(optional, item, followersFrontPosition, optional.FollowersActionTypesTemp);
		}
	}

	public Dictionary<AbstractUnitEntity, FollowerAction> CalculateTeleportToLeaderDestinations(UnitPartFollowedByUnits leader)
	{
		leader.FollowersActionTypesTemp.Clear();
		List<AbstractUnitEntity> list = TempList.Get<AbstractUnitEntity>();
		foreach (AbstractUnitEntity follower in leader.Followers)
		{
			if (!ShouldSkipProcessing(follower))
			{
				list.Add(follower);
				leader.FollowersActionTypesTemp[follower] = FollowerActionType.Teleport;
			}
		}
		foreach (AbstractUnitEntity item in list)
		{
			leader.FollowerDesiredActions.Remove(item);
		}
		Vector3 followersFrontPosition = GetFollowersFrontPosition(leader.Owner);
		foreach (List<AbstractUnitEntity> item2 in list.Slice(20))
		{
			PrepareFormation(leader, item2, followersFrontPosition, leader.FollowersActionTypesTemp);
		}
		Dictionary<AbstractUnitEntity, FollowerAction> dictionary = new Dictionary<AbstractUnitEntity, FollowerAction>();
		foreach (AbstractUnitEntity item3 in list)
		{
			dictionary[item3] = leader.FollowerDesiredActions[item3];
		}
		return dictionary;
	}

	private static bool ShouldSkipProcessing(AbstractUnitEntity follower)
	{
		if (follower.LifeState.State == UnitLifeState.Dead)
		{
			return true;
		}
		UnitPartFollowUnit optional = follower.GetOptional<UnitPartFollowUnit>();
		if (optional == null)
		{
			return false;
		}
		if (!optional.FollowWhileCutscene && follower.CutsceneControlledUnit?.GetCurrentlyActive() != null)
		{
			return true;
		}
		if (!optional.FollowInCombat)
		{
			return optional.Leader.IsInCombat;
		}
		return false;
	}

	private void PrepareFormation(UnitPartFollowedByUnits leader, IList<AbstractUnitEntity> followers, Vector3 position, Dictionary<AbstractUnitEntity, FollowerActionType> desiredActions)
	{
		if (!followers.Empty())
		{
			FollowersFormation followersFormation = Game.Instance.BlueprintRoot.Formations.FollowersFormation;
			List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
			list.Add(leader.Owner);
			Span<Vector3> resultPositions = stackalloc Vector3[followers.Count];
			PartyFormationHelper.FillFormationPositions(position, FormationAnchor.Front, ClickGroundHandler.GetDirection(position, list), followers, followers, followersFormation, resultPositions, 1f, forceRelax: true);
			for (int i = 0; i < followers.Count; i++)
			{
				CreateFollowerAction(followers[i], leader, resultPositions[i], desiredActions[followers[i]]);
			}
		}
	}

	public void OnEnable()
	{
		ClearCache();
	}

	public void OnDisable()
	{
		ClearCache();
	}

	private static void ClearCache()
	{
		foreach (AbstractUnitEntity item in Game.Instance.State.AllUnits.All)
		{
			item.GetOptional<UnitPartFollowedByUnits>()?.ClearCache();
		}
	}

	private static Vector3 GetFollowersFrontPosition(AbstractUnitEntity leader)
	{
		Vector2 playerOffset = Game.Instance.BlueprintRoot.Formations.FollowersFormation.PlayerOffset;
		Vector3 unitDestination = GetUnitDestination(leader);
		Quaternion orientationQuaternion = GetOrientationQuaternion(leader);
		return unitDestination + orientationQuaternion * new Vector3(playerOffset.x, 0f, playerOffset.y);
	}

	private static Quaternion GetOrientationQuaternion(AbstractUnitEntity unit)
	{
		return Quaternion.Euler(0f, unit.Commands.CurrentMoveTo?.Orientation ?? unit.Orientation, 0f);
	}

	private static float GetOrientation(BaseUnitEntity unit)
	{
		return unit.Commands.CurrentMoveTo?.Orientation ?? unit.Orientation;
	}

	private static Vector3 GetUnitDestination(AbstractUnitEntity unit)
	{
		return unit.Commands.Current?.ApproachPoint ?? unit.Commands.CurrentMoveTo?.ApproachPoint ?? unit.Position;
	}

	public static void CreateFollowerAction(AbstractUnitEntity follower, UnitPartFollowedByUnits leader, Vector3 position, FollowerActionType type)
	{
		float num = Game.Instance.BlueprintRoot.Formations.FollowersFormation.LookAngleRandomSpread / 2f;
		FollowerAction value = new FollowerAction(position, GetOrientation(leader.Owner) + leader.Owner.Random.Range(0f - num, num), type);
		leader.FollowerDesiredActions[follower] = value;
	}
}

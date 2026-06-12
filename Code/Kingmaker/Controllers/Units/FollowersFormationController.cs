using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Formations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Controllers.Units;

public class FollowersFormationController : BaseUnitController, IControllerEnable, IController, IControllerDisable
{
	private const float DestinationDiffTolerance = 1f;

	private const int FollowersFormationCapacity = 20;

	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		UnitPartFollowedByUnits leader = unit.GetOptional<UnitPartFollowedByUnits>();
		if (leader == null || leader.GetActiveFollowers().Count() == 0)
		{
			return;
		}
		float repathCooldownSec = Game.Instance.BlueprintRoot.Formations.FollowersFormation.RepathCooldownSec;
		if (!leader.ForceRefresh && Game.Instance.TimeController.GameTime.TotalSeconds < leader.LastRefreshTime + (double)repathCooldownSec)
		{
			return;
		}
		leader.LastRefreshTime = Game.Instance.TimeController.GameTime.TotalSeconds;
		Vector3 unitDestination = GetUnitDestination(unit);
		float num = GeometryUtils.SqrMechanicsDistance(unitDestination, leader.LastKnownDestination);
		if (!leader.ForceRefresh && num < 1f)
		{
			return;
		}
		leader.ForceRefresh = false;
		leader.LastKnownDestination = unitDestination;
		Dictionary<AbstractUnitEntity, FollowerActionType> actionTypes;
		uint leaderNodeArea;
		using (CollectionPool<Dictionary<AbstractUnitEntity, FollowerActionType>, KeyValuePair<AbstractUnitEntity, FollowerActionType>>.Get(out actionTypes))
		{
			List<AbstractUnitEntity> list = TempList.Get<AbstractUnitEntity>();
			List<AbstractUnitEntity> list2 = TempList.Get<AbstractUnitEntity>();
			leaderNodeArea = ObstacleAnalyzer.GetArea(unit.Position);
			foreach (AbstractUnitEntity follower in leader.Followers)
			{
				ProcessFollower(follower, list);
			}
			foreach (AbstractUnitEntity independentFollower in leader.IndependentFollowers)
			{
				UnitMoveTo currentMoveTo = unit.Commands.CurrentMoveTo;
				if (currentMoveTo == null || !currentMoveTo.Params.LeaveFollowers)
				{
					ProcessFollower(independentFollower, list2);
				}
			}
			Vector3 followersFrontPosition = GetFollowersFrontPosition(unit);
			foreach (List<AbstractUnitEntity> item in list.Slice(20))
			{
				PrepareFormation(leader, item, followersFrontPosition, actionTypes);
			}
			Quaternion orientationQuaternion = GetOrientationQuaternion(unit);
			foreach (AbstractUnitEntity item2 in list2)
			{
				CreateIndependentFollowerAction(item2, leader, unitDestination, orientationQuaternion, actionTypes[item2]);
			}
		}
		void ProcessFollower(AbstractUnitEntity follower, List<AbstractUnitEntity> resultList)
		{
			if (!ShouldSkipProcessing(follower))
			{
				FollowerAction? followerAction = leader.GetFollowerAction(follower);
				if (!followerAction.HasValue || followerAction.Value.Type != FollowerActionType.Teleport || !(followerAction.Value.Position != follower.Position))
				{
					resultList.Add(follower);
					actionTypes[follower] = GetActionType(follower, leaderNodeArea);
				}
			}
		}
	}

	private static FollowerActionType GetActionType(AbstractUnitEntity follower, uint leaderNodeArea)
	{
		uint area = ObstacleAnalyzer.GetArea(follower.Position);
		if (leaderNodeArea != area)
		{
			return FollowerActionType.Teleport;
		}
		return FollowerActionType.Move;
	}

	public Dictionary<AbstractUnitEntity, FollowerAction> CalculateFollowerActions(UnitPartFollowedByUnits leader, Vector3 position, float? orientation = null, bool alwaysTeleport = false, bool isCutsceneCommand = false)
	{
		Dictionary<AbstractUnitEntity, FollowerActionType> value;
		uint leaderNodeArea;
		using (CollectionPool<Dictionary<AbstractUnitEntity, FollowerActionType>, KeyValuePair<AbstractUnitEntity, FollowerActionType>>.Get(out value))
		{
			List<AbstractUnitEntity> list = TempList.Get<AbstractUnitEntity>();
			leaderNodeArea = ((!alwaysTeleport) ? ObstacleAnalyzer.GetArea(position) : uint.MaxValue);
			foreach (AbstractUnitEntity follower in leader.Followers)
			{
				if (!ShouldSkipProcessing(follower, isCutsceneCommand))
				{
					list.Add(follower);
					value[follower] = ActionType(follower);
					leader.FollowerDesiredActions.Remove(follower);
				}
			}
			Vector3 followersFrontPosition = GetFollowersFrontPosition(leader.Owner, position, orientation);
			foreach (List<AbstractUnitEntity> item in list.Slice(20))
			{
				PrepareFormation(leader, item, followersFrontPosition, value);
			}
			Dictionary<AbstractUnitEntity, FollowerAction> dictionary = new Dictionary<AbstractUnitEntity, FollowerAction>();
			foreach (AbstractUnitEntity item2 in list)
			{
				dictionary[item2] = leader.FollowerDesiredActions[item2];
			}
			List<AbstractUnitEntity> list2 = TempList.Get<AbstractUnitEntity>();
			foreach (AbstractUnitEntity independentFollower in leader.IndependentFollowers)
			{
				if (!ShouldSkipProcessing(independentFollower, isCutsceneCommand))
				{
					list2.Add(independentFollower);
					Quaternion orientationQuaternion = GetOrientationQuaternion(leader.Owner);
					CreateIndependentFollowerAction(independentFollower, leader, position, orientationQuaternion, ActionType(independentFollower));
					dictionary[independentFollower] = leader.FollowerDesiredActions[independentFollower];
				}
			}
			return dictionary;
		}
		FollowerActionType ActionType(AbstractUnitEntity follower)
		{
			if (!alwaysTeleport)
			{
				return GetActionType(follower, leaderNodeArea);
			}
			return FollowerActionType.Teleport;
		}
	}

	private static bool ShouldSkipProcessing(AbstractUnitEntity follower, bool isCutsceneCommand = false)
	{
		if (!follower.IsInGame || follower.LifeState.State == UnitLifeState.Dead)
		{
			return true;
		}
		UnitPartFollowUnit optional = follower.GetOptional<UnitPartFollowUnit>();
		if (optional == null)
		{
			return false;
		}
		if (!isCutsceneCommand && !optional.FollowWhileCutscene && follower.CutsceneControlledUnit?.GetCurrentlyActive() != null)
		{
			return true;
		}
		if (!optional.FollowInCombat)
		{
			return optional.Leader.IsInCombat;
		}
		return false;
	}

	private static void PrepareFormation(UnitPartFollowedByUnits leader, IList<AbstractUnitEntity> followers, Vector3 position, Dictionary<AbstractUnitEntity, FollowerActionType> desiredActions)
	{
		if (!followers.Empty())
		{
			FollowersFormation followersFormation = Game.Instance.BlueprintRoot.Formations.FollowersFormation;
			List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
			list.Add(leader.Owner);
			Span<Vector3> resultPositions = stackalloc Vector3[followers.Count];
			PartyFormationHelper.FillFormationPositions(position, FormationAnchor.Front, ClickGroundHandler.GetDirection(position, list), followers, followers, followersFormation, resultPositions, -1, leader.Owner.GetNearestNodeXZ());
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

	private static Vector3 GetFollowersFrontPosition(AbstractUnitEntity leader, Vector3? forcePosition = null, float? forceOrientation = null)
	{
		Vector2 playerOffset = Game.Instance.BlueprintRoot.Formations.FollowersFormation.PlayerOffset;
		Vector3 obj = forcePosition ?? GetUnitDestination(leader);
		Quaternion quaternion = (forceOrientation.HasValue ? GetOrientationQuaternion(forceOrientation.Value) : GetOrientationQuaternion(leader));
		return obj + quaternion * new Vector3(playerOffset.x, 0f, playerOffset.y);
	}

	private static Quaternion GetOrientationQuaternion(AbstractUnitEntity unit)
	{
		return GetOrientationQuaternion(unit.Commands.CurrentMoveTo?.Orientation ?? unit.Orientation);
	}

	private static Quaternion GetOrientationQuaternion(float orientation)
	{
		return Quaternion.Euler(0f, orientation, 0f);
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

	public static void CreateIndependentFollowerAction(AbstractUnitEntity follower, UnitPartFollowedByUnits leader, Vector3 leaderPosition, Quaternion leaderOrientation, FollowerActionType type)
	{
		NNInfo nearestNode = ObstacleAnalyzer.GetNearestNode(leaderPosition);
		if (nearestNode.node == null)
		{
			nearestNode = ObstacleAnalyzer.GetNearestNode(leaderPosition, null, ObstacleAnalyzer.UnwalkableXZConstraint);
		}
		Vector3 position = nearestNode.position;
		Vector3 vector = follower.GetOptional<UnitPartFollowUnit>().FollowingSettings.GetOffset(0, follower).To3D();
		Vector3 end = position + leaderOrientation * vector;
		Linecast.LinecastGrid(nearestNode.node.Graph, position, end, nearestNode.node, out var hit, ObstacleAnalyzer.DefaultXZConstraint, ref Linecast.HasConnectionTransition.Instance);
		Vector3 position2 = ObstacleAnalyzer.FindClosestPointToStandOn(hit.point, hint: (CustomGridNodeBase)hit.node, corpulence: follower.MovementAgent.Corpulence);
		FollowerAction value = new FollowerAction(position2, null, type);
		leader.FollowerDesiredActions[follower] = value;
	}
}

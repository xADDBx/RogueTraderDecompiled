using System.Linq;
using Code.Visual.Animation;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitFollowUnitController : BaseUnitController
{
	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		UnitPartFollowedByUnits optional = unit.GetOptional<UnitPartFollowedByUnits>();
		if (optional == null)
		{
			return;
		}
		foreach (AbstractUnitEntity activeFollower in optional.GetActiveFollowers())
		{
			if (!activeFollower.IsInGame)
			{
				continue;
			}
			UnitPartFollowUnit optional2 = activeFollower.GetOptional<UnitPartFollowUnit>();
			if (optional2 != null && (optional2.FollowWhileCutscene || activeFollower.CutsceneControlledUnit?.GetCurrentlyActive() == null) && (!activeFollower.IsInCombat || !optional2.Leader.IsInCombat))
			{
				FollowerAction? followerAction = optional.GetFollowerAction(activeFollower);
				if (followerAction.HasValue)
				{
					Tick(activeFollower, followerAction.Value, optional2);
				}
			}
		}
	}

	private static void Tick(AbstractUnitEntity unit, FollowerAction action, UnitPartFollowUnit followerPart)
	{
		BaseUnitEntity leader = followerPart.Leader;
		FollowerActionType type = action.Type;
		Vector3 position = action.Position;
		UnitMoveTo currentMoveTo = unit.Commands.CurrentMoveTo;
		bool alwaysRun = followerPart.AlwaysRun;
		if (currentMoveTo != null)
		{
			HandleMoveCommand(unit, currentMoveTo, leader, alwaysRun);
		}
		if (unit is BaseUnitEntity baseUnitEntity && baseUnitEntity.Faction.AttackFactions.Any())
		{
			if (leader.IsInCombat && !unit.IsInCombat)
			{
				baseUnitEntity.CombatState.JoinCombat();
				baseUnitEntity.CombatGroup.Group.IsFollowingUnitInCombat = true;
			}
			if (!leader.IsInCombat && leader.LifeState.IsConscious && unit.IsInCombat)
			{
				baseUnitEntity.CombatState.LeaveCombat();
				baseUnitEntity.CombatGroup.Group.IsFollowingUnitInCombat = false;
			}
		}
		if (!unit.IsInCombat && ShouldAct(unit, position))
		{
			float orientation = action.Orientation;
			if (type == FollowerActionType.Teleport)
			{
				Teleport(unit, action.Position, orientation);
			}
			else
			{
				Move(unit, action.Position, orientation, alwaysRun);
			}
		}
	}

	private static bool ShouldAct(AbstractUnitEntity follower, Vector3 desiredPosition)
	{
		bool result = false;
		float num = Mathf.Pow(Game.Instance.BlueprintRoot.Formations.FollowersFormation.RepathDistance, 2f);
		UnitMoveTo currentMoveTo = follower.Commands.CurrentMoveTo;
		if (currentMoveTo != null)
		{
			if (currentMoveTo.IsStarted && !currentMoveTo.IsFinished && !follower.View.MovementAgent.WantsToMove)
			{
				result = true;
			}
			if (GeometryUtils.SqrDistance2D(currentMoveTo.ApproachPoint, desiredPosition) > num)
			{
				result = true;
			}
		}
		else
		{
			result = GeometryUtils.SqrDistance2D(desiredPosition, follower.Position) > num;
		}
		return result;
	}

	private static void HandleMoveCommand(AbstractUnitEntity unit, UnitMoveTo move, BaseUnitEntity leader, bool alwaysRun)
	{
		if (move.Result == AbstractUnitCommand.ResultType.Interrupt || !(unit.Movable.CurrentSpeedMps < leader.Movable.CurrentSpeedMps))
		{
			return;
		}
		UnitPartFollowUnit optional = unit.GetOptional<UnitPartFollowUnit>();
		if (optional == null || !optional.CanBeSlowerThanLeader)
		{
			move.Params.OverrideSpeed = leader.Movable.CurrentSpeedMps;
			if (alwaysRun)
			{
				move.Params.OverrideSpeed *= 1.2f;
			}
		}
	}

	private static void Teleport(AbstractUnitEntity unit, Vector3 targetPoint, float orientation)
	{
		unit.Position = targetPoint;
		unit.DesiredOrientation = orientation;
		EntityFader entityFader = unit.View?.Fader;
		if ((bool)entityFader)
		{
			entityFader.Visible = false;
			entityFader.FastForward();
			entityFader.Visible = unit.View.IsVisible;
		}
	}

	private static void Move(AbstractUnitEntity unit, Vector3 targetPoint, float orientation, bool canRun)
	{
		PathfindingService.Instance.FindPathRT_Delayed(unit.MovementAgent, targetPoint, 0.1f, 1, delegate(ForcedPath path)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error($"An error path was returned. Ignoring. Unit: {unit}");
			}
			else
			{
				UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, targetPoint, 0.1f)
				{
					Orientation = orientation,
					AiCanInterruptMark = true
				};
				if (canRun)
				{
					unitMoveToParams.MovementType = WalkSpeedType.Run;
				}
				unit.Commands.Run(unitMoveToParams);
			}
		});
	}
}

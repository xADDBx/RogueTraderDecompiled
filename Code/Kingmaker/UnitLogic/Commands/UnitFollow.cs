using System.Collections.Generic;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitFollow : UnitCommand<UnitFollowParams>
{
	private readonly struct TargetEntityMovementData
	{
		private readonly UnitMovementAgentBase m_MovementAgent;

		private readonly UnitMoveContinuously m_MoveCommand;

		public bool IsMoving => m_MovementAgent?.IsReallyMoving ?? false;

		public bool IsPositionChanged => m_MovementAgent.IsPositionChanged;

		public float? MaxSpeedOverride
		{
			get
			{
				if (!IsMoving)
				{
					return null;
				}
				return m_MovementAgent.Speed;
			}
		}

		public WalkSpeedType? MovementType => m_MoveCommand?.MovementType;

		public Vector2? Direction => m_MoveCommand?.Params.Direction;

		public float? Multiplier => m_MoveCommand?.Params.Multiplier;

		public TargetEntityMovementData(MechanicEntity entity)
		{
			m_MovementAgent = entity.MaybeMovementAgent;
			m_MoveCommand = entity.GetOptional<PartUnitCommands>()?.CurrentMoveContinuously;
		}
	}

	private const float MatchMovementInnerRadiusSquared = 2f;

	private const float MatchMovementOuterRadiusSquared = 4f;

	private const float SprintDestinationRadiusSquared = 20f;

	private const float StopMovementRadius = 0.2f;

	private const float RepathTimeThreshold = 0.3f;

	private const float RepathDistanceThreshold = 1f;

	private float m_RepathTimer = float.MaxValue;

	private Vector3 m_RememberedDestination;

	public override bool IsMoveUnit => true;

	private Vector3 Destination => base.Params.Destination;

	private bool HasReachedDestination => (base.Executor.Position - Destination).sqrMagnitude < 0.02f;

	public UnitFollow([NotNull] UnitFollowParams @params)
		: base(@params)
	{
	}

	protected override void OnTick()
	{
		MechanicEntity mechanicEntity = base.Target?.Entity;
		if (mechanicEntity == null)
		{
			ForceFinish(ResultType.Fail);
			return;
		}
		TargetEntityMovementData targetData = new TargetEntityMovementData(mechanicEntity);
		if (!targetData.IsMoving && !base.Executor.MovementAgent.WantsToMove)
		{
			ForceFinish(ResultType.Success);
			return;
		}
		base.Params.MovementType = ((!((base.Executor.Position - Destination).sqrMagnitude > 20f)) ? WalkSpeedType.Run : WalkSpeedType.Sprint);
		if (ShouldMatchTargetMovement(targetData))
		{
			if (targetData.IsPositionChanged)
			{
				if (!base.Params.IsGamepadMovement && HasReachedDestination)
				{
					ForceFinish(ResultType.Success);
					return;
				}
				m_RepathTimer = float.MaxValue;
				MatchTargetMovement(targetData);
			}
		}
		else
		{
			ResetMoveAgentOverride();
			m_RepathTimer += Game.Instance.RealTimeController.SystemDeltaTime;
			if (ShouldRepath(targetData))
			{
				Repath();
			}
		}
	}

	private bool ShouldMatchTargetMovement(TargetEntityMovementData targetData)
	{
		if (!targetData.IsMoving)
		{
			return false;
		}
		float sqrMagnitude = (base.Executor.Position - Destination).sqrMagnitude;
		float num = (IsMovementAgentOverridden() ? 4f : 2f);
		return sqrMagnitude < num;
	}

	private void MatchTargetMovement(TargetEntityMovementData targetData)
	{
		UnitMovementAgentContinuous unitMovementAgentContinuous = SetMoveAgentContinuous();
		unitMovementAgentContinuous.MaxSpeedOverride = targetData.MaxSpeedOverride;
		unitMovementAgentContinuous.DirectionFromController = targetData.Direction ?? (Destination - base.Executor.Position).normalized.To2D();
		unitMovementAgentContinuous.DirectionFromControllerMagnitude = targetData.Multiplier ?? 1f;
		unitMovementAgentContinuous.m_Acceleration = 200f;
		base.Params.MovementType = targetData.MovementType ?? WalkSpeedType.Walk;
	}

	private bool ShouldRepath(TargetEntityMovementData targetData)
	{
		if (!targetData.IsMoving || base.Executor.MovementAgent.WantsToMove)
		{
			if ((m_RememberedDestination - Destination).sqrMagnitude > 1f)
			{
				return m_RepathTimer > 0.3f;
			}
			return false;
		}
		return true;
	}

	private void Repath()
	{
		m_RepathTimer = 0f;
		m_RememberedDestination = Destination;
		BaseUnitEntity executor = base.Executor;
		PathfindingService.Instance.FindPathRT_Delayed(base.Executor.MovementAgent, Destination, 0f, 1, delegate(ForcedPath path)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else if (path.path == null || path.path.Count == 0)
			{
				PFLog.Pathfinding.Error(((path.path == null) ? "Path is null" : "Path is empty") + ". Ignoring");
			}
			else
			{
				Vector3 destination = ((ObstacleAnalyzer.GetArea(Destination) == path.path[0].Area) ? Destination : TrimPathToCurrentArea(path));
				executor.View.MoveTo(path, destination, 0.2f);
			}
		});
	}

	private static Vector3 TrimPathToCurrentArea(ForcedPath path)
	{
		uint area = path.path[0].Area;
		int num = path.vectorPath.Count;
		while (num > 0 && ObstacleAnalyzer.GetArea(path.vectorPath[num - 1]) != area)
		{
			num--;
		}
		path.vectorPath.RemoveRange(num, path.vectorPath.Count - num);
		if (path.vectorPath == null || path.vectorPath.Count == 0)
		{
			return path.path[0].Vector3Position;
		}
		List<Vector3> vectorPath = path.vectorPath;
		return vectorPath[vectorPath.Count - 1];
	}

	private UnitMovementAgentContinuous SetMoveAgentContinuous()
	{
		UnitMovementAgentContinuous unitMovementAgentContinuous = base.Executor.View.gameObject.GetComponent<UnitMovementAgentContinuous>();
		if (unitMovementAgentContinuous == null)
		{
			unitMovementAgentContinuous = base.Executor.View.gameObject.AddComponent<UnitMovementAgentContinuous>();
			base.Executor.View.AgentOverride = unitMovementAgentContinuous;
			base.Executor.View.AgentOverride.Init(base.Executor.View.gameObject);
		}
		else
		{
			base.Executor.View.AgentOverride = unitMovementAgentContinuous;
		}
		return unitMovementAgentContinuous;
	}

	private void ResetMoveAgentOverride()
	{
		if (base.Executor?.View?.AgentOverride != null)
		{
			base.Executor.View.AgentOverride.Blocker.Unblock();
			base.Executor.View.AgentOverride = null;
		}
	}

	private bool IsMovementAgentOverridden()
	{
		return base.Executor?.View.Or(null)?.AgentOverride != null;
	}

	protected override ResultType OnAction()
	{
		return ResultType.None;
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		ResetMoveAgentOverride();
		base.Executor.MovementAgent.MaxSpeedOverride = null;
		if (base.Target != null)
		{
			base.Executor.DesiredOrientation = base.Target.Orientation;
		}
	}
}

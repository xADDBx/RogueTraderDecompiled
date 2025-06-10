using System;
using System.Collections.Generic;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[TypeId("dfbf27283a9d4924cb6f705e71ffe1a9")]
public class CommandMoveUnit : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;

		[CanBeNull]
		public UnitCommandHandle CommandHandle;

		public bool TakingTooLong;

		public Path Path;

		public Exception ExceptionToThrow;

		[CanBeNull]
		public AbstractUnitCommand Command => CommandHandle?.Cmd;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeReference]
	[WorkspaceSecondTarget]
	public PositionEvaluator Target;

	public float ApproachRadius;

	[SerializeReference]
	public FloatEvaluator Orientation;

	public WalkSpeedType Animation = WalkSpeedType.Walk;

	public bool OverrideSpeed;

	[ConditionalShow("OverrideSpeed")]
	public float Speed = 5f;

	public bool DisableAvoidance = true;

	[Tooltip("Unit will   when reach the target")]
	public bool RunAway;

	[SerializeField]
	[Tooltip("Timeout in case something breaks, forces this command to stop after this many seconds")]
	private float m_Timeout = 20f;

	[SerializeField]
	[Tooltip("Hotfix WH-118691")]
	private bool m_SnapToGridInCombat;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = Unit.GetValue();
		Vector3 targetPosition = Target.GetValue();
		float value;
		float? orientation = ((Orientation != null && Orientation.TryGetValue(out value)) ? new float?(value) : null);
		if (commandData.Unit == null)
		{
			throw new Exception($"Unit {commandData.Unit} not found");
		}
		if ((commandData.Unit.Position - targetPosition).magnitude < ApproachRadius)
		{
			commandData.TakingTooLong = true;
			return;
		}
		commandData.Path = PathfindingService.Instance.FindPathRT_Delayed(commandData.Unit.MovementAgent, targetPosition, 0.3f, 1, delegate(ForcedPath path)
		{
			commandData.Path = null;
			if (path.error)
			{
				commandData.ExceptionToThrow = new Exception("An error path was returned. Ignoring. Unit [" + commandData.Unit?.UniqueId + ", '" + commandData.Unit?.Blueprint?.Name + "']");
			}
			else if (commandData.Unit == null || commandData.Unit.IsDisposed)
			{
				path.Claim(this);
				path.Release(this);
				commandData.ExceptionToThrow = new Exception("Unit was disposed while searching for path");
			}
			else
			{
				float num = ApproachRadius;
				while (num > 0f && path.vectorPath.Count > 1 && path.path.Count > 0)
				{
					List<Vector3> vectorPath = path.vectorPath;
					Vector3 vector = vectorPath[vectorPath.Count - 1];
					List<Vector3> vectorPath2 = path.vectorPath;
					Vector3 vector2 = vector - vectorPath2[vectorPath2.Count - 2];
					float magnitude = vector2.magnitude;
					if (magnitude > num)
					{
						List<Vector3> vectorPath3 = path.vectorPath;
						int index = vectorPath3.Count - 1;
						List<Vector3> vectorPath4 = path.vectorPath;
						vectorPath3[index] = vectorPath4[vectorPath4.Count - 2] + vector2.normalized * (magnitude - num);
						List<Vector3> vectorPath5 = path.vectorPath;
						targetPosition = vectorPath5[vectorPath5.Count - 1];
						num = 0f;
					}
					else
					{
						num -= magnitude;
						List<Vector3> vectorPath6 = path.vectorPath;
						targetPosition = vectorPath6[vectorPath6.Count - 2];
						path.vectorPath.RemoveAt(path.vectorPath.Count - 1);
						path.path.RemoveAt(path.path.Count - 1);
					}
				}
				UnitMoveToParams cmdParams = new UnitMoveToParams(path, targetPosition)
				{
					MovementType = ((Animation == WalkSpeedType.Sprint) ? WalkSpeedType.Walk : Animation),
					Orientation = orientation,
					OverrideSpeed = (OverrideSpeed ? new float?(Speed) : null),
					RunAway = RunAway
				};
				commandData.CommandHandle = commandData.Unit.Commands.Run(cmdParams);
				if (DisableAvoidance)
				{
					commandData.Unit.View.MovementAgent.AvoidanceDisabled = true;
				}
				if (skipping)
				{
					commandData.TakingTooLong = true;
				}
			}
		});
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		AbstractUnitEntity unit = commandData.Unit;
		AbstractUnitCommand command = commandData.Command;
		if (command != null && command.Result == AbstractUnitCommand.ResultType.Fail)
		{
			commandData.TakingTooLong = true;
		}
		if (commandData.TakingTooLong)
		{
			unit?.View.MovementAgent.Stop();
			unit?.View.MovementAgent.Blocker.BlockAtCurrentPosition();
			Vector3? vector = commandData.CommandHandle?.Target?.Point;
			Vector3 valueOrDefault = vector.GetValueOrDefault();
			if (!vector.HasValue)
			{
				valueOrDefault = Target.GetValue();
				vector = valueOrDefault;
			}
			unit?.Translocate(vector.Value, null);
			return true;
		}
		if (commandData.Path != null)
		{
			return false;
		}
		if (unit != null && !unit.IsDisposed && unit.LifeState.IsConscious && unit.IsInGame && unit.IsInState)
		{
			return !unit.Commands.Contains(commandData.Command);
		}
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		Exception exceptionToThrow = commandData.ExceptionToThrow;
		if (exceptionToThrow != null)
		{
			commandData.ExceptionToThrow = null;
			throw new Exception("Re-throwing path exception", exceptionToThrow);
		}
		if (time > (double)m_Timeout)
		{
			player.GetCommandData<Data>(this).TakingTooLong = true;
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		player.GetCommandData<Data>(this).TakingTooLong = true;
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		AbstractUnitEntity unit = commandData.Unit;
		if (unit == null)
		{
			return;
		}
		commandData.Command?.Interrupt();
		if ((bool)unit.View)
		{
			if (DisableAvoidance)
			{
				unit.View.MovementAgent.AvoidanceDisabled = false;
			}
			commandData.Unit?.View.MovementAgent.Blocker.BlockAtCurrentPosition();
		}
		if (m_SnapToGridInCombat && Game.Instance.TurnController.TbActive)
		{
			(unit as UnitEntity)?.SnapToGrid();
		}
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!Unit || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override string GetCaption()
	{
		return Unit?.GetCaptionShort() + " <b>move</b> to " + (Target ? Target.GetCaptionShort() : "???");
	}

	public override string GetWarning()
	{
		if ((bool)Unit && Unit.CanEvaluate())
		{
			return null;
		}
		return "No unit";
	}
}

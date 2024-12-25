using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.PathRenderer;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitMoveToProper : UnitCommand<UnitMoveToProperParams>
{
	[JsonProperty]
	public float MovePointsSpent { get; private set; }

	public bool DisableAttackOfOpportunity => base.Params.DisableAttackOfOpportunity;

	private float ActionPointsPerCell => base.Params.ActionPointsPerCell;

	private int ActionPointsToSpend => base.Params.ActionPointsToSpend;

	private float[] ApCostPerEveryCell => base.Params.ApCostPerEveryCell;

	private int StraightMoveLength => base.Params.StraightMoveLength;

	private int DiagonalsCount => base.Params.DiagonalsCount;

	public override bool ShouldTurnToTarget => false;

	public override bool IsInterruptible => false;

	public override bool AwaitMovementFinish => true;

	public override bool IsMoveUnit => true;

	public override bool CanStart
	{
		get
		{
			if (base.Params.IsSynchronized && base.Executor.IsInCombat)
			{
				if (base.ForcedPath == null)
				{
					PFLog.Default.Error("Something bad happened, ForcePath==null. returning CanStart=false, this fixes progress block after moving out from invalid state (ship was in occupied cell)");
					return false;
				}
				float num = 0.5f * GraphParamsMechanicsCache.GridCellSize;
				Vector3 vector = base.ForcedPath.vectorPath.Get(0);
				Vector3 position = base.Executor.Position;
				if (num * num < (vector - position).sqrMagnitude)
				{
					PFLog.Default.Error($"UnitMoveToProper cannot be started! Executor is too far from the starting point. Start={vector} current={position}");
					return false;
				}
			}
			return true;
		}
	}

	public UnitMoveToProper([NotNull] UnitMoveToProperParams @params)
		: base(@params)
	{
	}

	public override void Tick()
	{
		using (ProfileScope.New("OnTick"))
		{
			OnTick();
		}
	}

	protected override ResultType OnAction()
	{
		return ResultType.None;
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (!IsPathValid(base.ForcedPath))
		{
			PFLog.Default.Error("UnitMoveToProper failed to start. Path is null or empty.");
			ForceFinish(ResultType.Fail);
			return;
		}
		if (!base.Executor.CanMove)
		{
			PFLog.Default.Error("UnitMoveToProper failed to start. Executor cannot move.");
			ForceFinish(ResultType.Fail);
			return;
		}
		base.Executor.View.MovementAgent.ForcePath(base.ForcedPath, base.Params.DisableApproachRadius);
		float a = CalculatePathCost();
		MovePointsSpent = Mathf.Min(a, base.Executor.CombatState.ActionPointsBlue);
		PartUnitCombatState combatState = base.Executor.CombatState;
		float? blue = MovePointsSpent;
		combatState.SpendActionPoints(null, blue);
		base.Executor.CombatState.LastStraightMoveLength = StraightMoveLength;
		base.Executor.CombatState.LastDiagonalCount = ((DiagonalsCount != 0) ? DiagonalsCount : (base.Executor.CombatState.LastDiagonalCount + base.ForcedPath.DiagonalsCount()));
		using (ProfileScope.New("HandleUnitCommandDidAct"))
		{
			EventBus.RaiseEvent((IMechanicEntity)base.Executor, (Action<IUnitCommandActHandler>)delegate(IUnitCommandActHandler h)
			{
				h.HandleUnitCommandDidAct(this);
			}, isCheckRuntime: true);
		}
		if (base.Executor.IsDirectlyControllable)
		{
			UnitPathManager.Instance.AddPath(base.Executor, base.ForcedPath, ActionPointsPerCell, MovePointsSpent, DiagonalsCount % 2 == 1, ApCostPerEveryCell);
		}
	}

	public float CalculatePathCost(BaseUnitEntity executor)
	{
		if (ApCostPerEveryCell == null)
		{
			return ActionPointsPerCell * (float)base.ForcedPath.LengthInCells(executor.CombatState.LastDiagonalCount % 2 == 1) + (float)ActionPointsToSpend;
		}
		float num = 0f;
		for (int i = 0; i < base.ForcedPath.vectorPath.Count; i++)
		{
			if (i >= ApCostPerEveryCell.Length)
			{
				PFLog.Default.Warning($"UnitMoveToProper: for {executor.CharacterName} can't calculate cell {i} cost, use default value {ActionPointsPerCell}");
				num += ActionPointsPerCell;
			}
			else
			{
				num += ApCostPerEveryCell[i];
			}
		}
		return num;
	}

	private float CalculatePathCost()
	{
		return CalculatePathCost(base.Executor);
	}

	protected override void OnTick()
	{
		base.OnTick();
		if (!base.Executor.CanMove)
		{
			base.Executor.SnapToGrid();
			ForceFinish(ResultType.Fail);
		}
		else if (!base.Executor.View.MovementAgent.WantsToMove || !base.Executor.View.MovementAgent.IsReallyMoving)
		{
			ForceFinish(ResultType.Success);
		}
	}

	protected override void OnEnded()
	{
		Vector3 position = base.Executor.Position;
		if (!base.Executor.State.IsProne && base.Result != ResultType.Fail)
		{
			if (base.ForcedPath.vectorPath.Count > 0)
			{
				List<Vector3> vectorPath = base.ForcedPath.vectorPath;
				position = vectorPath[vectorPath.Count - 1];
			}
			else
			{
				PFLog.Default.Error("UnitMoveToProper: for " + base.Executor.CharacterName + " forced path got lost. Taking current position as last.");
			}
		}
		base.OnEnded();
		GraphNode node = AstarPath.active.GetNearest(position).node;
		if (!WarhammerBlockManager.Instance.NodeContainsInvisibleAnyExcept(node, base.Executor.View.MovementAgent.Blocker))
		{
			base.Executor.Position = position;
			Vector3 forward = CustomGraphHelper.AdjustDirection(base.Executor.MovementAgent.FinalDirection);
			base.Executor.SetOrientation(Quaternion.LookRotation(forward).eulerAngles.y);
		}
		UnitPredictionManager.Instance.ClearHologram(base.Executor);
		UnitPathManager.Instance.RemovePath(base.Executor);
		base.Executor.View.MovementAgent.Blocker.BlockAtCurrentPosition();
		base.Executor.View.MovementAgent.Stop();
		EventBus.RaiseEvent(delegate(IUnitMoveToProperHandler h)
		{
			h.HandleUnitMoveToProper(this);
		});
	}

	private bool IsPathValid(Path path)
	{
		if (path?.vectorPath != null)
		{
			return path.vectorPath.Count > 0;
		}
		return false;
	}
}
